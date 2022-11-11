using System.ComponentModel;

namespace ServerDevcommands;

public class BindData
{
  public string key = "";
  [DefaultValue("")]
  public string modifiers = "";
  public string command = "";
  [DefaultValue("")]
  public string tag = "";
}
