using System;
using HarmonyLib;

namespace Service;

public class CommandInput : TextReceiver
{
  public string Topic = "text";
  public Action<string>? OnTextReceived;
  public Action? OnCancelled;
  public string Text = "";

  public void Ask(string topic, Action<string> onTextReceived, Action onCancelled)
  {
    Topic = topic;
    OnTextReceived = onTextReceived;
    OnCancelled = onCancelled;
    Show();
  }

  public void Show()
  {
    if (TextInput.instance)
      TextInput.instance.RequestText(this, Topic, 1000);
  }
  public string GetText() => Text;

  public void SetText(string text)
  {
    Text = text;
    OnTextReceived?.Invoke(text);
    OnTextReceived = null;
    OnCancelled = null;
  }

  public void Hide()
  {
    OnCancelled?.Invoke();
    OnTextReceived = null;
    OnCancelled = null;
  }
}

[HarmonyPatch(typeof(TextInput), nameof(TextInput.Hide))]
public class TextInput_Hide_Patch
{
  public static void Postfix()
  {
    if (TextInput.instance && TextInput.instance.m_queuedSign is CommandInput commandInput)
      commandInput.Hide();
  }
}