using UnityEngine;
namespace ServerDevcommands;
///<summary>Adds support for directly setting the value and makes it work without needing the raven appear first.</summary>
public class ResolutionCommand
{
  private string GetMode()
  {
    var mode = "window";
    if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen) mode = "exclusive";
    if (Screen.fullScreenMode == FullScreenMode.MaximizedWindow) mode = "max";
    if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow) mode = "full";
    return mode;
  }
  private void PrintResolution(Terminal terminal)
  {
    var resolution = Screen.currentResolution;
    Helper.AddMessage(terminal, $"Resolution is {Screen.width}x{Screen.height} ({resolution.width}x{resolution.height}) with {resolution.refreshRate} hz and {GetMode()} mode.");
  }
  private void SetResolution(Terminal terminal, string[] args)
  {
    var resolution = Screen.currentResolution;
    var fullscreen = Parse.String(args, 1, GetMode());
    var width = Parse.Int(args, 2, resolution.width);
    var height = Parse.Int(args, 3, resolution.height);
    var refresh = Parse.Int(args, 4, resolution.refreshRate);
    var mode = FullScreenMode.Windowed;
    if (fullscreen == "exclusive") mode = FullScreenMode.ExclusiveFullScreen;
    if (fullscreen == "max") mode = FullScreenMode.MaximizedWindow;
    if (fullscreen == "full") mode = FullScreenMode.FullScreenWindow;
    Screen.SetResolution(width, height, mode, refresh);
    var refreshStr = refresh == 0 ? "max" : refresh.ToString();
    Helper.AddMessage(terminal, $"Resolution set to {width}x{height} with {refreshStr} hz and {fullscreen} mode.");
  }
  public ResolutionCommand()
  {
    new Terminal.ConsoleCommand("resolution", "[mode] [width] [height] [refresh] - Prints or sets the resolution.", (args) =>
    {
      if (args.Length == 1) PrintResolution(args.Context);
      else SetResolution(args.Context, args.Args);
    });
    AutoComplete.Register("resolution", (int index) =>
    {
      if (index == 0) return new() { "exclusive", "full", "max", "window" };
      if (index == 1) return ParameterInfo.Create("Width", "a positive integer (max supported if not given)");
      if (index == 2) return ParameterInfo.Create("Height", "a positive integer (max supported if not given)");
      if (index == 3) return ParameterInfo.Create("Refresh rate", "a positive integer (max supported if not given)");
      return ParameterInfo.None;
    });
  }
}
