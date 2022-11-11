using System.Collections.Generic;
namespace ServerDevcommands;
///<summary>Helper class for parameter options/info. The main purpose is to provide some caching to avoid performance issues.</summary>
public partial class ParameterInfo
{

  public static List<string> RollPitchYaw(string name, string description, int index)
  {
    if (index == 0) return Create($"{name}=<color=yellow>roll</color>,pitch,yaw | {description}.");
    if (index == 1) return Create($"{name}=roll,<color=yellow>pitch</color>,yaw | {description}.");
    if (index == 2) return Create($"{name}=roll,pitch,<color=yellow>yaw</color> | {description}.");
    return None;
  }
  public static List<string> RollPitchYaw(string description, int index)
  {
    if (index == 0) return Create($"<color=yellow>roll</color>,pitch,yaw | {description}.");
    if (index == 1) return Create($"roll,<color=yellow>pitch</color>,yaw | {description}.");
    if (index == 2) return Create($"roll,pitch,<color=yellow>yaw</color> | {description}.");
    return None;
  }
  public static List<string> YawRollPitch(string name, string description, int index)
  {
    if (index == 0) return Create($"{name}=<color=yellow>yaw</color>,roll,pitch | {description}.");
    if (index == 1) return Create($"{name}=yaw,<color=yellow>roll</color>,pitch | {description}.");
    if (index == 2) return Create($"{name}=yaw,roll,<color=yellow>pitch</color> | {description}.");
    return None;
  }
  public static List<string> YawRollPitch(string description, int index)
  {
    if (index == 0) return Create($"<color=yellow>yaw</color>,roll,pitch | {description}.");
    if (index == 1) return Create($"yaw,<color=yellow>roll</color>,pitch | {description}.");
    if (index == 2) return Create($"yaw,roll,<color=yellow>pitch</color> | {description}.");
    return None;
  }
}
