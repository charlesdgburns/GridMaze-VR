using System.Collections.Generic;
using UnityEngine;

public class SmallMazeGameController : MonoBehaviour
{
    public MazeGenerator mazeGenerator;
    public MazeController mazeController;
    void Start()
{
    List<string> connections = new List<string> {"A1-A2",
                                                "A2-A3",
                                                "A1-B1",
                                                "A2-B2",
                                                "A3-B3",
                                                "B1-B2", 
                                                "B2-B3",
                                                "B1-C1",
                                                "B2-C2",
                                                "B3-C3",
                                                "C1-C2",
                                                "C2-C3" };
    mazeGenerator.GenerateMaze(connections);
    mazeController.InitializeTowerDictionary();
}

    // Update is called once per frame
    void Update()
    {
        
    }
}

