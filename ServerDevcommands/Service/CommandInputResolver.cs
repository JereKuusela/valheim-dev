using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Service;

public static class CommandInputResolver
{
  private static readonly Regex InputPattern = new("<input(?:_([^>]+))?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
  private static readonly Queue<InputRequest> PendingRequests = [];
  private static InputRequest? ActiveRequest;

  private sealed class InputRequest(string command, List<InputMatch> matches, Action<string> onResolved)
  {
    public readonly List<InputMatch> Matches = matches;
    public readonly Action<string> OnResolved = onResolved;
    public readonly CommandInput Input = new();
    public int NextIndex = 0;
    public string Command = command;
  }

  private readonly struct InputMatch(string token, string topic)
  {
    public readonly string Token = token;
    public readonly string Topic = topic;
  }

  public static bool TryResolve(string command, Action<string> onResolved)
  {
    var matches = FindMatches(command);
    if (matches.Count == 0) return false;

    PendingRequests.Enqueue(new InputRequest(command, matches, onResolved));
    StartNextRequest();
    return true;
  }

  private static void StartNextRequest()
  {
    if (ActiveRequest != null || PendingRequests.Count == 0) return;
    ActiveRequest = PendingRequests.Dequeue();
    ContinueActiveRequest();
  }

  private static void ContinueActiveRequest()
  {
    if (ActiveRequest == null) return;

    if (ActiveRequest.NextIndex >= ActiveRequest.Matches.Count)
    {
      var completed = ActiveRequest;
      ActiveRequest = null;
      completed.OnResolved(completed.Command);
      StartNextRequest();
      return;
    }

    var match = ActiveRequest.Matches[ActiveRequest.NextIndex++];
    ActiveRequest.Input.Ask(match.Topic, value =>
    {
      if (ActiveRequest == null) return;
      ActiveRequest.Command = ReplaceFirst(ActiveRequest.Command, match.Token, value);
      ContinueActiveRequest();
    });
  }

  private static List<InputMatch> FindMatches(string command)
  {
    List<InputMatch> matches = [];
    foreach (Match match in InputPattern.Matches(command))
    {
      if (!match.Success) continue;
      var topic = match.Groups[1].Success && !string.IsNullOrWhiteSpace(match.Groups[1].Value)
        ? match.Groups[1].Value
        : "text";
      matches.Add(new InputMatch(match.Value, topic.Replace("_", " ")));
    }
    return matches;
  }

  private static string ReplaceFirst(string source, string token, string replacement)
  {
    var index = source.IndexOf(token, StringComparison.Ordinal);
    if (index < 0) return source;
    return source.Substring(0, index) + replacement + source.Substring(index + token.Length);
  }
}
