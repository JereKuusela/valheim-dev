namespace ServerDevcommands;

/// <summary>
/// Pre-calculated stable hash codes for ServerDevcommands permission features.
/// Uses lowercase names for case-insensitive matching.
/// </summary>
public static class PermissionHash
{
  public const string Section = "serverdevcommands";

  // Map & Minimap Features
  public static readonly int MapCoordinates = "mapcoordinates".GetStableHashCode();
  public static readonly int MiniMapCoordinates = "minimapcoordinates".GetStableHashCode();
  public static readonly int ShowPrivatePlayers = "showprivateplayers".GetStableHashCode();


  // Ghost Mode Features
  public static readonly int Ghost = "ghost".GetStableHashCode();

  public static readonly int God = "god".GetStableHashCode();
  public static readonly int Fly = "fly".GetStableHashCode();
  public static readonly int IgnoreWards = "ignorewards".GetStableHashCode();
  public static readonly int NoClipCamera = "noclipcamera".GetStableHashCode();

  // Disable Features
  public static readonly int IgnoreNoMap = "ignorenomap".GetStableHashCode();
  public static readonly int DisableEvents = "disableevents".GetStableHashCode();
  public static readonly int DisableStartShout = "disablestartshout".GetStableHashCode();

  public static readonly int NoDrops = "nodrops".GetStableHashCode();
  public static readonly int HideShoutPings = "hideshoutpings".GetStableHashCode();

}
