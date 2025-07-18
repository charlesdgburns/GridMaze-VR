using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// Define the State enum before the GameStateManager class
public enum GameState
{
    ITI,
    Cue,
    PokedIn,
    Reward
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    public MazeController maze;
    
    [Header("Session Settings")]
    public float sessionDuration = 1800f; // 30 minutes in seconds
    public string endSceneName = "EndScene"; // Scene to load when session ends
    
    private string logFilePath;
    private GameState currentState;
    private List<string> goalSet = new List<string> { "A1", "A2", "A3", "A4", "A5", "A6", "A7",
                                                    "B1", "B2", "B3", "B4", "B5", "B6", "B7",
                                                    "C1","C2","C3","C4","C5","C6","C7",
                                                    "D1","D2","D3","D4","D5","D6","D7",
                                                    "E1","E2","E3","E4","E5","E6","E7",
                                                    "F1","F2","F3","F4","F5","F6","F7",
                                                    "G1","G2","G3","G4","G5","G6","G7" };
    private string currentGoal;
    private int nTrials = 0;
    private float trialStartTime;
    private float trialEndTime;
    private List<string> errorPokeList = new List<string>();
    
    // Session tracking
    private float sessionStartTime;
    private bool sessionActive = false;
    
    // Performance metrics
    private int correctPokes = 0;
    private int incorrectPokes = 0;
    private float totalReactionTime = 0;
    
    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        InitializeLogFile();
    }
    
    private void Start()
    {
        // Start the game session
        StartSession();
    }
    
    private void Update()
    {
        // Check if session should end
        if (sessionActive && Time.time - sessionStartTime >= sessionDuration)
        {
            EndSession();
        }
    }
    
    private void StartSession()
    {
        sessionStartTime = Time.time;
        sessionActive = true;
        LogEvent("Session started");
        
        // Start with ITI state
        SetState(GameState.ITI);
    }
    
    private void EndSession()
    {
        sessionActive = false;
        LogEvent("Session ended after " + ((Time.time - sessionStartTime) / 60f).ToString("F2") + " minutes");
        LogEvent("Final performance: " + GetPerformanceStats());
        
        // Stop any active coroutines
        StopAllCoroutines();
        
        // Deactivate current cue if any
        if (!string.IsNullOrEmpty(currentGoal) && maze != null)
        {
            maze.DeactivateCue(currentGoal);
        }
        
        // Load end scene or show end UI
        if (!string.IsNullOrEmpty(endSceneName))
        {
            SceneManager.LoadScene(endSceneName);
        }
        else
        {
            Debug.Log("Session completed! (No end scene specified)");
            // Alternatively, show UI panel with session results
        }
    }
    
    private void InitializeLogFile()
    {
        string sessionName = "BigMaze_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
        logFilePath = Path.Combine(Application.persistentDataPath, sessionName);
        File.WriteAllText(logFilePath, "Session Log\n");
    }
    
    public void SetState(GameState newState)
    {
        // Only process state changes if session is active
        if (!sessionActive && newState != GameState.ITI)
        {
            return;
        }
        
        currentState = newState;
        LogStateChange(newState);
        
        switch (newState)
        {
            case GameState.ITI:
                StartCoroutine(ITIState());
                break;
            case GameState.Cue:
                StartCoroutine(CueState());
                break;
            case GameState.PokedIn:
                StartCoroutine(PokedInState());
                break;
            case GameState.Reward:
                StartCoroutine(RewardState());
                break;
        }
    }
    
    private IEnumerator ITIState()
    {
        errorPokeList.Clear();
        yield return new WaitForSeconds(UnityEngine.Random.Range(4f, 8f));
        
        // Only proceed if session is still active
        if (sessionActive)
        {
            SetState(GameState.Cue);
        }
    }
    
    private IEnumerator CueState()
    {
        nTrials++;
        trialStartTime = Time.time;
        currentGoal = goalSet[UnityEngine.Random.Range(0, goalSet.Count)];
        LogEvent($"Trial {nTrials}: Goal {currentGoal} cued.");
        
        if (maze != null)
        {
            maze.ActivateCue(currentGoal);
        }
        else
        {
            Debug.LogError("MazeController reference is null!");
        }
        
        yield return null; // Wait for player interaction
    }
    
    public void RegisterPoke(string towerName)
    {
        if (!sessionActive)
            return;
            
        if (currentState == GameState.Cue)
        {
            if (towerName == currentGoal)
            {
                correctPokes++;
                float reactionTime = Time.time - trialStartTime;
                totalReactionTime += reactionTime;
                LogEvent($"Correct poke at {towerName}. Reaction time: {reactionTime:F2} seconds");
                SetState(GameState.PokedIn);
            }
            else if (!errorPokeList.Contains(towerName))
            {
                incorrectPokes++;
                LogEvent($"Error poke at {towerName}");
                errorPokeList.Add(towerName);
            }
        }
    }
    
    private IEnumerator PokedInState()
    {
        trialEndTime = Time.time;
        yield return new WaitForSeconds(0.2f);
        SetState(GameState.Reward);
    }
    
    private IEnumerator RewardState()
    {
        LogEvent("Reward given at " + currentGoal);
        
        if (maze != null)
        {
            maze.DeactivateCue(currentGoal);
        }
        
        yield return new WaitForSeconds(2f);
        
        // Only proceed if session is still active
        if (sessionActive)
        {
            SetState(GameState.ITI);
        }
    }
    
    private void LogStateChange(GameState state)
    {
        string logEntry = $"{DateTime.Now:HH:mm:ss} - State changed to {state}";
        File.AppendAllText(logFilePath, logEntry + "\n");
        Debug.Log(logEntry);
    }
    
    private void LogEvent(string message)
    {
        string logEntry = $"{DateTime.Now:HH:mm:ss} - {message}";
        File.AppendAllText(logFilePath, logEntry + "\n");
        Debug.Log(logEntry);
    }
    
    public string GetPerformanceStats()
    {
        float accuracy = correctPokes > 0 ? 
            (float)correctPokes / (correctPokes + incorrectPokes) * 100f : 0f;
        float avgReactionTime = correctPokes > 0 ? 
            totalReactionTime / correctPokes : 0f;
        
        return $"Trials completed: {correctPokes}\n" +
               $"Errors made: {incorrectPokes}\n" +
               $"Accuracy: {accuracy:F1}%\n" +
               $"Average reaction time: {avgReactionTime:F2} seconds";
    }
    
    // Public method to force end the session (can be called from UI)
    public void ForceEndSession()
    {
        EndSession();
    }
}