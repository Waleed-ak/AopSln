using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tools
{
  public delegate object DelInvoke(object target,object[] args,Type[] types);

  public delegate object DelPropertyGet(object target,object[] args);

  public delegate void DelPropertySet(object target,object value,object[] args);

  public class ItemMethod
  {
    #region Internal Constructors

    internal ItemMethod(MethodInfo info)
    {
      Attributes = CustomAttributeExtensions.GetCustomAttributes(info,true).ToArray();
      DeclaringType = info.DeclaringType;
      Method = info;
      Name = info.Name;
      Parameters = info.GetParameters().Select((c,i) => new ItemParam(c,i)).ToArray();
      Return = new ItemReturn(info.ReturnParameter);
    }

    #endregion Internal Constructors

    #region Public Properties
    public Attribute[] Attributes { get; }
    public Type DeclaringType { get; }
    public MethodInfo Method { get; }
    public string Name { get; }
    public ItemParam[] Parameters { get; }
    public ItemReturn Return { get; internal set; }
    #endregion Public Properties
  }

  public class ItemParam
  {
    #region Internal Constructors

    internal ItemParam(ParameterInfo info,int index)
    {
      Attributes = CustomAttributeExtensions.GetCustomAttributes(info,true).ToArray();
      Index = index;
      Name = info.Name;
    }

    #endregion Internal Constructors

    #region Public Properties
    public Attribute[] Attributes { get; }
    public int Index { get; }
    public string Name { get; }
    #endregion Public Properties
  }

  public class ItemProperty
  {
    #region Internal Constructors

    internal ItemProperty(PropertyInfo info)
    {
      Attributes = CustomAttributeExtensions.GetCustomAttributes(info,true).ToArray();
      DeclaringType = info.DeclaringType;
      Name = info.Name;
      Parameters = info.GetIndexParameters().Select((c,i) => new ItemParam(c,i)).ToArray();
      Property = info;
    }

    #endregion Internal Constructors

    #region Public Properties
    public Attribute[] Attributes { get; }
    public Type DeclaringType { get; }
    public string Name { get; }
    public ItemParam[] Parameters { get; }
    public PropertyInfo Property { get; }
    #endregion Public Properties
  }

  public class ItemReturn
  {
    #region Internal Constructors

    internal ItemReturn(ParameterInfo info)
    {
      Attributes = CustomAttributeExtensions.GetCustomAttributes(info,true).ToArray();
    }

    #endregion Internal Constructors

    #region Public Properties
    public Attribute[] Attributes { get; }
    #endregion Public Properties
  }

  internal class Lookups
  {
    #region Internal Fields
    internal Dictionary<string,Relax<DelInvoke>> _DelMethod = new();
    internal Dictionary<string,Relax<DelPropertyGet>> _DelPropertyGet = new();
    internal Dictionary<string,Relax<DelPropertySet>> _DelPropertySet = new();
    internal Dictionary<string,Relax<DelInvoke>> _DelTask = new();
    internal Dictionary<string,ItemMethod> _Methods = new();
    internal Dictionary<string,ItemProperty> _Properties = new();
    #endregion Internal Fields
  }
}
