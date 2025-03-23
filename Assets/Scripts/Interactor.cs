using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IsInteractable{
    public void Interact();
}
public class Interactor : MonoBehaviour
{
    public Transform InteractorSource;
    public float InteractRange;

    void Start() {

    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.F)) {
            Ray r = new Ray(InteractorSource.position, InteractorSource.forward);
            if (Physics.Raycast(r, out RaycastHit hitInfo, InteractRange)){
                if (hitInfo.collider.gameObject.TryGetComponent(out IsInteractable interactObj)) {
                    interactObj.Interact();
                }
            } 
        }
    }
}
