using HarmonyLib;

namespace Service {
  [HarmonyPatch]
  public class GetValue {
    private static T Get<T>(object obj, string field) => Traverse.Create(obj).Field<T>(field).Value;
    public static bool Cheat(Terminal obj) => Get<bool>(obj, "m_cheat");
  }

}