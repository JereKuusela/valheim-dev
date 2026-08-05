using System;

namespace Service;

public class CommandInput : TextReceiver
{
  public string Topic = "text";
  public Action<string>? OnTextReceived;
  public string Text = "";

  public void Ask(string topic, Action<string> onTextReceived)
  {
    Topic = topic;
    OnTextReceived = text =>
    {
      OnTextReceived = null;
      onTextReceived(text);
    };
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
  }
}
