using System;
using System.Linq;
namespace ServerDevcommands;
///<summary>New command to search to list object or location ids.</summary>
public class SearchIdCommand
{
  public SearchIdCommand()
  {
    Helper.Command("search_id", "[term] [max_lines=5] - Prints object ids matching the search term. Without term, prints all of them to the log file.", (args) =>
    {
      if (args.Length < 2)
      {
        ZLog.Log("\n" + string.Join("\n", ParameterInfo.ObjectIds));
        return;
      }
      var term = args.Length < 2 ? "" : args[1].ToLower();
      var maxLines = args.TryParameterInt(2, 5);
      var objects = term == ""
        ? ParameterInfo.ObjectIds.ToArray()
        : ParameterInfo.ObjectIds.Where(id => id.ToLower().Contains(term)).ToArray();
      if (objects.Length > 100)
      {
        args.Context.AddString("Over 100 results, printing to the log file.");
        ZLog.Log("\n" + string.Join("\n", objects));
        return;
      }
      var bufferSize = (int)Math.Ceiling((float)objects.Length / maxLines);
      var buffer = new string[bufferSize];
      for (int i = 0; i < objects.Length; i += bufferSize)
      {
        if (objects.Length - i < bufferSize)
        {
          bufferSize = objects.Length - i;
          buffer = new string[bufferSize];
        }
        Array.Copy(objects, i, buffer, 0, bufferSize);
        args.Context.AddString(string.Join(", ", buffer));
      }
    }, () => ParameterInfo.Create("Search term"));
    AutoComplete.Register("search_id", (int index) =>
    {
      if (index == 0) return ParameterInfo.Create("Search term");
      if (index == 1) return ParameterInfo.Create("Max lines", "number (default 5)");
      return ParameterInfo.None;
    });
  }
}
