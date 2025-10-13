// Assets/Scripts/UI/MainMenuMinimal.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using GridMaze.Data;
using GridMaze.Sessions;

namespace GridMaze.UI {
  public class MainMenuMinimal : MonoBehaviour {
    [Header("Buttons")]
    public Button goalNavButton;
    public Button abcdButton;
    public Button optionsButton;
    public Button infoButton;

    [Header("Popups")]
    public GameObject namePopup;
    public TMP_InputField nameInput;
    public Button nameOkButton;
    public Button nameCancelButton;

    public GameObject pretrainPopup;
    public Button pretrainYesButton;
    public Button pretrainNoButton;

    [Header("Panels")]
    public GameObject optionsPanel;
    public GameObject infoPanel;

    [Header("Scenes")]
    public string tutorialScene = "Tutorial";
    public string goalScene = "BigMaze";
    public string abcdScene = "SmallMaze";

    enum PendingTask { None, Goal, ABCD }
    PendingTask pendingTask = PendingTask.None;

    void Awake() {
      // Ensure GameSession singleton exists
      if (GameSession.I == null) {
        var go = new GameObject("_GameSession");
        go.AddComponent<GameSession>();
      }
    }

    void Start() {
      // Wire buttons
      goalNavButton.onClick.AddListener(() => StartFlow(PendingTask.Goal));
      abcdButton.onClick.AddListener(() => StartFlow(PendingTask.ABCD));
      optionsButton.onClick.AddListener(() => { optionsPanel.SetActive(true); });
      infoButton.onClick.AddListener(() => { infoPanel.SetActive(true); });

      nameOkButton.onClick.AddListener(OnNameOk);
      nameCancelButton.onClick.AddListener(() => { namePopup.SetActive(false); pendingTask = PendingTask.None; });

      pretrainYesButton.onClick.AddListener(() => OnPretrainChosen(true));
      pretrainNoButton.onClick.AddListener(() => OnPretrainChosen(false));

      // Hide popups/panels
      namePopup.SetActive(false);
      pretrainPopup.SetActive(false);
      if (optionsPanel) optionsPanel.SetActive(false);
      if (infoPanel) infoPanel.SetActive(false);
    }

    void StartFlow(PendingTask task) {
      pendingTask = task;
      nameInput.text = "";
      namePopup.SetActive(true);
      nameInput.Select();
      nameInput.ActivateInputField();
    }

    void OnNameOk() {
      var subject = nameInput.text.Trim();
      if (string.IsNullOrEmpty(subject)) return;

      namePopup.SetActive(false);

      // If profile exists, skip pretrained popup and route immediately.
      if (ProfileStore.Exists(subject)) {
        var prof = ProfileStore.Load(subject);
        BeginSessionAndRoute(subject, prof.pretrained);
      } else {
        // First time → ask pretrained
        pretrainPopup.SetActive(true);
      }
    }

    void OnPretrainChosen(bool pretrained) {
      pretrainPopup.SetActive(false);
      var subject = nameInput.text.Trim();
      // Create profile with the chosen pretrained flag
      ProfileStore.CreateOrUpdate(subject, pretrained);
      BeginSessionAndRoute(subject, pretrained);
    }

    void BeginSessionAndRoute(string subject, bool pretrained) {
      var taskName = pendingTask == PendingTask.Goal ? "Goal navigation" : "ABCD";
      GameSession.I.StartSession(subject, taskName);

      if (!pretrained) {
        LoadSceneSafe(tutorialScene);
        return;
      }

      if (pendingTask == PendingTask.Goal) LoadSceneSafe(goalScene);
      else if (pendingTask == PendingTask.ABCD) LoadSceneSafe(abcdScene);
      pendingTask = PendingTask.None;
    }

    void LoadSceneSafe(string scene) {
#if UNITY_EDITOR
      if (Application.CanStreamedLevelBeLoaded(scene) == false) {
        Debug.LogError($"Scene '{scene}' isn’t in Build Settings. Add it there.");
      }
#endif
      SceneManager.LoadScene(scene);
    }

    // Optional: Close buttons on Options/Info panels can call these from the Inspector
    public void CloseOptions() { if (optionsPanel) optionsPanel.SetActive(false); }
    public void CloseInfo() { if (infoPanel) infoPanel.SetActive(false); }
  }
}
