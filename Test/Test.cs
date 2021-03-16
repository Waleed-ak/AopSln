using System;

namespace Tools
{
  internal static class Test
  {
    #region Public Methods

    public static T GetInstance<T>() => AopInterceptor.Factory<T>(new TargetInterceptor());

    #endregion Public Methods

    #region Internal Methods

    internal static void Run()
    {
      var runner = new BenchMarkRunner();
      runner.RunCase(Case00,"Aop with output");
      runner.RunCase(Case02,"Tmp with output");
      runner.RunCase(Case04,"Target with output ");

      //var x1 = AopHelper.GetInstance<ITemp>();
      //RunMethod(x1);
      //var x2 = new Temp(new Interceptor());
      //RunMethod(x2);
      //var x3 = new Target();
      //  RunMethod(x3)
      const int count = 1000;
      //Wrap(Case00,"Aop with output",count);
      //Wrap(Case02,"Tmp with output",count);
      //Wrap(Case04,"Target with output ",count);

      runner.RunCase(Case05,"Target",count);
      runner.RunCase(Case03,"Tmp",count);
      runner.RunCase(Case01,"Aop",count);

      runner.Display();
      AopWrapper.Save();
    }

    #endregion Internal Methods

    #region Private Methods

    private static void Case00()
    {
      var t = GetInstance<ITemp>();

      RunMethod(t);
    }

    private static void Case01()
    {
      var t = GetInstance<ITemp>();
    }

    private static void Case02()
    {
      var t = new TargetTemp(new TargetInterceptor());
      //RunMethod(t);
    }

    private static void Case03()
    {
      var t = new TargetTemp(new TargetInterceptor());
    }

    private static void Case04()
    {
      //var t = new Target();
      //RunMethod(t);
    }

    private static void Case05()
    {
      var t = new Target();
    }

    private static void RunMethod(ITemp t)
    {
      var m = t.Age;
      t.Name = "Waleed";
      t.Print(t.Name);
      Console.WriteLine("Add    " + t.Add(20,10));
      Console.WriteLine("AddAsync" + t.AddAsync(100,200).Result);
      t.PrintAsync("Waleed").Wait();
      t.Add(1,2,out var x);
      Console.WriteLine("Add with out " + x);
      t.Write(20.887);
      Console.WriteLine(t.ToString());
      Console.WriteLine(t.GetHashCode());
      t[4] = 7;
      Console.WriteLine(t[4]);
    }

    #endregion Private Methods
  }
}
