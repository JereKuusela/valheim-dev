namespace ServerDevcommands;
///<summary>Commnand to print the seed.</summary>
public class SeedCommand
{
  public SeedCommand()
  {
    Helper.Command("seed", "Prints the world seed.", (args) =>
    {
      Helper.AddMessage(args.Context, WorldGenerator.instance.m_world.m_seedName);
    });
    AutoComplete.RegisterEmpty("seed");
  }
}
