using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MainMenuUIBuilder : MonoBehaviour
{
    public Font defaultFont;
    public Material defaultFontMaterial;

    void Start()
    {
        CreateUI();
    }

    void CreateUI()
    {
        // Ensure there’s an EventSystem
        if (!FindObjectOfType<EventSystem>())
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        // Create Canvas
        GameObject canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler cs = canvasGO.GetComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);

        // Create black background panel
        GameObject bg = CreateUIObj("Background", canvasGO.transform);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = Color.black;
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Create Main Menu Panel
        GameObject mainPanel = CreatePanel("Main Panel", canvasGO.transform, new Vector2(600, 400));
        mainPanel.AddComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

        string[] menuButtons = { "Goal Navigation", "ABCD Task", "Options", "Info" };

        foreach (string btnLabel in menuButtons)
        {
            GameObject btn = CreateTextButton(btnLabel, mainPanel.transform);
        }

        // Create popups (disabled by default)
        CreatePopup(canvasGO.transform, "Pretrain Prompt Panel", "Have you been pre-trained?", new string[] { "Yes", "No" });
        CreateMazeConfigPopup(canvasGO.transform);
        CreateOptionsPopup(canvasGO.transform);
    }

    GameObject CreatePanel(string name, Transform parent, Vector2 size)
    {
        GameObject panel = CreateUIObj(name, parent);
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.8f);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        return panel;
    }

    GameObject CreateTextButton(string label, Transform parent)
    {
        GameObject btnGO = CreateUIObj(label + " Button", parent);
        Button btn = btnGO.AddComponent<Button>();
        Image img = btnGO.AddComponent<Image>();
        img.color = Color.clear;

        GameObject txtGO = CreateUIObj("Text", btnGO.transform);
        TMP_Text tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 36;
        tmp.color = Color.white;
        RectTransform txtRT = txtGO.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;

        // Hover color change
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.clear;
        colors.highlightedColor = Color.gray;
        colors.pressedColor = new Color(0.5f, 0.5f, 0.5f);
        btn.colors = colors;

        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(500, 80);
        return btnGO;
    }

    GameObject CreatePopup(Transform parent, string name, string promptText, string[] buttonLabels)
    {
        GameObject popup = CreatePanel(name, parent, new Vector2(500, 300));
        popup.SetActive(false);

        GameObject textGO = CreateUIObj("Prompt", popup.transform);
        TMP_Text txt = textGO.AddComponent<TextMeshProUGUI>();
        txt.text = promptText;
        txt.alignment = TextAlignmentOptions.Center;
        txt.fontSize = 28;
        txt.color = Color.white;
        RectTransform txtRT = textGO.GetComponent<RectTransform>();
        txtRT.anchorMin = new Vector2(0.1f, 0.6f);
        txtRT.anchorMax = new Vector2(0.9f, 0.9f);
        txtRT.offsetMin = txtRT.offsetMax = Vector2.zero;

        for (int i = 0; i < buttonLabels.Length; i++)
        {
            GameObject btn = CreateTextButton(buttonLabels[i], popup.transform);
            RectTransform rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.2f, 0.3f - 0.1f * i);
            rt.anchorMax = new Vector2(0.8f, 0.4f - 0.1f * i);
        }

        return popup;
    }

    void CreateMazeConfigPopup(Transform parent)
    {
        GameObject panel = CreatePanel("Maze Config Panel", parent, new Vector2(600, 400));
        panel.SetActive(false);

        GameObject subjectLabel = CreateText("Subject ID:", panel.transform, new Vector2(0, 100));
        GameObject subjectInput = CreateUIObj("SubjectID_Input", panel.transform);
        TMP_InputField input = subjectInput.AddComponent<TMP_InputField>();
        subjectInput.AddComponent<Image>().color = Color.white;
        RectTransform inputRT = subjectInput.GetComponent<RectTransform>();
        inputRT.sizeDelta = new Vector2(400, 40);
        inputRT.anchoredPosition = new Vector2(0, 60);

        GameObject mazeLabel = CreateText("Select Maze Config:", panel.transform, new Vector2(0, 20));
        GameObject dropdownGO = CreateUIObj("MazeConfig_Dropdown", panel.transform);
        TMP_Dropdown dropdown = dropdownGO.AddComponent<TMP_Dropdown>();
        dropdown.options.Add(new TMP_Dropdown.OptionData("Maze A"));
        dropdown.options.Add(new TMP_Dropdown.OptionData("Maze B"));
        dropdownGO.AddComponent<Image>().color = Color.white;
        RectTransform ddRT = dropdownGO.GetComponent<RectTransform>();
        ddRT.sizeDelta = new Vector2(400, 40);
        ddRT.anchoredPosition = new Vector2(0, -20);

        CreateTextButton("Start Game", panel.transform);
    }

    void CreateOptionsPopup(Transform parent)
    {
        GameObject panel = CreatePanel("Options Panel", parent, new Vector2(600, 500));
        panel.SetActive(false);

        string[] toggles = { "Enable Mouse Vision" };
        foreach (string label in toggles)
        {
            GameObject toggleGO = CreateUIObj(label + "_Toggle", panel.transform);
            Toggle toggle = toggleGO.AddComponent<Toggle>();
            toggleGO.AddComponent<Image>().color = Color.clear;

            GameObject labelGO = CreateText(label, toggleGO.transform, Vector2.zero);
            labelGO.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Left;
        }

        string[] keys = { "Forward", "Backward", "Left", "Right", "Interact" };
        foreach (string key in keys)
        {
            GameObject keyLabel = CreateText(key + " Key:", panel.transform, Vector2.zero);
            GameObject keyInput = CreateUIObj(key + "_Input", panel.transform);
            TMP_InputField input = keyInput.AddComponent<TMP_InputField>();
            keyInput.AddComponent<Image>().color = Color.white;
            RectTransform rt = keyInput.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 30);
        }

        CreateTextButton("Save & Close", panel.transform);
    }

    GameObject CreateText(string content, Transform parent, Vector2 position)
    {
        GameObject txtGO = CreateUIObj(content + "_Label", parent);
        TMP_Text txt = txtGO.AddComponent<TextMeshProUGUI>();
        txt.text = content;
        txt.alignment = TextAlignmentOptions.Center;
        txt.fontSize = 24;
        txt.color = Color.white;
        RectTransform rt = txtGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(500, 40);
        rt.anchoredPosition = position;
        return txtGO;
    }

    GameObject CreateUIObj(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }
}
