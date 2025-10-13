// Assets/Scripts/Data/ProfileStore.cs
using UnityEngine;
using System.IO;

namespace GridMaze.Data {
  [System.Serializable]
  public class PlayerProfile {
    public string subject;
    public bool pretrained;
    public long createdUtcTicks;
    public long lastSeenUtcTicks;
  }

  public static class ProfileStore {
    static string Dir => Path.Combine(Application.persistentDataPath, "Profiles");

    static string PathFor(string subject) {
      Directory.CreateDirectory(Dir);
      var safe = string.Join("_", subject.Split(Path.GetInvalidFileNameChars()));
      return System.IO.Path.Combine(Dir, safe + ".json");
    }

    public static bool Exists(string subject) {
      return File.Exists(PathFor(subject));
    }

    public static PlayerProfile Load(string subject) {
      var p = PathFor(subject);
      if (!File.Exists(p)) return null;
      var json = File.ReadAllText(p);
      var prof = JsonUtility.FromJson<PlayerProfile>(json);
      prof.lastSeenUtcTicks = System.DateTime.UtcNow.Ticks;
      File.WriteAllText(p, JsonUtility.ToJson(prof, true));
      return prof;
    }

    public static PlayerProfile CreateOrUpdate(string subject, bool pretrained) {
      var p = PathFor(subject);
      PlayerProfile prof;
      if (File.Exists(p)) {
        prof = JsonUtility.FromJson<PlayerProfile>(File.ReadAllText(p));
        prof.pretrained = pretrained; // allow updating
        prof.lastSeenUtcTicks = System.DateTime.UtcNow.Ticks;
      } else {
        prof = new PlayerProfile {
          subject = subject, pretrained = pretrained,
          createdUtcTicks = System.DateTime.UtcNow.Ticks,
          lastSeenUtcTicks = System.DateTime.UtcNow.Ticks
        };
      }
      File.WriteAllText(p, JsonUtility.ToJson(prof, true));
      return prof;
    }
  }
}
