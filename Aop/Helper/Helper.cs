using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Tools
{
  public static class Helper
  {
    #region Private Fields

    private static readonly Dictionary<Type,string> _ShorthandMap = new()
    {
      { typeof(bool),"bool" },
      { typeof(byte),"byte" },
      { typeof(char),"char" },
      { typeof(decimal),"decimal" },
      { typeof(double),"double" },
      { typeof(float),"float" },
      { typeof(int),"int" },
      { typeof(long),"long" },
      { typeof(sbyte),"sbyte" },
      { typeof(short),"short" },
      { typeof(string),"string" },
      { typeof(uint),"uint" },
      { typeof(ulong),"ulong" },
      { typeof(ushort),"ushort" },
      { typeof(void),"void" },
    };

    #endregion Private Fields

    #region Public Methods

    public static Task WrapAsync(Action<int> action)
    {
      action(0);
      return Task.FromResult(0);
    }

    public static Task WrapAsync(Func<int,Task> task) => task(0);

    public static Task<T> WrapAsync<T>(Func<int,T> func) => Task.FromResult(func(0));

    public static Task<T> WrapAsync<T>(Func<int,Task<T>> task) => task(0);

    #endregion Public Methods

    #region Internal Methods

    internal static string Key(this MethodInfo method)
    => $"{method.ReturnType.Nice()} {method.Name}{method.Generic()}({string.Join(",",Array.ConvertAll(method.GetParameters(),c => c.GetRefValueType() + c.ParameterType.Nice()))})";

    #endregion Internal Methods

    #region Private Methods

    private static string Generic(this MethodInfo method)
    => method.ContainsGenericParameters ? "<" + string.Join(",",method.GetGenericArguments().Select(c => c.IsGenericType ? Nice(c) : c.Name)) + ">" : "";

    private static string GetRefValueType(this ParameterInfo pi)
      => (pi.IsOut)
      ? "out "
      : (pi.ParameterType.IsByRef) ? "ref " : "";

    private static string Nice(this Type typeIn)
    {
      var type = typeIn;
      if(typeIn.IsByRef)
      {
        type = typeIn.GetElementType();
      }
      if(_ShorthandMap.TryGetValue(type,out var shortName))
      {
        return shortName;
      }
      var friendlyName = type.Name;
      if(friendlyName.Contains("AnonymousType"))
        return "Anonymous";
      if(type.IsGenericType)
      {
        var iBacktick = friendlyName.IndexOf('`');
        if(iBacktick > 0)
        {
          friendlyName = friendlyName.Remove(iBacktick);
        }
        friendlyName += "<" + string.Join(",",type.GetGenericArguments().Select(c => Nice(c))) + ">";
      }
      return friendlyName;
    }

    #endregion Private Methods
  }
}
