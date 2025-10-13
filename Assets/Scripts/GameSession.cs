using UnityEngine;
using System;

namespace GridMaze.Sessions {
  public class GameSession : MonoBehaviour {
    public static GameSession I { get; private set; }
    public string SubjectId { get; private set; }
    public string TaskName { get; private set; }
    public string SessionId { get; private set; }
    public DateTime StartUtc { get; private set; }
    public DateTime EndUtc { get; private set; }
    public int RewardsCollected { get; private set; }

    void Awake() {
      if (I != null && I != this) { Destroy(gameObject); return; }
      I = this; DontDestroyOnLoad(gameObject);
    }

    public void StartSession(string subjectId, string taskName) {
      SubjectId = string.IsNullOrWhiteSpace(subjectId) ? "anon" : subjectId.Trim();
      TaskName = string.IsNullOrWhiteSpace(taskName) ? "default" : taskName.Trim();
      SessionId = Guid.NewGuid().ToString("N");
      StartUtc = DateTime.UtcNow;
      RewardsCollected = 0;
    }

    public void IncrementRewards() => RewardsCollected++;

    public void EndSession() { EndUtc = DateTime.UtcNow; }
  }
}
