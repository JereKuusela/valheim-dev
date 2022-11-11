using System.Collections.Generic;
using System.Linq;
namespace ServerDevcommands;
///<summary>Helper class for command info.</summary>
public static class CommandInfo
{
  public static string Parameter(string name) => $"[<color=yellow>{name}</color>]";
  public static string NamedParameter(string name) => $"<color=yellow>{name}</color>";
  public static string Create(string description, IEnumerable<string>? parameters = null, IEnumerable<string>? namedParameters = null)
  {
    var parameterString = parameters == null ? "" : string.Join(" ", parameters.Select(Parameter));
    if (parameterString != "") parameterString += " ";
    var namedParameterString = namedParameters == null ? "" : string.Join(", ", namedParameters.Select(NamedParameter));
    if (namedParameterString != "") namedParameterString = $"({namedParameterString}) ";
    return $"{parameterString}{namedParameterString}- {description}";
  }
}
