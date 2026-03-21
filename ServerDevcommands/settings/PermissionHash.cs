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

  // God Mode Features
  public static readonly int GodModeNoWeightLimit = "godmodenoweightlimit".GetStableHashCode();
  public static readonly int GodModeNoStamina = "godmodenostamina".GetStableHashCode();
  public static readonly int GodModeNoEitr = "godmodenoeitr".GetStableHashCode();
  public static readonly int GodModeNoUsage = "godmodenousage".GetStableHashCode();
  public static readonly int GodModeAlwaysDodge = "godmodealwaysdodge".GetStableHashCode();
  public static readonly int GodModeAlwaysParry = "godmodealwaysparry".GetStableHashCode();
  public static readonly int GodModeNoStagger = "godmodenostagger".GetStableHashCode();
  public static readonly int GodModeNoKnockback = "godmodenoknockback".GetStableHashCode();
  public static readonly int GodModeNoEdgeOfWorld = "godmodenoedgeofworld".GetStableHashCode();
  public static readonly int GodModeNoMist = "godmodenomist".GetStableHashCode();

  // Ghost Mode Features
  public static readonly int GhostInvisibility = "ghostinvisibility".GetStableHashCode();
  public static readonly int GhostNoSpawns = "ghostnospawns".GetStableHashCode();
  public static readonly int GhostIgnoreSleep = "ghostignoresleep".GetStableHashCode();

  // Fly Mode Features
  public static readonly int Fly = "fly".GetStableHashCode();

  // Access Features
  public static readonly int AccessPrivateChests = "accessprivatechests".GetStableHashCode();
  public static readonly int AccessWardedAreas = "accesswardedareas".GetStableHashCode();

  // Disable Features
  public static readonly int DisableNoMap = "disablenomap".GetStableHashCode();
  public static readonly int DisableEvents = "disableevents".GetStableHashCode();
  public static readonly int DisableStartShout = "disablestartshout".GetStableHashCode();

  public static readonly int NoDrops = "nodrops".GetStableHashCode();
  public static readonly int HideShoutPings = "hideshoutpings".GetStableHashCode();

}
