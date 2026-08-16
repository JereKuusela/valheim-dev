using BepInEx.Logging;

namespace Service;

public class Log
{
#nullable disable
  private static ManualLogSource Logger;
#nullable enable
  public static void Init(ManualLogSource logger)
  {
    Logger = logger;
  }
  public static void Error(string message)
  {
    Logger.LogError(message);
  }
  public static void Warning(string message)
  {
    Logger.LogWarning(message);
  }
  public static void Info(string message)
  {
    Logger.LogInfo(message);
  }
  public static void Debug(string message)
  {
    Logger.LogDebug(message);
  }
}