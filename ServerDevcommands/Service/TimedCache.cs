using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerDevcommands;

public class TimedCache(Func<List<string>?> build, List<string>? initial = null)
{
  private const float IdleSeconds = 10f;
  private const float MaxAgeSeconds = 30f;
  private List<string> value = initial ?? [];
  private float lastRead = float.NegativeInfinity;
  private float lastBuild = float.NegativeInfinity;

  public List<string> Get()
  {
    var now = Time.realtimeSinceStartup;
    // MaxAge forces a refresh even if the cache is read constantly.
    if (now - lastRead > IdleSeconds || now - lastBuild > MaxAgeSeconds)
    {
      var built = build();
      if (built == null) return value;
      value = built;
      lastBuild = now;
    }
    lastRead = now;
    return value;
  }
}
