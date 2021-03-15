using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tools
{
  public class TargetTemp:ITemp
  {
    #region Private Fields
    private readonly AopInterceptor _Interceptor;
    #endregion Private Fields

    #region Public Constructors

    public TargetTemp(AopInterceptor interceptor)
    {
      _Interceptor = interceptor;
      _Interceptor.Init<ITemp>();
    }

    #endregion Public Constructors

    #region Public Properties
    public int Age => _Interceptor._PropertyGet<int>(key: "Age",Array.Empty<object>());

    public string Name
    {
      get => _Interceptor._PropertyGet<string>(key: "Name",Array.Empty<object>());
      set => _Interceptor._PropertySet(key: "Name",value,Array.Empty<object>());
    }

    #endregion Public Properties

    #region Public Indexers

    public int this[int i]
    {
      get => _Interceptor._PropertyGet<int>(key: "Item",new object[] { i });
      set => _Interceptor._PropertySet(key: "Item",value,new object[] { i });
    }

    #endregion Public Indexers

    #region Public Methods

    public int Add(int a,int b) => _Interceptor._Func<int>("int Add(int,int)",new object[] { a,b },Type.EmptyTypes);

    public void Add(int a,int b,out double d)
    {
      d = default;
      var args = new object[] { a,b,d };
      _Interceptor._Action("void Add(int,int,out double)",args,Type.EmptyTypes);
      d = (double)args[2];
    }

    public Task<int> AddAsync(int a,int b) => _Interceptor._FuncAsync<int>("Task<int> AddAsync(int,int)",new object[] { a,b },Type.EmptyTypes);

    public override int GetHashCode() => _Interceptor._Func<int>("int GetHashCode()",Array.Empty<object>(),Type.EmptyTypes);

    public void Print(string a) => _Interceptor._Action("void Print(string)",new object[] { a },Type.EmptyTypes);

    public Task PrintAsync(string a) => _Interceptor._ActionAsync("Task PrintAsync(string)",new object[] { a },Type.EmptyTypes);

    public void Test(List<int> a,int b,out DateTime d)
    {
      d = default;
      var args = new object[] { a,b,d };
      _Interceptor._Action("void Test(List<int>,int,out DateTime)",args,Type.EmptyTypes);
      d = (DateTime)args[2];
    }

    public void TestBool(int a,int b,out bool d)
    {
      d = false;
      var args = new object[] { a,b,d };
      _Interceptor._Action("void TestBool(int,int,out bool)",args,Type.EmptyTypes);
      d = (bool)args[2];
    }

    public void TestDateTime(int a,int b,out DateTime d)
    {
      d = default;
      var args = new object[] { a,b,d };
      _Interceptor._Action("void TestDateTime(int,int,out DateTime)",args,Type.EmptyTypes);
      d = (DateTime)args[2];
    }

    public void TestDouble(int a,int b,out double d)
    {
      d = 0;
      var args = new object[] { a,b,d };
      _Interceptor._Action("void TestDouble(int,int,out double)",args,Type.EmptyTypes);
      d = (double)args[2];
    }

    public void TestTarger(int a,int b,out List<int> d)
    {
      d = default;
      var args = new object[] { a,b,d };
      _Interceptor._Action("void TestTarger(int,int,out List<int>)",args,Type.EmptyTypes);
      d = (List<int>)args[2];
    }

    public void TestTarget(int a,int b,out Target d)
    {
      d = default;
      var args = new object[] { a,b,d };
      _Interceptor._Action("void TestTarget(int,int,out Target)",args,Type.EmptyTypes);
      d = (Target)args[2];
    }

    public override string ToString() => _Interceptor._Func<string>("string ToString()",Array.Empty<object>(),Type.EmptyTypes);

    public void Write<T>(T a) => _Interceptor._Action("void Write<Tin>(Tin)",new object[] { a },new[] { typeof(T) });

    #endregion Public Methods
  }
}
