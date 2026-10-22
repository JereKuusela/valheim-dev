using System;
using System.Linq;
namespace ServerDevcommands;
///<summary>New command to search to list object or location ids.</summary>
public class SearchIdCommand
{
  public SearchIdCommand()
  {
    Helper.Command("search_id", "[term] [log] - Prints object ids containing the term. The term * prints all of them. The flag log writes the result to the client log file instead of the console.", (args) =>
    {
      Helper.ArgsCheck(args, 2, "Missing the search term");
      var values = args.Args.Skip(1).ToArray();
      var log = SearchOutput.TakeLog(ref values, 1);
      if (!ZNetScene.instance) throw new InvalidOperationException("Join a world first.");
      var term = values[0].Trim('*');
      var ids = term == ""
        ? ParameterInfo.ObjectIds
        : ParameterInfo.ObjectIds.Where(id => id.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
      SearchOutput.Print(args.Context, "search_id", $"\"{values[0]}\"", ids, log);
    });
    AutoComplete.Register("search_id", index =>
    {
      if (index == 0) return ParameterInfo.ObjectIds;
      if (index == 1) return ParameterInfo.Create("log", "Write the result to the client log file instead of the console.");
      return ParameterInfo.None;
    });
  }
}
