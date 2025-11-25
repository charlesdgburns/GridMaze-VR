using UnityEngine;
using UnityEditor;
using System.IO;
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
    private int currentFrameIndex = 0;
    private int framesWaited = 0;
    private List<FrameData> framesToRender = new List<FrameData>();
    private int capturedFrames = 0;

    private class FrameData
    {
        public int rowNumber;
        public float x;
        public float y;
        public float headDirection;
        public string trialPhase;
        public string goal;
    }

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
        
        GUI.enabled = !isRendering;
        if (GUILayout.Button("Render Frames"))
        {
            StartRendering();
        }
        GUI.enabled = true;
        
        if (isRendering)
        {
            EditorGUILayout.HelpBox($"Rendering in progress... Frame {capturedFrames}/{framesToRender.Count}", MessageType.Info);
            if (GUILayout.Button("Cancel"))
            {
                StopRendering();
            }
        }
    }

    void StartRendering()
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

        // Parse CSV files and prepare frames
        if (!PrepareFrames())
        {
            return;
        }

        Debug.Log($"Prepared {framesToRender.Count} frames to render");
        
        // Start rendering process
        currentFrameIndex = 0;
        framesWaited = 0;
        capturedFrames = 0;
        isRendering = true;
        EditorApplication.update += UpdateRendering;
        
        Debug.Log("Rendering started!");
    }

    bool PrepareFrames()
    {
        framesToRender.Clear();

        // Read both CSV files
        string[] trajLines = File.ReadAllLines(trajectoriesPath);
        string[] trialLines = File.ReadAllLines(trialInfoPath);
        
        Debug.Log($"Trajectories file has {trajLines.Length} lines");
        Debug.Log($"Trial info file has {trialLines.Length} lines");

        if (trajLines.Length < 2 || trialLines.Length < 2)
        {
            EditorUtility.DisplayDialog("Error", "One or both CSV files are empty or have no data rows!", "OK");
            Debug.LogError("CSV files are empty or have no data!");
            return false;
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
            return false;
        }

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

            // Parse and store frame data
            FrameData frame = new FrameData
            {
                rowNumber = i,
                x = float.Parse(trajValues[xIdx].Trim())*100f-15f, // Convert to Unity units
                y = float.Parse(trajValues[yIdx].Trim())*100f-15f,
                headDirection = float.Parse(trajValues[headDirIdx].Trim())+90f,
                trialPhase = trialPhase,
                goal = trialValues[goalIdx].Trim()
            };

            framesToRender.Add(frame);
        }

        return true;
    }

    void UpdateRendering()
    {
        if (!isRendering || currentFrameIndex >= framesToRender.Count)
        {
            return;
        }

        FrameData frame = framesToRender[currentFrameIndex];

        // Wait for specified number of frames before capturing
        if (framesWaited < frameDelay)
        {
            if (framesWaited == 0)
            {
                // First frame of delay - set up the scene
                character.transform.position = new Vector3(frame.x, character.transform.position.y, frame.y);
                character.transform.rotation = Quaternion.Euler(0, frame.headDirection, 0);

                // Handle cue lights
                if (frame.trialPhase == "ITI")
                {
                    TurnOffAllCues();
                }
                else if (frame.trialPhase == "navigation")
                {
                    TurnOffAllCues();
                    ActivateCue(frame.goal);
                }

                SceneView.RepaintAll();
            }
            
            framesWaited++;
            return;
        }

        // Capture the screenshot
        string filename = $"{outputFolder}frame_{frame.rowNumber:0000}.png";
        ScreenCapture.CaptureScreenshot(filename);
        
        Debug.Log($"Captured frame for row {frame.rowNumber}: phase={frame.trialPhase}, goal={frame.goal}");
        capturedFrames++;

        // Show progress
        EditorUtility.DisplayProgressBar("Rendering Frames", 
            $"Frame {capturedFrames}/{framesToRender.Count} (row {frame.rowNumber})", 
            (float)capturedFrames / framesToRender.Count);

        // Move to next frame
        currentFrameIndex++;
        framesWaited = 0;

        // Check if we're done
        if (currentFrameIndex >= framesToRender.Count)
        {
            StopRendering();
            EditorUtility.DisplayDialog("Complete", 
                $"Rendered {capturedFrames} frames to {outputFolder}", "OK");
            Debug.Log($"Rendering complete! Total frames captured: {capturedFrames}");
        }

        Repaint();
    }

    void StopRendering()
    {
        isRendering = false;
        EditorApplication.update -= UpdateRendering;
        EditorUtility.ClearProgressBar();
        Repaint();
    }

    void OnDestroy()
    {
        StopRendering();
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