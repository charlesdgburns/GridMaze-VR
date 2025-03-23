using UnityEngine;

public class NumberGenerator : MonoBehaviour, IsInteractable
{
    public void Interact(){
        Debug.Log(Random.Range(0,100));
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
