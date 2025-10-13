using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GridMaze.Data;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PauseMenu : MonoBehaviour {
  [Header("UI")]
  [Tooltip("Full-screen overlay panel that contains the pause UI.")]
  public GameObject panel;                  // Assign in Inspector. If null, we try to auto-find.
  public string mainMenuScene = "MainMenuMinimal";
  public bool manageCursor = false;         // true for desktop, false for VR

  [Header("Keys")]
  public KeyCode pauseToggleKey1 = KeyCode.Escape;
  public KeyCode pauseToggleKey2 = KeyCode.P;

  [Header("Movement/Input to disable on pause")]
#if ENABLE_INPUT_SYSTEM
  [Tooltip("Optional: InputActionAsset for the new Input System; relevant action maps will be disabled on pause.")]
  public InputActionAsset inputActions;
  [Tooltip("Action maps to disable on pause (e.g., Player, Gameplay, XR).")]
  public string[] actionMapsToDisable = new[] { "Player", "Gameplay", "XR" };
#endif

  [Tooltip("Optional: components to disable on pause (legacy controllers, custom movers, XR providers, etc.).")]
  public Behaviour[] componentsToDisable;

  public static bool IsPaused { get; private set; }

  DataRecorder recorder;
  readonly List<Behaviour> reEnableList = new List<Behaviour>();
  float prevTimeScale = 1f;

  void Start() {
    // Try to auto-wire common mistakes
    if (panel == null) {
      var found = GameObject.Find("PauseOverlay") ?? GameObject.Find("PausePanel");
      if (found) panel = found;
      else Debug.LogWarning("PauseMenu: No panel assigned and none auto-found. Assign your overlay panel.");
    }
    if (panel) panel.SetActive(false);

    recorder = FindObjectOfType<DataRecorder>(true);

    // Ensure there is an EventSystem in the scene for UI clicks
    if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null) {
      var es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
      DontDestroyOnLoad(es); // safe enough for a quick fix
    }
  }

  void Update() {
    if (Input.GetKeyDown(pauseToggleKey1) || Input.GetKeyDown(pauseToggleKey2)) {
      if (IsPaused) Resume();
      else Pause();
    }
  }

  void RefreshRecorder() {
     if (recorder == null) recorder = FindObjectOfType<GridMaze.Data.DataRecorder>(true);
   }

  public void Pause() {
    RefreshRecorder();
    IsPaused = true;
    Debug.Log("[PauseMenu] ==== GAME PAUSED ====");

    if (panel) panel.SetActive(true);

    prevTimeScale = Time.timeScale;
    Time.timeScale = 0f;
    AudioListener.pause = true;

    if (recorder) recorder.SetPaused(true);

#if ENABLE_INPUT_SYSTEM
    if (inputActions != null) {
      foreach (var mapName in actionMapsToDisable) {
        var map = inputActions.FindActionMap(mapName, throwIfNotFound: false);
        if (map != null && map.enabled) map.Disable();
      }
    }
#endif
    reEnableList.Clear();
    if (componentsToDisable != null) {
      foreach (var c in componentsToDisable) {
        if (c != null && c.enabled) {
          c.enabled = false;
          reEnableList.Add(c);
        }
      }
    }

    if (manageCursor) {
      Cursor.visible = true;
      Cursor.lockState = CursorLockMode.None;
    }

    Debug.Log("Paused.");
  }

  public void Resume() {
    RefreshRecorder();
    IsPaused = false;
    Debug.Log("[PauseMenu] ==== GAME RESUMED ====");

    if (panel) panel.SetActive(false);

    Time.timeScale = prevTimeScale;
    AudioListener.pause = false;

    if (recorder) recorder.SetPaused(false);

#if ENABLE_INPUT_SYSTEM
    if (inputActions != null) {
      foreach (var mapName in actionMapsToDisable) {
        var map = inputActions.FindActionMap(mapName, throwIfNotFound: false);
        if (map != null && !map.enabled) map.Enable();
      }
    }
#endif
    foreach (var c in reEnableList) {
      if (c) c.enabled = true;
    }
    reEnableList.Clear();

    if (manageCursor) {
      Cursor.visible = false;
      Cursor.lockState = CursorLockMode.Locked;
    }

    Debug.Log("Resumed.");
  }

    public void ExitToMainMenu() {
    // 1) Make sure we have the recorder (don't rely on a stale cached reference)
    if (recorder == null) recorder = FindObjectOfType<GridMaze.Data.DataRecorder>(true);

    // 2) Explicitly unpause everything *before* saving (harmless but robust)
    if (IsPaused) {
        IsPaused = false;
        if (panel) panel.SetActive(false);
    }
    AudioListener.pause = false;
    Time.timeScale = 1f;
    if (recorder) recorder.SetPaused(false);

    // 3) Save now (with loud logs inside DataRecorder)
    if (recorder) {
        Debug.Log("[PauseMenu] Exiting → EndAndSave()");
        recorder.EndAndSave();
    } else {
        Debug.LogWarning("[PauseMenu] No DataRecorder found when exiting. Nothing to save.");
    }

    // 4) Go to main menu
    SceneManager.LoadScene(mainMenuScene);
    }

}
