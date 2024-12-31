using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
namespace ServerDevcommands;
public class Range<T>
{
  public T Min;
  public T Max;
  public bool Uniform;
  public Range(T value)
  {
    Min = value;
    Max = value;
  }
  public Range(T min, T max)
  {
    Min = min;
    Max = max;
  }
}

///<summary>Contains functions for parsing arguments, etc.</summary>
public static class Parse
{
  private static Range<string> Range(string arg)
  {
    var range = arg.Split(';').ToList();
    if (range.Count == 2) return new(range[0], range[1]);
    range = arg.Split('-').ToList();
    if (range.Count > 1 && range[0] == "")
    {
      range[0] = "-" + range[1];
      range.RemoveAt(1);
    }
    if (range.Count > 2 && range[1] == "")
    {
      range[1] = "-" + range[2];
      range.RemoveAt(2);
    }
    if (range.Count == 1) return new(range[0]);
    else return new(range[0], range[1]);

  }
  public static int Int(string arg, int defaultValue = 0)
  {
    if (int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
      return result;
    return defaultValue;
  }
  public static int Int(string[] args, int index, int defaultValue = 0)
  {
    if (args.Length <= index) return defaultValue;
    return Int(args[index], defaultValue);
  }
  public static int? IntNull(string arg)
  {
    if (int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
      return result;
    return null;
  }
  public static int? IntNull(string[] args, int index)
  {
    if (args.Length <= index) return null;
    return IntNull(args[index]);
  }
  public static Range<int> IntRange(string arg, int defaultValue = 0)
  {
    var range = Range(arg);
    return new(Int(range.Min, defaultValue), Int(range.Max, defaultValue));
  }
  public static Range<int> IntRange(string[] args, int index, int defaultValue = 0)
  {
    if (args.Length <= index) return new(defaultValue);
    return IntRange(args[index], defaultValue);
  }
  public static uint UInt(string arg, uint defaultValue = 0)
  {
    if (uint.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
      return result;
    return defaultValue;
  }
  public static uint UInt(string[] args, int index, uint defaultValue = 0)
  {
    if (args.Length <= index) return defaultValue;
    return UInt(args[index], defaultValue);
  }
  public static Range<uint> UIntRange(string arg, uint defaultValue = 0)
  {
    var range = Range(arg);
    return new(UInt(range.Min, defaultValue), UInt(range.Max, defaultValue));
  }
  public static Range<uint> UIntRange(string[] args, int index, uint defaultValue = 0)
  {
    if (args.Length <= index) return new(defaultValue);
    return UIntRange(args[index], defaultValue);
  }
  public static long Long(string arg, long defaultValue = 0)
  {
    if (long.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
      return result;
    return defaultValue;
  }
  public static long Long(string[] args, int index, long defaultValue = 0)
  {
    if (args.Length <= index) return defaultValue;
    return Long(args[index], defaultValue);
  }
  public static long? LongNull(string[] args, int index)
  {
    if (args.Length <= index) return null;
    return LongNull(args[index]);
  }
  public static long? LongNull(string arg)
  {
    if (long.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
      return result;
    return null;
  }
  public static Range<long> LongRange(string arg, long defaultValue = 0)
  {
    var range = Range(arg);
    return new(Long(range.Min, defaultValue), Long(range.Max, defaultValue));
  }
  public static Range<long> LongRange(string[] args, int index, long defaultValue = 0)
  {
    if (args.Length <= index) return new(defaultValue);
    return LongRange(args[index], defaultValue);
  }
  public static float Float(string arg, float defaultValue = 0f)
  {
    if (!float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
      return defaultValue;
    return result;
  }
  public static bool TryFloat(string arg, out float value)
  {
    return float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
  }
  public static float Float(string[] args, int index, float defaultValue = 0f)
  {
    if (args.Length <= index) return defaultValue;
    return Float(args[index], defaultValue);
  }
  public static float? FloatNull(string arg)
  {
    if (float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
      return result;
    return null;
  }
  public static float? FloatNull(string[] args, int index)
  {
    if (args.Length <= index) return null;
    return FloatNull(args[index]);
  }
  public static Range<float> FloatRange(string arg, float defaultValue = 0f)
  {
    var range = Range(arg);
    return new(Float(range.Min, defaultValue), Float(range.Max, defaultValue));
  }
  public static Range<float> FloatRange(string[] args, int index, float defaultValue = 0f)
  {
    if (args.Length <= index) return new(defaultValue);
    return FloatRange(args[index], defaultValue);
  }
  public static string String(string[] args, int index, string defaultValue = "")
  {
    if (args.Length <= index) return defaultValue;
    return args[index];
  }
  public static Quaternion AngleYXZ(string arg) => AngleYXZ(arg, Quaternion.identity);
  public static Quaternion AngleYXZ(string[] values, int index) => AngleYXZ(values, Quaternion.identity, index);
  public static Quaternion AngleYXZ(string arg, Quaternion defaultValue) => AngleYXZ(Split(arg), defaultValue);
  public static Quaternion AngleYXZ(string[] values, Quaternion defaultValue, int index = 0)
  {
    var angle = Vector3.zero;
    angle.y = Float(values, 0 + index, defaultValue.eulerAngles.y);
    angle.x = Float(values, 1 + index, defaultValue.eulerAngles.x);
    angle.z = Float(values, 2 + index, defaultValue.eulerAngles.z);
    return Quaternion.Euler(angle);
  }
  public static Quaternion? AngleYXZNull(string arg) => AngleYXZNull(Split(arg));
  public static Quaternion? AngleYXZNull(string[] values)
  {
    var y = FloatNull(values, 0);
    var x = FloatNull(values, 1);
    var z = FloatNull(values, 2);
    if (y == null || x == null || z == null) return null;
    return Quaternion.Euler(new(x.Value, y.Value, z.Value));
  }
  public static Range<Quaternion> AngleYXZRange(string arg) => AngleYXZRange(arg, Quaternion.identity);
  public static Range<Quaternion> AngleYXZRange(string arg, Quaternion defaultValue)
  {
    var parts = Split(arg);
    var y = Parse.FloatRange(parts, 0, defaultValue.y);
    var x = Parse.FloatRange(parts, 1, defaultValue.x);
    var z = Parse.FloatRange(parts, 2, defaultValue.z);
    return ToAngleRange(x, y, z);
  }
  private static Range<Quaternion> ToAngleRange(Range<float> x, Range<float> y, Range<float> z)
  {
    var min = Quaternion.Euler(new(x.Min, y.Min, z.Min));
    var max = Quaternion.Euler(new(x.Max, y.Max, z.Max));
    return new(min, max);
  }
  ///<summary>Parses XZY vector starting at zero index. Zero is used for missing values.</summary>
  public static Vector3 VectorXZY(string arg) => VectorXZY(Split(arg), 0, Vector3.zero);
  public static Vector3 VectorXZY(string[] args) => VectorXZY(args, 0, Vector3.zero);
  ///<summary>Parses XZY vector starting at zero index. Default values is used for missing values.</summary>
  public static Vector3 VectorXZY(string[] args, Vector3 defaultValue) => VectorXZY(args, 0, defaultValue);
  ///<summary>Parses XZY vector starting at given index. Zero is used for missing values.</summary>
  public static Vector3 VectorXZY(string[] args, int index) => VectorXZY(args, index, Vector3.zero);
  ///<summary>Parses XZY vector starting at given index. Default values is used for missing values.</summary>
  public static Vector3 VectorXZY(string[] args, int index, Vector3 defaultValue)
  {
    var vector = Vector3.zero;
    vector.x = Float(args, index, defaultValue.x);
    vector.y = Float(args, index + 2, defaultValue.y);
    vector.z = Float(args, index + 1, defaultValue.z);
    return vector;
  }
  public static Vector3? VectorXZYNull(string arg) => VectorXZYNull(Split(arg));
  public static Vector3? VectorXZYNull(string[] args)
  {
    var x = FloatNull(args, 0);
    var y = FloatNull(args, 2);
    var z = FloatNull(args, 1);
    if (x == null || y == null || z == null) return null;
    return new(x.Value, y.Value, z.Value);
  }
  public static Vector3 VectorZYX(string arg) => VectorZYX(Split(arg), 0, Vector3.zero);
  public static Vector3 VectorZYX(string[] args, int index, Vector3 defaultValue)
  {
    var vector = Vector3.zero;
    vector.x = Float(args, index + 2, defaultValue.z);
    vector.y = Float(args, index + 1, defaultValue.y);
    vector.z = Float(args, index, defaultValue.x);
    return vector;
  }
  public static Range<Vector3> VectorZYXRange(string arg, Vector3 defaultValue)
  {
    var parts = Split(arg);
    var x = FloatRange(parts, 2, defaultValue.x);
    var y = FloatRange(parts, 1, defaultValue.y);
    var z = FloatRange(parts, 0, defaultValue.z);
    return ToVectorRange(x, y, z);
  }
  public static Range<Vector3Int> VectorIntZYXRange(string arg, Vector3Int defaultValue)
  {
    var parts = Split(arg);
    var x = IntRange(parts, 2, defaultValue.x);
    var y = IntRange(parts, 1, defaultValue.y);
    var z = IntRange(parts, 0, defaultValue.z);
    return ToVectorRange(x, y, z);
  }
  public static Range<Vector3> VectorXZYRange(string arg, Vector3 defaultValue)
  {
    var parts = Split(arg);
    var x = FloatRange(parts, 0, defaultValue.x);
    var y = FloatRange(parts, 2, defaultValue.y);
    var z = FloatRange(parts, 1, defaultValue.z);
    return ToVectorRange(x, y, z);
  }
  public static Vector2i Vector2Int(string arg)
  {
    var parts = SplitWithEmpty(arg);
    return new(Int(parts[0]), parts.Length > 1 ? Int(parts[1]) : 0);
  }
  ///<summary>Parses ZXY vector starting at zero index. Zero is used for missing values.</summary>
  public static Vector3 VectorZXY(string[] args) => VectorZXY(args, 0, Vector3.zero);
  ///<summary>Parses ZXY vector starting at zero index. Default values is used for missing values.</summary>
  public static Vector3 VectorZXY(string[] args, Vector3 defaultValue) => VectorZXY(args, 0, defaultValue);
  ///<summary>Parses ZXY vector starting at given index. Zero is used for missing values.</summary>
  public static Vector3 VectorZXY(string[] args, int index) => VectorZXY(args, index, Vector3.zero);
  ///<summary>Parses ZXY vector starting at given index. Default values is used for missing values.</summary>
  public static Vector3 VectorZXY(string[] args, int index, Vector3 defaultValue)
  {
    var vector = Vector3.zero;
    vector.x = Float(args, index + 1, defaultValue.x);
    vector.y = Float(args, index + 2, defaultValue.y);
    vector.z = Float(args, index, defaultValue.z);
    return vector;
  }
  public static Range<Vector3> VectorZXYRange(string arg, Vector3 defaultValue)
  {
    var parts = Split(arg);
    var x = FloatRange(parts, 1, defaultValue.x);
    var y = FloatRange(parts, 2, defaultValue.y);
    var z = FloatRange(parts, 0, defaultValue.z);
    return ToVectorRange(x, y, z);
  }
  private static Range<Vector3> ToVectorRange(Range<float> x, Range<float> y, Range<float> z)
  {
    Vector3 min = new(x.Min, y.Min, z.Min);
    Vector3 max = new(x.Max, y.Max, z.Max);
    return new(min, max);
  }
  private static Range<Vector3Int> ToVectorRange(Range<int> x, Range<int> y, Range<int> z)
  {
    Vector3Int min = new(x.Min, y.Min, z.Min);
    Vector3Int max = new(x.Max, y.Max, z.Max);
    return new(min, max);
  }
  public static Vector3 VectorYXZ(string arg) => VectorYXZ(Split(arg), 0, Vector3.zero);
  ///<summary>Parses YXZ vector starting at zero index. Zero is used for missing values.</summary>
  public static Vector3 VectorYXZ(string[] args) => VectorYXZ(args, 0, Vector3.zero);
  ///<summary>Parses YXZ vector starting at zero index. Default values is used for missing values.</summary>
  public static Vector3 VectorYXZ(string[] args, Vector3 defaultValue) => VectorYXZ(args, 0, defaultValue);
  ///<summary>Parses YXZ vector starting at given index. Zero is used for missing values.</summary>
  public static Vector3 VectorYXZ(string[] args, int index) => VectorYXZ(args, index, Vector3.zero);
  ///<summary>Parses YXZ vector starting at given index. Default values is used for missing values.</summary>
  public static Vector3 VectorYXZ(string[] args, int index, Vector3 defaultValue)
  {
    var vector = Vector3.zero;
    vector.y = Float(args, index, defaultValue.y);
    vector.x = Float(args, index + 1, defaultValue.x);
    vector.z = Float(args, index + 2, defaultValue.z);
    return vector;
  }
  public static Range<Vector3> VectorYXZRange(string arg, Vector3 defaultValue)
  {
    var parts = Split(arg);
    var x = FloatRange(parts, 1, defaultValue.x);
    var y = FloatRange(parts, 0, defaultValue.y);
    var z = FloatRange(parts, 2, defaultValue.z);
    return ToVectorRange(x, y, z);
  }
  ///<summary>Parses scale starting at zero index. Includes a sanity check and giving a single value for all axis.</summary>
  public static Vector3 Scale(string[] args) => Scale(args, 0);
  ///<summary>Parses scale starting at given index. Includes a sanity check and giving a single value for all axis.</summary>
  public static Vector3 Scale(string[] args, int index) => SanityCheck(VectorXZY(args, index));
  private static Vector3 SanityCheck(Vector3 scale)
  {
    // Sanity check and also adds support for setting all values with a single number.
    if (scale.x == 0) scale.x = 1;
    if (scale.y == 0) scale.y = scale.x;
    if (scale.z == 0) scale.z = scale.x;
    return scale;
  }
  public static Range<Vector3> ScaleRange(string arg)
  {
    var parts = Split(arg);
    var x = FloatRange(parts, 0, 0f);
    var y = FloatRange(parts, 2, 0f);
    var z = FloatRange(parts, 1, 0f);
    var range = ToVectorRange(x, y, z);
    range.Min = SanityCheck(range.Min);
    range.Max = SanityCheck(range.Max);
    range.Uniform = parts.Length < 4;
    return range;
  }

  public static string[] SplitWithEscape(string arg, char separator = ',')
  {
    var parts = new List<string>();
    var split = arg.Split(separator);
    for (var i = 0; i < split.Length; i++)
    {
      var part = split[i].TrimStart();
      // Escape should only work if at start/end of the string.
      if (part.StartsWith("\""))
      {
        split[i] = part.Substring(1);
        var j = i;
        for (; j < split.Length; j++)
        {
          part = split[j].TrimEnd();
          if (part.EndsWith("\""))
          {
            split[j] = part.Substring(0, part.Length - 1);
            break;
          }
        }
        parts.Add(string.Join(separator.ToString(), split.Skip(i).Take(j - i + 1)));
        i = j;
        continue;
      }
      parts.Add(split[i].Trim());
    }
    return [.. parts];
  }
  public static KeyValuePair<string, string> Kvp(string str, char separator = ',')
  {
    var split = str.Split([separator], 2);
    return split.Length < 2 ? new("", "") : new(split[0], split[1].Trim());
  }
  public static string[] SplitWithEmpty(string arg, char separator = ',') => arg.Split(separator).Select(static s => s.Trim()).ToArray();
  public static string[] Split(string arg, char separator = ',') => arg.Split(separator).Select(static s => s.Trim()).Where(static s => s != "").ToArray();
  public static string[] Split(string[] args, int index, char separator)
  {
    if (args.Length <= index) return [];
    return Split(args[index], separator);
  }
  private static readonly HashSet<string> Truthies = [
    "1",
    "t",
    "true",
    "yes",
    "on"
  ];
  private static bool IsTruthy(string value) => Truthies.Contains(value);
  private static readonly HashSet<string> Falsies = [
    "0",
    "f",
    "false",
    "no",
    "off"
  ];
  private static bool IsFalsy(string value) => Falsies.Contains(value);
  public static bool? Boolean(string value)
  {
    if (IsTruthy(value)) return true;
    if (IsFalsy(value)) return false;
    return Helper.IsDown(value);
  }
  public static bool? BoolNull(string? arg)
  {
    if (arg == null) return null;
    arg = arg.ToLower();
    if (IsTruthy(arg)) return true;
    if (IsFalsy(arg)) return false;
    return null;
  }
  public static string Logic(string value)
  {
    if (!value.Contains("?") || !value.Contains(":")) return value;
    var split = value.Split('?');
    var condition = split[0];
    split = split[1].Split(':');
    var truth = Boolean(condition) ?? false;
    return truth ? split[0] : split[1];
  }

  public static float Multiplier(string value)
  {
    var multiplier = 1f;
    var split = value.Split('*');
    foreach (var str in split) multiplier *= Float(str, 1f);
    return multiplier;
  }
  public static float TryMultiplier(string[] args, int index, float defaultValue = 1f)
  {
    if (args.Length <= index) return defaultValue;
    return Multiplier(args[index]);
  }
  public static float Direction(string[] args, int index) => args.Length <= index ? 1f : Direction(args[index]);
  public static float Direction(string arg) => Float(arg, 1) > 0 ? 1f : -1f;
}
