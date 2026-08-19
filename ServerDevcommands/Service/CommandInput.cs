using System;
using HarmonyLib;

namespace Service;

public class CommandInput(Action<string> onTextReceived, Action onCancelled) : TextReceiver
{
  public string Topic = "text";

  public void Ask(string topic)
  {
    Topic = topic;
    Show();
  }

  public void Show()
  {
    if (TextInput.instance)
      TextInput.instance.RequestText(this, Topic, 1000);
  }

  public void ShowIfHidden()
  {
    if (TextInput.instance && !TextInput.IsVisible())
      Show();
  }

  public string GetText() => "";

  public void SetText(string text)
  {
    onTextReceived.Invoke(text);
  }

  public void Hide()
  {
    onCancelled.Invoke();
  }
}

[HarmonyPatch(typeof(TextInput), nameof(TextInput.Hide))]
public class TextInput_OnCancel_Patch
{
  public static void Postfix()
  {
    if (TextInput.instance && TextInput.instance.m_queuedSign is CommandInput commandInput)
      commandInput.Hide();
  }
}