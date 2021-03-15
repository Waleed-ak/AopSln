using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Tools
{
  public class BenchMarkRunner
  {
    #region Private Fields
    private readonly List<CaseResult> _Summary = new();
    #endregion Private Fields

    #region Public Methods

    public IEnumerable<CaseResult> AsEnumerable()
    {
      foreach(var item in _Summary)
      {
        yield return item;
      }
    }

    public List<CaseResult> AsList() => _Summary;

    public void Clear() => _Summary.Clear();

    public void Display()
    {
      foreach(var item in _Summary)
      {
        Console.WriteLine($"{item}");
      }
    }

    public void RunCase(Action action,string title,int count = 1)
    {
      var lst = new List<double>();
      var sw = new Stopwatch();

      for(var i = 0;i < count;i++)
      {
        sw.Start();
        action();
        sw.Stop();
        lst.Add(sw.ElapsedTicks);
      }
      var item = new CaseResult
      {
        Title = title,
        Count = count,
        Toltal = TimeSpan.FromTicks((long)lst.Sum(c => c)),
        Min = TimeSpan.FromTicks((long)lst.Min(c => c)),
        Max = TimeSpan.FromTicks((long)lst.Max(c => c)),
      };
      var avgLong = (long)lst.Average(c => c);
      item.Average = TimeSpan.FromTicks(avgLong);
      var stdLong = (long)Math.Sqrt(lst.Sum(c => (avgLong - c) * (avgLong - c)) / count);
      item.Std = TimeSpan.FromTicks(stdLong);
      var medianLong = (long)lst.OrderBy(c => c).ToList()[count / 2];
      item.Median = TimeSpan.FromTicks(medianLong);
      lst.Clear();
      _Summary.Add(item);
    }

    #endregion Public Methods
  }
}
