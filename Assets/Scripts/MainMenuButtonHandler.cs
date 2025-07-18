using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuButtonHandler : MonoBehaviour
{
    [Header("Main Buttons")]
    public Button goalNavigationButton;
    public Button abcdTaskButton;
    public Button optionsButton;
    public Button infoButton;

    [Header("Panels")]
    public GameObject preTrainingPanel;
    public GameObject subjectSetupPanel;
    public GameObject optionsPanel;

    [Header("PreTraining Buttons")]
    public Button preTrainingYesButton;
    public Button preTrainingNoButton;

    [Header("Subject Setup")]
    public InputField subjectIDInput;
    public Dropdown mazeConfigDropdown;
    public Button subjectSetupStartButton;

    [Header("Options Setup")]
    public Toggle mouseVisionToggle;
    public InputField forwardKeyInput;
    public InputField backwardKeyInput;
    public InputField leftKeyInput;
    public InputField rightKeyInput;
    public InputField interactKeyInput;

    private string selectedScene = "";
    private string selectedMazeConfig = "";

    void Start()
    {
        // Main menu button listeners
        goalNavigationButton.onClick.AddListener(() => OnGameSelected("GoalNavigationScene"));
        abcdTaskButton.onClick.AddListener(() => OnGameSelected("ABCDTaskScene"));
        optionsButton.onClick.AddListener(OpenOptions);
        infoButton.onClick.AddListener(OpenInfo);

        // Pretraining panel button listeners
        preTrainingYesButton.onClick.AddListener(OnPreTrainedYes);
        preTrainingNoButton.onClick.AddListener(OnPreTrainedNo);

        // Subject setup button listener
        subjectSetupStartButton.onClick.AddListener(OnSubjectSetupComplete);

        // Options toggles
        mouseVisionToggle.onValueChanged.AddListener(OnMouseVisionToggle);
    }

    void OnGameSelected(string sceneName)
    {
        selectedScene = sceneName;
        preTrainingPanel.SetActive(true);
    }

    void OpenOptions()
    {
        optionsPanel.SetActive(true);
    }

    void OpenInfo()
    {
        // For now, just log info
        Debug.Log("Info button clicked.");
    }

    void OnPreTrainedYes()
    {
        preTrainingPanel.SetActive(false);
        subjectSetupPanel.SetActive(true);
    }

    void OnPreTrainedNo()
    {
        preTrainingPanel.SetActive(false);
        // Load tutorial scene instead
        SceneManager.LoadScene("TutorialScene");
    }

    void OnSubjectSetupComplete()
    {
        string subjectID = subjectIDInput.text;
        selectedMazeConfig = mazeConfigDropdown.options[mazeConfigDropdown.value].text;

        // Store subjectID and maze config (maybe use a GameManager or static class)

        // Then load the selected scene
        SceneManager.LoadScene(selectedScene);
    }

    void OnMouseVisionToggle(bool enabled)
    {
        Debug.Log("Mouse Vision: " + (enabled ? "On" : "Off"));
    }
}
