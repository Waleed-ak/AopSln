namespace Tools
{
  using System;

  public class Relax<T>
  {
    #region Private Fields
    private readonly Func<T> _Func;
    private readonly object _Lock = new();
    private bool _HasValue;
    private T _Value;
    #endregion Private Fields

    #region Public Constructors

    public Relax(Func<T> func) => _Func = func;

    #endregion Public Constructors

    #region Public Properties
    public T Value => Create();
    #endregion Public Properties

    #region Private Methods

    private T Create()
    {
      if(_HasValue)
        return _Value;
      lock(_Lock)
      {
        if(!_HasValue)
        {
          try
          {
            _Value = _Func();
            _HasValue = true;
          }
          catch(Exception ex)
          {
            _HasValue = false;
            throw new RelaxException(ex,typeof(T));
          }
        }
      }
      return _Value;
    }

    #endregion Private Methods
  }
}
