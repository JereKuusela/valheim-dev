using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace ServerDevcommands;

public class BindData
{
  [DefaultValue("")]
  public string keys = "";
  [DefaultValue("")]
  public string state = "";
  public string? key;
  public string? modifiers;
  [DefaultValue("")]
  public string command = "";
  [DefaultValue("")]
  public string offCommand = "";
}

public class CommandBind
{
  public List<KeyCode> Required = [];
  public List<KeyCode>? Banned;
  public List<string>? RequiredState;
  public List<string>? BannedState;
  public bool MouseWheel;
  public string Command = "";
  public string OffCommand = "";
  public bool Executed = false;
  public bool WasExecuted = false;
  public string? Keys;
}
