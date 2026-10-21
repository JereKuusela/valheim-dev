using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerDevcommands;

public class TimedCache(Func<List<string>?> build, List<string>? initial = null)
{
  private const float IdleSeconds = 30f;
  private List<string> value = initial ?? [];
  private float lastRead = float.NegativeInfinity;

  public List<string> Get()
  {
    var now = Time.realtimeSinceStartup;
    if (now - lastRead > IdleSeconds)
    {
      var built = build();
      if (built == null) return value;
      value = built;
    }
    lastRead = now;
    return value;
  }
}
