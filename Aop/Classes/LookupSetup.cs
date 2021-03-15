using System;
using System.Reflection;

namespace Tools
{
  internal static class LookupSetup<T>
  {
    #region Private Fields
    private static readonly Type _Type = typeof(T);
    private static Lookups _Instance;
    #endregion Private Fields

    #region Public Methods

    public static Lookups Create()
    {
      if(_Instance is null)
      {
        _Instance = new Lookups();
        GetInterfaceMethodsAndProperties(_Type,_Instance);
        GetInterfaceMethodsAndProperties(typeof(object),_Instance);
        var _DelMethod = _Instance._DelMethod;
        var _DelTask = _Instance._DelTask;
        var _DelPropertyGet = _Instance._DelPropertyGet;
        var _DelPropertySet = _Instance._DelPropertySet;
        foreach(var item in _Instance._Properties)
        {
          var pi = item.Value.Property;
          _DelPropertySet[item.Key] = new(() => (a,b,c) => pi.SetValue(a,b,c));
          _DelPropertyGet[item.Key] = new(() => (a,b) => pi.GetValue(a,b));
        }
        foreach(var item in _Instance._Methods)
        {
          var mi = item.Value.Method;
          if(mi.ReturnType.IsTask())
          {
            _DelTask[item.Key] = new(() => BuildMethod(mi));
          }
          else
          {
            _DelMethod[item.Key] = new(() => BuildMethod(mi));
          }
        }
      }
      return _Instance;
    }

    #endregion Public Methods

    #region Private Methods

    private static DelInvoke BuildMethod(MethodInfo methodInfo) => (a,b,c) => GetMethodInfo(methodInfo,c).Invoke(a,b);

    private static void GetInterfaceMethodsAndProperties(Type iType,Lookups instance)
    {
      foreach(MethodInfo mi in iType.GetMethods())
      {
        if(mi.IsSpecialName)
          continue;
        instance._Methods[mi.Key()] = new(mi);
      }
      foreach(PropertyInfo pi in iType.GetProperties())
      {
        instance._Properties[pi.Name] = new(pi);
      }
      foreach(Type IBase in iType.GetInterfaces())
      {
        GetInterfaceMethodsAndProperties(IBase,instance);
      }
    }

    private static MethodInfo GetMethodInfo(MethodInfo methodInfo,Type[] types) => types.Length > 0 ? methodInfo.MakeGenericMethod(types) : methodInfo;

    #endregion Private Methods
  }
}
