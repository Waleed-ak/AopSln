using System.Threading.Tasks;

namespace Tools
{
  public interface ITemp
  {
    #region Public Properties
    int Age { get; }

    string Name { get; set; }
    #endregion Public Properties

    #region Public Indexers
    int this[int i] { get; set; }
    #endregion Public Indexers

    #region Public Methods

    int Add(int a,int b);

    void Add(int a,int b,out double d);

    Task<int> AddAsync(int a,int b);

    void Print(string a);

    Task PrintAsync(string a);

    void Write<Tin>(Tin a);

    #endregion Public Methods
  }
}
