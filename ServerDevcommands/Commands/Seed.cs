using System.Linq;

namespace ServerDevcommands {

  ///<summary>Commnand to print the seed.</summary>
  public class SeedCommand {
    public SeedCommand() {
      new Terminal.ConsoleCommand("seed", "Prints the world seed.", delegate (Terminal.ConsoleEventArgs args) {
        Helper.AddMessage(args.Context, WorldGenerator.instance.m_world.m_seedName);
      }, true, true, optionsFetcher: () => RandEventSystem.instance.m_events.Select(ev => ev.m_name).ToList());
      AutoComplete.RegisterEmpty("seed");
    }
  }
}