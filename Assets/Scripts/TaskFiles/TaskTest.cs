using UnityEngine;

public class TaskTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

// So one way of doing this is having a state file and updating values there. 
// This could be similar to PyControl's .txt output- current state would be last state line.
// Then Poke would simply print a line on the GameState,txt file which could trigger events to update the state.