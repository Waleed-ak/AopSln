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

    protected override void InvokeMethodAction(string key,ItemMethod item,object[] args,Type[] types)
    {
      RunMethodAction(key,CurrentObject,args,types);
    }

    protected override async Task InvokeMethodActionAsync(string key,ItemMethod item,object[] args,Type[] types)
    {
      await RunMethodActionAsync(key,CurrentObject,args,types).ConfigureAwait(false);
    }

    protected override T InvokeMethodFunc<T>(string key,ItemMethod item,object[] args,Type[] types)
    {
      var obj = RunMethodFunc<T>(key,CurrentObject,args,types);

      return obj;
    }

    protected override async Task<T> InvokeMethodFuncAsync<T>(string key,ItemMethod item,object[] args,Type[] types)
    {
      var res = await RunMethodFuncAsync<T>(key,CurrentObject,args,types).ConfigureAwait(false);

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
