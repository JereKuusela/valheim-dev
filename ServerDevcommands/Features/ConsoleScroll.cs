using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace ServerDevcommands;

[HarmonyPatch(typeof(Terminal), nameof(Terminal.UpdateInput))]
public class ConsoleWheelScroll
{
  const int LinesPerTick = 3;
  static void Prefix(Terminal __instance, out int __state) => __state = __instance.m_scrollHeight;
  static void Postfix(Terminal __instance, int __state)
  {
    if (__instance != Console.instance) return;
    var page = Mathf.Max(1, __instance.m_maxVisibleBufferLength - 1);
    var shift = ZInput.GetKey(KeyCode.LeftShift) || ZInput.GetKey(KeyCode.RightShift);
    var ctrl = ZInput.GetKey(KeyCode.LeftControl) || ZInput.GetKey(KeyCode.RightControl);
    var wheel = ZInput.GetMouseScrollWheel();
    if (wheel != 0f)
    {
      var step = shift ? page : LinesPerTick;
      ConsoleScrollBar.Scroll(__instance, __instance.m_scrollHeight + (wheel > 0f ? step : -step));
      return;
    }
    var up = ZInput.GetButtonDown("ScrollChatUp");
    if (!up && !ZInput.GetButtonDown("ScrollChatDown")) return;
    if (ctrl) ConsoleScrollBar.Scroll(__instance, up ? int.MaxValue : 0);
    else if (shift) ConsoleScrollBar.Scroll(__instance, __state + (up ? page : -page));
  }
}

[HarmonyPatch(typeof(GameCamera), nameof(GameCamera.UpdateMouseCapture))]
public class ConsoleCursor
{
  static void Postfix()
  {
    if (!Console.IsVisible()) return;
    ZCursor.LockState = ZInput.IsMouseActive() ? CursorLockMode.None : CursorLockMode.Locked;
    ZCursor.Show();
  }
}

[HarmonyPatch(typeof(Terminal), nameof(Terminal.UpdateChat))]
public class ConsoleScrollBar
{
  static RectTransform? track;
  static RectTransform? thumb;

  public static void Scroll(Terminal terminal, int height)
  {
    height = Mathf.Clamp(height, 0, Mathf.Max(0, terminal.m_chatBuffer.Count - 5));
    if (height == terminal.m_scrollHeight) return;
    terminal.m_scrollHeight = height;
    terminal.UpdateChat();
  }

  public static void GetRange(Terminal terminal, out int start, out int end)
  {
    var total = terminal.m_chatBuffer.Count;
    end = Mathf.Min(total, Mathf.Max(5, total - terminal.m_scrollHeight));
    start = Mathf.Max(0, end - terminal.m_maxVisibleBufferLength);
  }

  static void Postfix(Terminal __instance)
  {
    if (__instance != Console.instance) return;
    if (!track) Create(__instance);
    var total = __instance.m_chatBuffer.Count;
    track!.gameObject.SetActive(total > __instance.m_maxVisibleBufferLength);
    if (total <= __instance.m_maxVisibleBufferLength) return;
    GetRange(__instance, out var start, out var end);
    thumb!.anchorMin = new Vector2(0f, 1f - end / (float)total);
    thumb.anchorMax = new Vector2(1f, 1f - start / (float)total);
  }
  static void Create(Terminal terminal)
  {
    var obj = new GameObject("ScrollBar", typeof(RectTransform), typeof(Image));
    track = obj.GetComponent<RectTransform>();
    track.SetParent(terminal.m_output.rectTransform, false);
    track.anchorMin = new Vector2(1f, 0f);
    track.anchorMax = Vector2.one;
    track.pivot = new Vector2(1f, 0.5f);
    track.sizeDelta = new Vector2(10f, 0f);
    obj.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.1f);
    obj.AddComponent<ScrollBarDrag>();

    var thumbObj = new GameObject("Thumb", typeof(RectTransform), typeof(Image));
    thumb = thumbObj.GetComponent<RectTransform>();
    thumb.SetParent(track, false);
    thumb.offsetMin = Vector2.zero;
    thumb.offsetMax = Vector2.zero;
    thumbObj.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.5f);
  }
}

public class ScrollBarDrag : MonoBehaviour
{
  static readonly List<RaycastResult> hits = [];
  bool dragging;
  float grab;

  void OnGUI()
  {
    GUI.depth = int.MaxValue;
    var e = Event.current;
    if (e.type == EventType.MouseDown && e.button == 0) Press(e);
    else if (dragging && e.type == EventType.MouseDrag) Drag(e);
    else if (dragging && e.type == EventType.MouseUp)
    {
      dragging = false;
      e.Use();
      Console.instance.m_input.ActivateInputField();
    }
  }
  void OnDisable() => dragging = false;

  Camera? GetCamera()
  {
    var canvas = GetComponentInParent<Canvas>().rootCanvas;
    return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
  }
  static Vector2 ToScreen(Event e) => new(e.mousePosition.x, Screen.height - e.mousePosition.y);

  float GetFraction(Event e, out bool inside)
  {
    var rect = (RectTransform)transform;
    var cam = GetCamera();
    var screen = ToScreen(e);
    inside = RectTransformUtility.RectangleContainsScreenPoint(rect, screen, cam);
    var corners = new Vector3[4];
    rect.GetWorldCorners(corners);
    var bottom = RectTransformUtility.WorldToScreenPoint(cam, corners[0]).y;
    var top = RectTransformUtility.WorldToScreenPoint(cam, corners[1]).y;
    return Mathf.InverseLerp(top, bottom, screen.y);
  }

  bool IsTopmost(Event e)
  {
    if (!EventSystem.current) return true;
    hits.Clear();
    EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = ToScreen(e) }, hits);
    return hits.Count > 0 && hits[0].gameObject.transform.IsChildOf(transform);
  }

  void Press(Event e)
  {
    var fraction = GetFraction(e, out var inside);
    if (!inside) return;
    var topmost = IsTopmost(e);
    if (!topmost) return;
    var terminal = Console.instance;
    var total = terminal.m_chatBuffer.Count;
    ConsoleScrollBar.GetRange(terminal, out var start, out var end);
    var center = (start + end) / 2f / total;
    grab = Mathf.Abs(fraction - center) <= (end - start) / 2f / total ? fraction - center : 0f;
    dragging = true;
    e.Use();
    terminal.m_input.ActivateInputField();
    Drag(e);
  }

  void Drag(Event e)
  {
    var terminal = Console.instance;
    var total = terminal.m_chatBuffer.Count;
    ConsoleScrollBar.GetRange(terminal, out var start, out var end);
    var fraction = GetFraction(e, out _) - grab;
    var newEnd = Mathf.RoundToInt(fraction * total + (end - start) / 2f);
    ConsoleScrollBar.Scroll(terminal, total - newEnd);
    e.Use();
  }
}
