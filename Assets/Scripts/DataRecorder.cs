// Assets/Scripts/Data/DataRecorder.cs
using UnityEngine;
using System.Text;
using System.IO;
using System.Globalization;
using GridMaze.Sessions;
using UnityEngine.SceneManagement;

namespace GridMaze.Data {
  [DisallowMultipleComponent]
  public class DataRecorder : MonoBehaviour {
    [Header("Targets")]
    [Tooltip("Player rig root (e.g., XR Origin). If null, will try head.root.")]
    public Transform playerRoot;
    [Tooltip("Head/camera transform (CenterEye or Camera.main).")]
    public Transform head;

    [Header("Sampling")]
    [Range(10,240)] public int targetHz = 60;

    // Runtime
    StringBuilder navCsv;
    StringBuilder eventsCsv;
    float accumulator;
    float dtTarget;
    bool recording;
    bool paused;

    string dir;
    string navPath, eventsPath, metaPath;

    public static string LastSessionDir { get; private set; }

    void OnEnable() {
      // Prepare sampling cadence
      dtTarget = 1f / Mathf.Max(1, targetHz);
      accumulator = 0f;

      // Ensure we have a session; create fallback if needed (useful when Play is pressed in a gameplay scene)
      var s = GameSession.I;
      if (s == null) {
        var go = new GameObject("_GameSession_Fallback");
        var gs = go.AddComponent<GameSession>();
        string sceneName = SceneManager.GetActiveScene().name;
        gs.StartSession("anon", string.IsNullOrEmpty(sceneName) ? "default" : sceneName);
        s = GameSession.I;
        Debug.LogWarning("[DataRecorder] No GameSession found – started fallback session.");
      }

      // Prepare builders + headers
      navCsv = new StringBuilder(1 << 20);
      eventsCsv = new StringBuilder(1 << 16);
      navCsv.AppendLine("t,px,py,pz,rx,ry,rz,fx,fy,fz");
      eventsCsv.AppendLine("t,event,info");

      // Resolve transforms if not assigned
      if (head == null && Camera.main != null) head = Camera.main.transform;
      if (playerRoot == null && head != null) playerRoot = head.root;

      // File paths
      dir = Path.Combine(Application.persistentDataPath, "Sessions", s.SessionId);
      Directory.CreateDirectory(dir);
      navPath    = Path.Combine(dir, $"navigation_{s.SessionId}.csv");
      eventsPath = Path.Combine(dir, $"events_{s.SessionId}.csv");
      metaPath   = Path.Combine(dir, $"session_meta.json");
      LastSessionDir = dir;

      Debug.Log($"[DataRecorder] Recording to:\n{navPath}\n{eventsPath}\n{metaPath}\n[persistentDataPath]\n{Application.persistentDataPath}");

      // Subscriptions
      GameEvents.OnPoke   += HandlePoke;
      GameEvents.OnReward += HandleReward;
      GameEvents.OnCustom += HandleCustom;

      // Mark start
      WriteEventRow("TASK_START", s.TaskName);

      recording = true;
      paused = false;

      SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    void OnDisable() {
      // Unsubscribe safely
      GameEvents.OnPoke   -= HandlePoke;
      GameEvents.OnReward -= HandleReward;
      GameEvents.OnCustom -= HandleCustom;

      SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    void Update() {
      if (!recording || paused) return;

      // Lazy resolve (handles Camera replacing at runtime)
      if (head == null && Camera.main != null) head = Camera.main.transform;
      if (playerRoot == null && head != null) playerRoot = head.root;
      if (head == null || playerRoot == null) return;

      // Wall-clock style sampling (independent of Time.timeScale)
      accumulator += Time.unscaledDeltaTime;
      while (accumulator >= dtTarget) {
        accumulator -= dtTarget;
        Sample();
      }
    }

    void Sample() {
      // t uses Time.time so it's relative to scene time; change to unscaledTime if you prefer
      float t = Time.time;
      Vector3 p = playerRoot ? playerRoot.position : Vector3.zero;
      Vector3 e = head.rotation.eulerAngles;
      Vector3 f = head.forward;

      navCsv.AppendFormat(CultureInfo.InvariantCulture,
        "{0:F6},{1:F6},{2:F6},{3:F6},{4:F3},{5:F3},{6:F3},{7:F6},{8:F6},{9:F6}\n",
        t, p.x, p.y, p.z, e.x, e.y, e.z, f.x, f.y, f.z);
    }

    void HandlePoke(string info)   => WriteEventRow("POKE", info);
    void HandleReward(string info) => WriteEventRow("REWARD", info);
    void HandleCustom(string info) => WriteEventRow("CUSTOM", info);

    void WriteEventRow(string evt, string info) {
      float t = Time.time; // scene time
      if (string.IsNullOrEmpty(info)) info = "";
      info = info.Replace(",", ";");
      eventsCsv.AppendFormat(CultureInfo.InvariantCulture, "{0:F6},{1},{2}\n", t, evt, info);
    }

    public void SetPaused(bool p) {
      paused = p;
    }

    public void EndAndSave() {
      if (!recording) return; // already saved
      recording = false;

      var s = GameSession.I;
      if (s != null) s.EndSession();
      WriteEventRow("TASK_END", s != null ? s.TaskName : "unknown");

      try {
        File.WriteAllText(navPath,    navCsv.ToString(), Encoding.UTF8);
        File.WriteAllText(eventsPath, eventsCsv.ToString(), Encoding.UTF8);

        var meta = new SessionMeta {
          subject = s?.SubjectId ?? "anon",
          task = s?.TaskName ?? "unknown",
          sessionId = s?.SessionId ?? "no-session",
          startUtc = s?.StartUtc.ToString("o"),
          endUtc = s?.EndUtc.ToString("o"),
          rewards = s?.RewardsCollected ?? 0,
          app = Application.productName,
          version = Application.version,
          unity = Application.unityVersion
        };
        File.WriteAllText(metaPath, JsonUtility.ToJson(meta, true), Encoding.UTF8);

        Debug.Log($"[DataRecorder] Saved session to:\n{dir}\n- {Path.GetFileName(navPath)}\n- {Path.GetFileName(eventsPath)}\n- {Path.GetFileName(metaPath)}");
      }
      catch (System.Exception ex) {
        Debug.LogError($"[DataRecorder] Failed to save session to {dir}\n{ex}");
      }
    }

    void OnApplicationQuit() {
      if (recording) {
        Debug.Log("[DataRecorder] OnApplicationQuit → flushing.");
        EndAndSave();
      }
    }

    void OnDestroy() {
      if (recording) {
        Debug.Log("[DataRecorder] OnDestroy → flushing.");
        EndAndSave();
      }
    }

    void OnActiveSceneChanged(Scene oldScene, Scene newScene) {
      if (recording) {
        Debug.Log("[DataRecorder] Scene change → flushing session.");
        EndAndSave();
      }
    }

    [System.Serializable]
    class SessionMeta {
      public string subject;
      public string task;
      public string sessionId;
      public string startUtc;
      public string endUtc;
      public int rewards;
      public string app;
      public string version;
      public string unity;
    }

#if UNITY_EDITOR
    [ContextMenu("Reveal Session Folder")]
    void RevealFolder() {
      try {
        if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir)) {
          UnityEditor.EditorUtility.RevealInFinder(dir);
        } else {
          UnityEditor.EditorUtility.RevealInFinder(Application.persistentDataPath);
        }
      } catch { /* ignore */ }
    }
#endif
  }
}
