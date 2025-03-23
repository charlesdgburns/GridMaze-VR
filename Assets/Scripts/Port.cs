using UnityEngine;

public class Port: MonoBehaviour, IsInteractable {
    public void Interact() {
        Debug.Log(Random.Range(0,100));  // for now just outputs random number. TODO: should interact with environment states.
    }
}
