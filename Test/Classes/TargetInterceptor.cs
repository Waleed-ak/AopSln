using System;
using System.Threading.Tasks;

namespace Tools
{
  public class TargetInterceptor:AopInterceptor
  {
    #region Public Properties
    public object CurrentObject { get; set; } = new Target();
    #endregion Public Properties

    #region Protected Methods

    protected override void InvokeMethod(string key,ItemMethod item,object[] args,Type[] types)
    {
      RunMethod(key,CurrentObject,args,types);
    }

    protected override T InvokeMethod<T>(string key,ItemMethod item,object[] args,Type[] types)
    {
      var obj = RunMethod<T>(key,CurrentObject,args,types);

      return obj;
    }

    protected override async Task InvokeMethodAsync(string key,ItemMethod item,object[] args,Type[] types)
    {
      await RunMethodAsync(key,CurrentObject,args,types).ConfigureAwait(false);
    }

    protected override async Task<T> InvokeMethodAsync<T>(string key,ItemMethod item,object[] args,Type[] types)
    {
      var res = await RunMethodAsync<T>(key,CurrentObject,args,types).ConfigureAwait(false);

      return res;
    }

    protected override T InvokePropertyGet<T>(string key,ItemProperty item,object[] args)
    {
      var res = RunPropertyGet<T>(key,CurrentObject,args);

      return res;
    }

    protected override void InvokePropertySet<T>(string key,ItemProperty item,T value,object[] args)
    {
      RunPropertySet(key,CurrentObject,value,args);
    }

    #endregion Protected Methods
  }
}
