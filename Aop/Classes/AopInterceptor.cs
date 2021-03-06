using System;
using System.Threading.Tasks;

namespace Tools
{
  public abstract class AopInterceptor
  {
    #region Private Fields
    private Lookups _Lookups = null;
    #endregion Private Fields

    #region Public Methods

    public static T Factory<T>(AopInterceptor interceptor) => (T)Activator.CreateInstance(AopWrapper.CreateType<T>(),new[] { interceptor });

    public static object Factory(Type interFacetype,AopInterceptor interceptor) => Activator.CreateInstance(AopWrapper.CreateType(interFacetype),new[] { interceptor });

    public void _Action(string key,object[] args,Type[] types) => InvokeMethod(key,_Lookups._Methods[key],args,types);

    public async Task _ActionAsync(string key,object[] args,Type[] types) => await InvokeMethodAsync(key,_Lookups._Methods[key],args,types).ConfigureAwait(false);

    public T _Func<T>(string key,object[] args,Type[] types) => InvokeMethod<T>(key,_Lookups._Methods[key],args,types);

    public async Task<T> _FuncAsync<T>(string key,object[] args,Type[] types) => await InvokeMethodAsync<T>(key,_Lookups._Methods[key],args,types).ConfigureAwait(false);

    public T _PropertyGet<T>(string key,object[] args) => InvokePropertyGet<T>(key,_Lookups._Properties[key],args);

    public void _PropertySet<T>(string key,T value,object[] args) => InvokePropertySet(key,_Lookups._Properties[key],value,args);

    public void Init<T>() => _Lookups = LookupSetup<T>.Create();

    #endregion Public Methods

    #region Protected Methods

    protected abstract void InvokeMethod(string key,ItemMethod methodInfo,object[] args,Type[] types);

    protected abstract T InvokeMethod<T>(string key,ItemMethod methodInfo,object[] args,Type[] types);

    protected abstract Task InvokeMethodAsync(string key,ItemMethod methodInfo,object[] args,Type[] types);

    protected abstract Task<T> InvokeMethodAsync<T>(string key,ItemMethod methodInfo,object[] args,Type[] types);

    protected abstract T InvokePropertyGet<T>(string key,ItemProperty propertyInfo,object[] args);

    protected abstract void InvokePropertySet<T>(string key,ItemProperty propertyInfo,T value,object[] args);

    protected void RunMethod(string key,object obj,object[] args,Type[] types) => _ = _Lookups._DelMethod[key].Value(obj,args,types);

    protected T RunMethod<T>(string key,object obj,object[] args,Type[] types) => (T)_Lookups._DelMethod[key].Value(obj,args,types);

    protected Task RunMethodAsync(string key,object obj,object[] args,Type[] types) => (Task)_Lookups._DelTask[key].Value(obj,args,types);

    protected Task<T> RunMethodAsync<T>(string key,object obj,object[] args,Type[] types) => (Task<T>)_Lookups._DelTask[key].Value(obj,args,types);

    protected T RunPropertyGet<T>(string key,object obj,object[] args) => (T)_Lookups._DelPropertyGet[key].Value(obj,args);

    protected void RunPropertySet<T>(string key,object obj,T value,object[] args) => _Lookups._DelPropertySet[key].Value(obj,value,args);

    #endregion Protected Methods
  }
}
