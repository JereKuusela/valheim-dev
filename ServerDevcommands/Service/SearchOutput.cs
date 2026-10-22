using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerDevcommands;
public static class SearchOutput
{
  public const int RowWidth = 150;
  public const int MaxRows = 250;

  public static bool TakeLog(ref string[] values, int required, bool unlimited = false)
  {
    var log = values.Length > required && values[values.Length - 1].Equals("log", StringComparison.OrdinalIgnoreCase);
    if (log) values = [.. values.Take(values.Length - 1)];
    if (!unlimited && values.Length > required) throw new InvalidOperationException("Too many arguments.");
    return log;
  }

  public static (List<string> Rows, int Hidden) Wrap(IReadOnlyList<string> values, int width = RowWidth, int maxRows = MaxRows)
  {
    var rows = new List<string>();
    var row = new StringBuilder();
    for (var i = 0; i < values.Count; i++)
    {
      var value = values[i];
      if (row.Length > 0 && row.Length + 2 + value.Length > width)
      {
        rows.Add(row.ToString());
        row.Clear();
        if (rows.Count == maxRows) return (rows, values.Count - i);
      }
      if (row.Length > 0) row.Append(", ");
      row.Append(value);
    }
    if (row.Length > 0) rows.Add(row.ToString());
    return (rows, 0);
  }

  public static void Print(Terminal context, string command, string search, IReadOnlyList<string> values, bool log, string? empty = null)
  {
    if (values.Count == 0)
    {
      context.AddString(empty ?? $"No results for {search}.");
      return;
    }
    context.AddString($"{values.Count} {(values.Count == 1 ? "result" : "results")} for {search}.");
    if (log)
    {
      ZLog.Log($"{command} results for {search}:\n{string.Join("\n", values)}");
      context.AddString("Output written to the client log file instead of the console.");
      return;
    }
    var (rows, hidden) = Wrap(values);
    foreach (var row in rows) context.AddString(row);
    if (hidden > 0) context.AddString($"\n{hidden} more results hidden. Add \"log\" argument to write all results to the client log file.");
  }
}
