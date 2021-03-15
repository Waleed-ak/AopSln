namespace Tools
{
  using System;
  using System.Diagnostics.CodeAnalysis;

  [SuppressMessage("Design","RCS1194:Implement exception constructors.")]
  public class RelaxException:Exception
  {
    #region Public Constructors

    public RelaxException(Exception inner,Type type) : base($"Class Relax Generic Parameter Type {type.Name}",inner)
    {
    }

    #endregion Public Constructors
  }
}
