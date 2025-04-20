using UnityEngine;

public class Port : MonoBehaviour, IsInteractable 
{
    private string towerName;
    private GameStateManager gameStateManager;
    
    void Start()
    {
        // Get the tower name from the parent object
        towerName = transform.parent.name;
        
        // Find the GameStateManager in the scene
        gameStateManager = GameStateManager.Instance;
        if (gameStateManager == null)
        {
            Debug.LogError("GameStateManager not found in the scene!");
        }
        
        //Debug.Log($"Port initialized for tower: {towerName}"); //This seems to work
    }
    
    public void Interact() 
    {
        Debug.Log($"Port on tower {towerName} was interacted with");
        
        // Register interaction directly with the GameStateManager
        if (gameStateManager != null)
        {
            gameStateManager.RegisterPoke(towerName);
        }
        else
        {
            Debug.LogError("Cannot register interaction: GameStateManager is null");
        }
    }
}