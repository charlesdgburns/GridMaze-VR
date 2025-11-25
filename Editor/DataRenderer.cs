using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class CSVFrameRenderer : EditorWindow
{
    public string trajectoriesPath = "Assets/Data/frames.trajectories.htsv";
    public string trialInfoPath = "Assets/Data/frames.trialInfo.htsv";
    public string outputFolder = "Screenshots/";
    public GameObject character;
    public GameObject mazeParent;
    public Material cueOnMaterial;
    public Material cueOffMaterial;
    public int frameDelay = 5;
    
    private bool isRendering = false;

    [MenuItem("Tools/CSV Frame Renderer")]
    static void ShowWindow()
    {
        GetWindow<CSVFrameRenderer>("CSV Renderer");
    }

    void OnGUI()
    {
        GUILayout.Label("CSV Frame Renderer", EditorStyles.boldLabel);
        
        trajectoriesPath = EditorGUILayout.TextField("Trajectories CSV:", trajectoriesPath);
        trialInfoPath = EditorGUILayout.TextField("Trial Info CSV:", trialInfoPath);
        outputFolder = EditorGUILayout.TextField("Output Folder:", outputFolder);
        character = EditorGUILayout.ObjectField("Character:", character, typeof(GameObject), true) as GameObject;
        mazeParent = EditorGUILayout.ObjectField("Maze Parent:", mazeParent, typeof(GameObject), true) as GameObject;
        cueOnMaterial = EditorGUILayout.ObjectField("Cue On Material:", cueOnMaterial, typeof(Material), false) as Material;
        cueOffMaterial = EditorGUILayout.ObjectField("Cue Off Material:", cueOffMaterial, typeof(Material), false) as Material;
        frameDelay = EditorGUILayout.IntField("Frame Delay:", frameDelay);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Render Frames"))
        {
            RenderFrames();
        }
        
        if (isRendering)
        {
            EditorGUILayout.HelpBox("Rendering in progress...", MessageType.Info);
        }
    }

    void RenderFrames()
    {
        Debug.Log("=== Starting RenderFrames ===");
        
        if (character == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a character GameObject!", "OK");
            Debug.LogError("Character is not assigned!");
            return;
        }
        Debug.Log($"Character assigned: {character.name}");

        if (mazeParent == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign the mazeParent GameObject!", "OK");
            Debug.LogError("MazeParent is not assigned!");
            return;
        }
        Debug.Log($"MazeParent assigned: {mazeParent.name}");

        // Convert relative path to absolute if needed
        string fullTrajPath = Path.GetFullPath(trajectoriesPath);
        string fullTrialPath = Path.GetFullPath(trialInfoPath);
        
        Debug.Log($"Looking for trajectories file at: {fullTrajPath}");
        if (!File.Exists(fullTrajPath))
        {
            EditorUtility.DisplayDialog("Error", "Trajectories CSV file not found at: " + fullTrajPath, "OK");
            Debug.LogError($"Trajectories file not found at: {fullTrajPath}");
            return;
        }
        Debug.Log("Trajectories file found!");

        Debug.Log($"Looking for trial info file at: {fullTrialPath}");
        if (!File.Exists(fullTrialPath))
        {
            EditorUtility.DisplayDialog("Error", "Trial Info CSV file not found at: " + fullTrialPath, "OK");
            Debug.LogError($"Trial info file not found at: {fullTrialPath}");
            return;
        }
        Debug.Log("Trial info file found!");

        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
            Debug.Log($"Created output folder: {outputFolder}");
        }

        Debug.Log("Starting coroutine...");
        EditorCoroutineUtility.StartCoroutine(RenderFramesCoroutine(), this);
    }

    IEnumerator RenderFramesCoroutine()
    {
        Debug.Log("=== Coroutine Started ===");
        isRendering = true;

        // Read both CSV files
        string[] trajLines = File.ReadAllLines(trajectoriesPath);
        string[] trialLines = File.ReadAllLines(trialInfoPath);
        
        Debug.Log($"Trajectories file has {trajLines.Length} lines");
        Debug.Log($"Trial info file has {trialLines.Length} lines");

        if (trajLines.Length < 2 || trialLines.Length < 2)
        {
            EditorUtility.DisplayDialog("Error", "One or both CSV files are empty or have no data rows!", "OK");
            Debug.LogError("CSV files are empty or have no data!");
            isRendering = false;
            yield break;
        }

        // Parse headers to find column indices
        string[] trajHeaders = trajLines[0].Split('\t');
        string[] trialHeaders = trialLines[0].Split('\t');
        
        Debug.Log($"Trajectory headers: {string.Join(", ", trajHeaders)}");
        Debug.Log($"Trial info headers: {string.Join(", ", trialHeaders)}");

        int xIdx = System.Array.IndexOf(trajHeaders, "centroid_position.x");
        int yIdx = System.Array.IndexOf(trajHeaders, "centroid_position.y");
        int headDirIdx = System.Array.IndexOf(trajHeaders, "head_direction.value");
        int phaseIdx = System.Array.IndexOf(trialHeaders, "trial_phase");
        int goalIdx = System.Array.IndexOf(trialHeaders, "goal");
        
        Debug.Log($"Column indices - X:{xIdx}, Y:{yIdx}, HeadDir:{headDirIdx}, Phase:{phaseIdx}, Goal:{goalIdx}");

        if (xIdx == -1 || yIdx == -1 || headDirIdx == -1 || phaseIdx == -1 || goalIdx == -1)
        {
            EditorUtility.DisplayDialog("Error", "Could not find required columns in CSV files!", "OK");
            Debug.LogError("Missing required columns!");
            isRendering = false;
            yield break;
        }

        Debug.Log($"Starting render process...");
        int capturedFrames = 0;

        // Process each frame
        int maxRows = Mathf.Min(trajLines.Length, trialLines.Length);
        for (int i = 1; i < maxRows; i++)
        {
            string[] trajValues = trajLines[i].Split('\t');
            string[] trialValues = trialLines[i].Split('\t');

            if (trajValues.Length <= Mathf.Max(xIdx, yIdx, headDirIdx) ||
                trialValues.Length <= Mathf.Max(phaseIdx, goalIdx))
            {
                Debug.LogWarning($"Skipping row {i}: insufficient data");
                continue;
            }

            string trialPhase = trialValues[phaseIdx].Trim();
            
            // Only process ITI or navigation phases
            if (trialPhase != "ITI" && trialPhase != "navigation")
            {
                continue;
            }

            // Parse position and rotation
            float x = float.Parse(trajValues[xIdx].Trim());
            float y = float.Parse(trajValues[yIdx].Trim());
            float headDirection = float.Parse(trajValues[headDirIdx].Trim());

            // Update character position and rotation
            character.transform.position = new Vector3(x, character.transform.position.y, y);
            character.transform.rotation = Quaternion.Euler(0, headDirection, 0);

            // Handle cue lights
            if (trialPhase == "ITI")
            {
                // Turn off all cues
                TurnOffAllCues();
            }
            else if (trialPhase == "navigation")
            {
                string goal = trialValues[goalIdx].Trim();
                // Turn off all cues first, then activate the goal cue
                TurnOffAllCues();
                ActivateCue(goal);
            }

            // Force scene view to update
            SceneView.RepaintAll();
            
            // Wait for rendering to settle
            for (int j = 0; j < frameDelay; j++)
            {
                yield return null;
            }

            // Capture screenshot using original row number
            string filename = $"{outputFolder}frame_{i:0000}.png";
            ScreenCapture.CaptureScreenshot(filename);
            
            Debug.Log($"Captured frame for row {i}: phase={trialPhase}, goal={trialValues[goalIdx].Trim()}");
            capturedFrames++;
            
            // Show progress
            EditorUtility.DisplayProgressBar("Rendering Frames", 
                $"Frame {capturedFrames} (processing row {i}/{maxRows})", 
                (float)i / maxRows);
        }

        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("Complete", 
            $"Rendered {capturedFrames} frames to {outputFolder}", "OK");
        
        Debug.Log($"Rendering complete! Total frames captured: {capturedFrames}");
        isRendering = false;
    }

    void TurnOffAllCues()
    {
        // Iterate through all children of mazeParent
        foreach (Transform child in mazeParent.transform)
        {
            Transform cue = child.Find("Cue");
            if (cue != null)
            {
                ToggleCue(cue, false);
            }
        }
    }

    void ActivateCue(string towerName)
    {
        Transform tower = mazeParent.transform.Find(towerName);
        if (tower != null)
        {
            Transform cue = tower.Find("Cue");
            if (cue != null)
            {
                ToggleCue(cue, true);
            }
        }
        else
        {
            Debug.LogWarning($"Tower '{towerName}' not found under mazeParent");
        }
    }

    void ToggleCue(Transform cue, bool activate)
    {
        // The cue itself
        Renderer renderer = cue.GetComponent<Renderer>();
        Light cueLight = cue.Find("Light")?.GetComponent<Light>();
        
        if (renderer != null && cueOnMaterial != null && cueOffMaterial != null)
        {
            renderer.material = activate ? cueOnMaterial : cueOffMaterial;
        }
        
        if (cueLight != null)
        {
            cueLight.enabled = activate;
        }

        // The reflection
        Transform reflection = cue.Find("reflection");
        if (reflection != null)
        {
            Renderer reflectionRenderer = reflection.GetComponent<Renderer>();
            Light reflectionLight = reflection.GetComponent<Light>();
            
            if (reflectionRenderer != null && cueOnMaterial != null && cueOffMaterial != null)
            {
                reflectionRenderer.material = activate ? cueOnMaterial : cueOffMaterial;
            }
            
            if (reflectionLight != null)
            {
                reflectionLight.enabled = activate;
            }
        }
    }
}

// Helper class for Editor Coroutines
public static class EditorCoroutineUtility
{
    public static void StartCoroutine(IEnumerator routine, EditorWindow window)
    {
        window.StartCoroutine(routine);
    }

    private static IEnumerator StartCoroutine(this EditorWindow window, IEnumerator routine)
    {
        while (routine.MoveNext())
        {
            if (routine.Current is IEnumerator)
            {
                yield return window.StartCoroutine((IEnumerator)routine.Current);
            }
            else
            {
                yield return routine.Current;
            }
        }
    }
}