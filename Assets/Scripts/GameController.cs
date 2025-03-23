using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public MazeGenerator mazeGenerator;

    void Start()
{
    List<string> connections = new List<string> {"A1-A2",
    "A3-A4",
    "A4-A5",
    "A5-A6",
    "A6-A7",
    "A2-B2",
    "A3-B3",
    "A5-B5",
    "A7-B7",
    "B4-B5",
    "B6-B7",
    "B1-C1",
    "B2-C2",
    "B3-C3",
    "B6-C6",
    "C1-C2",
    "C2-C3",
    "C3-C4",
    "C4-C5",
    "C5-C6",
    "C6-C7",
    "C2-D2",
    "C5-D5",
    "C7-D7",
    "D1-D2",
    "D3-D4",
    "D4-D5",
    "D6-D7",
    "D1-E1",
    "D2-E2",
    "D3-E3",
    "D4-E4",
    "D5-E5",
    "D6-E6",
    "E2-F2",
    "E3-F3",
    "E5-F5",
    "E6-F6",
    "E7-F7",
    "F1-F2",
    "F2-F3",
    "F4-F5",
    "F6-F7",
    "F2-G2",
    "F5-G5",
    "F6-G6",
    "G1-G2",
    "G2-G3",
    "G3-G4",
    "G4-G5",
    "G5-G6",
    "G6-G7", };
    mazeGenerator.GenerateMaze(connections);
}

    // Update is called once per frame
    void Update()
    {
        
    }
}

