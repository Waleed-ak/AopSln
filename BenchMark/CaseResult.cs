using System;
using System.Text;

namespace Tools
{
  public class CaseResult
  {
    #region Public Properties
    public TimeSpan Average { get; set; }
    public int Count { get; set; }
    public TimeSpan Max { get; set; }
    public TimeSpan Median { get; set; }
    public TimeSpan Min { get; set; }
    public TimeSpan Std { get; set; }
    public string Title { get; set; }
    public TimeSpan Toltal { get; set; }
    #endregion Public Properties

    #region Public Methods

    public override string ToString()
    {
      var sb = new StringBuilder();
      const string labelFormat = "{0,7}";
      const string numberFormat = "{0,10:f3}";
      const string unit = " (ms)";
      return sb.AppendLine()
        .Append('*').Append(Title).Append("* Count:").Append(Count).AppendLine()
        .AppendLine("[")
        .AppendFormat(labelFormat,"Total:").AppendFormat(numberFormat,Toltal.TotalMilliseconds).AppendLine(unit)
        .AppendFormat(labelFormat,"Avg:").AppendFormat(numberFormat,Average.TotalMilliseconds).AppendLine(unit)
        .AppendFormat(labelFormat,"Median:").AppendFormat(numberFormat,Median.TotalMilliseconds).AppendLine(unit)
        .AppendFormat(labelFormat,"Std:").AppendFormat(numberFormat,Std.TotalMilliseconds).AppendLine(unit)
        .AppendFormat(labelFormat,"Min:").AppendFormat(numberFormat,Min.TotalMilliseconds).AppendLine(unit)
        .AppendFormat(labelFormat,"Max:").AppendFormat(numberFormat,Max.TotalMilliseconds).AppendLine(unit)
        .AppendLine("]")
        .ToString();
    }

    #endregion Public Methods
  }
}
