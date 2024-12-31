using System.Linq;
using UnityEngine;

namespace ServerDevcommands;
///<summary>Adds output when used without the parameter.</summary>
public class KillCommand
{
  public KillCommand()
  {
    Helper.Command("killall", "kill nearby creatures", static (args) =>
    {
      var allCharacters = Character.GetAllCharacters();
      var killedCreatures = 0;
      var killedSpawners = 0;
      foreach (var character in allCharacters)
      {
        if (character.IsPlayer()) continue;
        character.Damage(new HitData(1E+10f));
        killedCreatures++;

      }
      if (Settings.KillDestroySpawners)
      {
        var spawners = Object.FindObjectsByType<SpawnArea>(FindObjectsSortMode.None);
        foreach (var spawner in spawners)
        {
          var destructible = spawner.gameObject.GetComponent<Destructible>();
          if (destructible)
          {
            destructible.Damage(new HitData(1E+10f));
            killedSpawners++;
          }
        }
      }
      Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, string.Format("Killed {0} monsters{1}", killedCreatures, (killedSpawners > 0) ? string.Format(" & {0} spawners.", killedSpawners) : "."), 0, null);
    });
    AutoComplete.RegisterEmpty("killall");

    Helper.Command("killenemies", "kill nearby enemies", static (args) =>
    {
      var allCharacters = Character.GetAllCharacters();
      var killedCreatures = 0;
      var killedSpawners = 0;
      foreach (var character in allCharacters)
      {
        if (character.IsPlayer() || character.IsTamed()) continue;
        character.Damage(new HitData(1E+10f));
        killedCreatures++;

      }
      if (Settings.KillDestroySpawners)
      {
        var spawners = Object.FindObjectsByType<SpawnArea>(FindObjectsSortMode.None);
        foreach (var spawner in spawners)
        {
          var destructible = spawner.gameObject.GetComponent<Destructible>();
          if (destructible)
          {
            destructible.Damage(new HitData(1E+10f));
            killedSpawners++;
          }
        }
      }
      Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, string.Format("Killed {0} monsters{1}", killedCreatures, (killedSpawners > 0) ? string.Format(" & {0} spawners.", killedSpawners) : "."), 0, null);
    });
    AutoComplete.RegisterEmpty("killenemies");
  }
}
