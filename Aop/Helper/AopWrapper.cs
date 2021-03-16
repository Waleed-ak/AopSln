using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Tools
{
  public delegate MethodInfo MakeGeneric(Type type);

  public static class AopWrapper
  {
    #region Private Fields
    private const string _AssemblyName = "DynamicAssembly";
    private const TypeAttributes _AttClass = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout;
    private const MethodAttributes _AttMethod = MethodAttributes.Public | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig;
    private const MethodAttributes _AttMethodOverride = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig;
    private const MethodAttributes _AttProperty = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName | MethodAttributes.Virtual | MethodAttributes.Final;
    private const BindingFlags _PublicBindingFlags = BindingFlags.Instance | BindingFlags.Public;
    private static readonly Relax<MethodInfo> _Action = new(() => _TypeAopInterceptor.GetMethod(nameof(AopInterceptor._Action),_PublicBindingFlags));
    private static readonly Relax<MethodInfo> _ActionAsync = new(() => _TypeAopInterceptor.GetMethod(nameof(AopInterceptor._ActionAsync),_PublicBindingFlags));
    private static readonly Relax<AssemblyBuilder> _AssemblyBuilder = new(() => AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(_AssemblyName),AssemblyBuilderAccess.RunAndSave));
    private static readonly Dictionary<Type,Type> _DicType = new();
    private static readonly Relax<MethodInfo> _EmptyObjects = new(() => typeof(Array).GetMethod("Empty",BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(new[] { _TypeObject }));
    private static readonly Relax<FieldInfo> _FieldEmtyTypes = new(() => _TypeType.GetField("EmptyTypes"));

    private static readonly Relax<MakeGeneric> _GenFunc = new(() =>
    {
      var method = _TypeAopInterceptor.GetMethod(nameof(AopInterceptor._Func),_PublicBindingFlags);
      return (x) => method.MakeGenericMethod(new[] { x });
    });

    private static readonly Relax<MakeGeneric> _GenFuncAsync = new(() =>
    {
      var method = _TypeAopInterceptor.GetMethod(nameof(AopInterceptor._FuncAsync),_PublicBindingFlags);
      return (x) => method.MakeGenericMethod(new[] { x.GenericTypeArguments[0] });
    });

    private static readonly Relax<MakeGeneric> _GenInit = new(() =>
        {
          var method = _TypeAopInterceptor.GetMethod(nameof(AopInterceptor.Init));
          return (x) => method.MakeGenericMethod(new[] { x });
        });

    private static readonly Relax<MakeGeneric> _GenPropertyGet = new(() =>
    {
      var method = _TypeAopInterceptor.GetMethod(nameof(AopInterceptor._PropertyGet),_PublicBindingFlags);
      return (x) => method.MakeGenericMethod(new[] { x });
    });

    private static readonly Relax<MakeGeneric> _GenPropertySet = new(() =>
    {
      var method = _TypeAopInterceptor.GetMethod(nameof(AopInterceptor._PropertySet),_PublicBindingFlags);
      return (x) => method.MakeGenericMethod(new[] { x });
    });

    private static readonly Relax<MethodInfo> _Handler = new(() => _TypeType.GetMethod("GetTypeFromHandle",BindingFlags.Public | BindingFlags.Static));
    private static readonly Relax<ModuleBuilder> _ModuleBuilder = new(() => _AssemblyBuilder.Value.DefineDynamicModule(_AssemblyName,_AssemblyName + ".dll",false));

    private static readonly Type _TypeAopInterceptor = typeof(AopInterceptor);
    private static readonly Type _TypeObject = typeof(object);
    private static readonly Type _TypeTask = typeof(Task);
    private static readonly Type _TypeType = typeof(Type);
    private static readonly Type _TypeVoid = typeof(void);
    #endregion Private Fields

    #region Public Methods

    public static void Save() => _AssemblyBuilder.Value.Save(_AssemblyName + ".dll");

    #endregion Public Methods

    #region Internal Methods

    internal static Type CreateType<T>() => CreateType(typeof(T));

    internal static Type CreateType(Type type) => _DicType.TryGetValue(type,out var outType) ? outType : _DicType[type] = CreateTypeUncachedAsync(type).Result;

    internal static bool IsTask(this Type type) => _TypeTask.IsAssignableFrom(type);

    #endregion Internal Methods

    #region Private Methods

    private static Task AddArgs(ILGenerator il,ParameterInfo[] args,bool hasIndexParameters)
    {
      if(hasIndexParameters)
      {
        var length = args.Length;
        il.Emit(OpCodes.Ldc_I4,length);
        il.Emit(OpCodes.Newarr,_TypeObject);
        for(var i = 0;i < length;i++)
        {
          var arg = args[i];
          var ptype = arg.ParameterType;
          il.Emit(OpCodes.Dup);
          il.Emit(OpCodes.Ldc_I4,i);
          il.Emit(OpCodes.Ldarg,i + 1);
          if(ptype.IsValueType || ptype.IsGenericParameter)
          {
            il.Emit(OpCodes.Box,ptype);
          }
          il.Emit(OpCodes.Stelem_Ref);
        }
      }
      else
      {
        il.Emit(OpCodes.Call,_EmptyObjects.Value);
      }
      return Task.CompletedTask;
    }

    private static Task AddConstructorAsync(Type type,TypeBuilder tb,FieldBuilder field)
    {
      var temp = tb.DefineConstructor(MethodAttributes.Public,CallingConventions.Standard,new[] { _TypeAopInterceptor });
      temp.DefineParameter(1,ParameterAttributes.None,"interceptor");
      var il = temp.GetILGenerator();
      il.DeclareLocal(typeof(int));
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Call,_TypeObject.GetConstructor(Type.EmptyTypes));
      // Interceptor = interceptor;
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Ldarg_1);
      il.Emit(OpCodes.Stfld,field);
      // Interceptor.Init<T>();
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Ldfld,field);
      il.Emit(OpCodes.Callvirt,_GenInit.Value(type));
      il.Emit(OpCodes.Ret);
      return Task.FromResult(0);
    }

    private static async Task<MethodBuilder> AddMethodAsync(MethodInfo methodInfo,TypeBuilder tb,FieldBuilder field,MethodAttributes methodAttributes)
    {
      var args = methodInfo.GetParameters();
      var length = args.Length;
      var method = tb.DefineMethod(methodInfo.Name,methodAttributes,methodInfo.ReturnType,Array.ConvertAll(args,c => c.ParameterType));
      if(methodInfo.IsGenericMethod)
      {
        await Helper.WrapAsync(_ =>
        {
          var genericArguments = methodInfo.GetGenericArguments();
          var genericTypeParameters = method.DefineGenericParameters(Array.ConvertAll(genericArguments,a => a.Name));
          for(var i = 0;i < genericArguments.Length;i++)
          {
            genericTypeParameters[i].SetGenericParameterAttributes(genericArguments[i].GetTypeInfo().GenericParameterAttributes);
            var genericConstraints = genericArguments[i].GetTypeInfo().GetGenericParameterConstraints();
            genericTypeParameters[i].SetInterfaceConstraints(genericConstraints.Where(gc => gc.GetTypeInfo().IsInterface).ToArray());
            genericTypeParameters[i].SetBaseTypeConstraint(Array.Find(genericConstraints,t => t.GetTypeInfo().IsClass));
          }
        }).ConfigureAwait(false);
      }

      for(var i = 0;i < length;i++)
      {
        var arg = args[i];
        var param = method.DefineParameter(i + 1,arg.Attributes,arg.Name);
        if((arg.Attributes & ParameterAttributes.HasDefault) > 0)
        {
          param.SetConstant(arg.RawDefaultValue);
        }
      }
      var il = method.GetILGenerator();
      il.DeclareLocal(typeof(object[]));
      if(methodInfo.ReturnType != _TypeVoid)
      {
        il.DeclareLocal(methodInfo.ReturnType);
      }

      for(var i = 0;i < length;i++)
      {
        var arg = args[i];
        if(arg.IsOut)
        {
          il.Emit(OpCodes.Ldarg_S,i + 1);
          var pType = arg.ParameterType;
          var elType = pType.GetElementType();
          if(elType.IsValueType)
          {
            il.Emit(OpCodes.Initobj,elType);
          }
          else
          {
            il.Emit(OpCodes.Initobj,pType);
          }
        }
      }

      il.Emit(OpCodes.Ldc_I4,length);
      il.Emit(OpCodes.Newarr,_TypeObject);
      for(var i = 0;i < length;i++)
      {
        var arg = args[i];
        var ptype = arg.ParameterType;
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Ldc_I4,i);
        il.Emit(OpCodes.Ldarg,i + 1);
        await Helper.WrapAsync(_ =>
         {
           if(arg.IsOut)
           {
             var elType = ptype.GetElementType();
             if(elType.IsValueType)
             {
               il.Emit(OpCodes.Ldobj,elType);
               il.Emit(OpCodes.Box,elType);
             }
             else
             {
               il.Emit(OpCodes.Ldind_Ref);
             }
           }
           else if(ptype.IsValueType || ptype.IsGenericParameter)
           {
             il.Emit(OpCodes.Box,ptype);
           }
         }).ConfigureAwait(false);
        il.Emit(OpCodes.Stelem_Ref);
      }
      il.Emit(OpCodes.Stloc,0);
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Ldfld,field);
      il.Emit(OpCodes.Ldstr,methodInfo.Key());
      il.Emit(OpCodes.Ldloc,0);
      if(methodInfo.IsGenericMethod)
      {
        var GenTypes = method.GetGenericArguments();
        il.Emit(OpCodes.Ldc_I4,GenTypes.Length);
        il.Emit(OpCodes.Newarr,_TypeType);
        for(var i = 0;i < GenTypes.Length;i++)
        {
          var ptype = GenTypes[i];
          il.Emit(OpCodes.Dup);
          il.Emit(OpCodes.Ldc_I4,i);
          il.Emit(OpCodes.Ldtoken,GenTypes[i]);
          il.Emit(OpCodes.Call,_Handler.Value);
        }
        il.Emit(OpCodes.Stelem_Ref);
      }
      else
      {
        il.Emit(OpCodes.Ldsfld,_FieldEmtyTypes.Value);
      }
      var tempMethod = await Helper.WrapAsync(_ =>
      {
        var type = methodInfo.ReturnType;
        return type.IsTask()
          ? (type == _TypeTask) ? _ActionAsync.Value : _GenFuncAsync.Value(type)
          : (methodInfo.ReturnType == _TypeVoid) ? _Action.Value : _GenFunc.Value(type);
      }).ConfigureAwait(false);
      il.Emit(OpCodes.Callvirt,tempMethod);
      if(methodInfo.ReturnType != _TypeVoid)
      {
        il.Emit(OpCodes.Stloc,1);
      }
      for(var i = 0;i < length;i++)
      {
        var arg = args[i];
        if(arg.IsOut)
        {
          var pType = arg.ParameterType;
          var elType = pType.GetElementType();

          il.Emit(OpCodes.Ldarg_S,i + 1);
          il.Emit(OpCodes.Ldloc,0);
          il.Emit(OpCodes.Ldc_I4,i);
          il.Emit(OpCodes.Ldelem_Ref);
          if(elType.IsValueType)
          {
            il.Emit(OpCodes.Unbox_Any,elType);
            il.Emit(OpCodes.Stobj,elType);
          }
          else
          {
            il.Emit(OpCodes.Castclass,pType);
            il.Emit(OpCodes.Stind_Ref);
          }
        }
      }

      if(methodInfo.ReturnType != _TypeVoid)
      {
        il.Emit(OpCodes.Ldloc,1);
      }
      il.Emit(OpCodes.Ret);
      return method;
    }

    private static async Task<MethodBuilder> AddMethodObjectAsync(MethodInfo methodInfo,TypeBuilder tb,FieldBuilder field)
      => await AddMethodAsync(methodInfo,tb,field,_AttMethodOverride).ConfigureAwait(false);

    private static async Task AddMethodsAsync(Type type,TypeBuilder tb,FieldBuilder field)
    {
      var tasks = type.GetMethods().Where(c => !c.IsSpecialName).Select(c => AddMethodAsync(c,tb,field,_AttMethod));
      await Task.WhenAll(tasks).ConfigureAwait(false);
      foreach(var iType in type.GetInterfaces())
      {
        await AddMethodsAsync(iType,tb,field).ConfigureAwait(false);
      }
    }

    private static async Task AddMethodsObjectAsync(TypeBuilder tb,FieldBuilder field)
    {
      var tasks = _TypeObject.GetMethods().Where(c => !c.IsSpecialName).Select(c => AddMethodObjectAsync(c,tb,field));
      await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private static async Task AddPropertiesAsync(Type type,TypeBuilder tb,FieldBuilder field)
    {
      var tasks = type.GetProperties().Select(c => AddPropertyAsync(c,tb,field));
      await Task.WhenAll(tasks).ConfigureAwait(false);
      foreach(var iType in type.GetInterfaces())
      {
        await AddPropertiesAsync(iType,tb,field).ConfigureAwait(false);
      }
    }

    private static async Task AddPropertyAsync(PropertyInfo propertyInfo,TypeBuilder tb,FieldBuilder field)
    {
      var indexParameters = propertyInfo.GetIndexParameters();
      var hasIndexParameters = indexParameters.Length > 0;
      var parameterTypes = hasIndexParameters ? Array.ConvertAll(indexParameters,c => c.ParameterType) : Type.EmptyTypes;
      var temp = tb.DefineProperty(propertyInfo.Name,PropertyAttributes.None,CallingConventions.HasThis,propertyInfo.PropertyType,parameterTypes);

      if(propertyInfo.CanRead)
      {
        //Get
        var get = tb.DefineMethod("get_" + propertyInfo.Name,_AttProperty,propertyInfo.PropertyType,parameterTypes);
        if(hasIndexParameters)
        {
          var i = 0;
          foreach(var item in indexParameters)
          {
            get.DefineParameter(++i,item.Attributes,item.Name);
          }
        }
        var il = get.GetILGenerator();
        il.DeclareLocal(typeof(int));
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld,field);
        il.Emit(OpCodes.Ldstr,propertyInfo.Name);
        await AddArgs(il,indexParameters,hasIndexParameters).ConfigureAwait(false);
        il.Emit(OpCodes.Callvirt,_GenPropertyGet.Value(propertyInfo.PropertyType));
        il.Emit(OpCodes.Ret);
        temp.SetGetMethod(get);
      }
      if(propertyInfo.CanWrite)
      {
        //Set
        var Params = parameterTypes.Concat(new[] { propertyInfo.PropertyType }).ToArray();
        var set = tb.DefineMethod("set_" + propertyInfo.Name,_AttProperty,null,Params);
        var i = 0;
        if(hasIndexParameters)
        {
          foreach(var item in indexParameters)
          {
            set.DefineParameter(++i,item.Attributes,item.Name);
          }
        }
        set.DefineParameter(++i,ParameterAttributes.None,"value");

        var il = set.GetILGenerator();
        il.DeclareLocal(typeof(int));
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld,field);
        il.Emit(OpCodes.Ldstr,propertyInfo.Name);
        il.Emit(OpCodes.Ldarg,indexParameters.Length + 1);
        await AddArgs(il,indexParameters,hasIndexParameters).ConfigureAwait(false);
        il.Emit(OpCodes.Callvirt,_GenPropertySet.Value(propertyInfo.PropertyType));
        il.Emit(OpCodes.Ret);
        temp.SetSetMethod(set);
      }
    }

    private static Task<TypeBuilder> CreateTypeBuilderAsync(Type type)
       => Task.FromResult(_ModuleBuilder.Value.DefineType(
       "Imp" + type.Name,
        _AttClass,
        _TypeObject,
        new Type[] { type }));

    private static async Task<Type> CreateTypeUncachedAsync(Type type)
    {
      if(!type.IsInterface)
        throw new TypeAccessException($"({type.Name}) Should be an Interface !!");
      var tb = await CreateTypeBuilderAsync(type).ConfigureAwait(false);
      var field = tb.DefineField("Interceptor",_TypeAopInterceptor,FieldAttributes.Private | FieldAttributes.InitOnly);
      var tasks = new[] { AddConstructorAsync(type,tb,field),AddPropertiesAsync(type,tb,field),AddMethodsAsync(type,tb,field),AddMethodsObjectAsync(tb,field) };

      await Task.WhenAll(tasks).ConfigureAwait(false);
      return tb.CreateType();
    }

    #endregion Private Methods
  }
}
