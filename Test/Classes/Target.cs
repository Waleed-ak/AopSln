using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tools
{
  public class Target:ITemp
  {
    #region Private Fields
    private readonly Dictionary<int,int> _Index = new();
    #endregion Private Fields

    #region Public Properties
    public int Age => 10;
    public string Name { get; set; } = "Hi";
    #endregion Public Properties

    #region Public Indexers

    public int this[int i]
    {
      get => (_Index.TryGetValue(i,out var output) ? output : 0) * 10;
      set => _Index[i] = value;
    }

    #endregion Public Indexers

    #region Public Methods

    public int Add(int a,int b) => a + b;

    public void Add(int a,int b,out double d) => d = (double)a + b;

    public async Task<int> AddAsync(int a,int b) => await Task.FromResult(a + b).ConfigureAwait(false);

    public override int GetHashCode() => 5;

    public void Print(string a)
    {
      Console.WriteLine("Hi " + a);
    }

    public Task PrintAsync(string a)
    {
      Console.WriteLine("Hi " + a);
      return Task.FromResult(0);
    }

    public override string ToString() => Name + " " + Age;

    public void Write<T>(T a) => Console.WriteLine(typeof(T).Name + " " + a);

    #endregion Public Methods
  }
}
