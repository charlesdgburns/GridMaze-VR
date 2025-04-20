using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeController : MonoBehaviour
{
    public GameObject mazeParent;
    public Material cueOnMaterial;
    public Material cueOffMaterial;
    public Dictionary<string, GameObject> towers = new Dictionary<string, GameObject>();
    private bool dictionaryInitialized = false;


    public void InitializeTowerDictionary()
    {
        if (dictionaryInitialized)
        {
            Debug.Log("Tower dictionary already initialized");
            return;
        }
        
        Debug.Log("Manually initializing tower dictionary");
        towers.Clear(); // Clear any previous entries just in case
        
        foreach (Transform child in mazeParent.transform)
        {
            Debug.Log($"Checking child: {child.name}");
            if (child.name.Length == 2 && char.IsLetter(child.name[0]) && char.IsDigit(child.name[1]))
            {
                towers[child.name] = child.gameObject;
                Debug.Log($"Added tower: {child.name}");
            }
        }
        
        Debug.Log($"Dictionary populated with {towers.Count} towers");
        dictionaryInitialized = true;
    }
    public void ActivateCue(string towerName)
    {
        if (towers.TryGetValue(towerName, out GameObject tower))
        {
            Transform cue = tower.transform.Find("Cue");
            if (cue != null)
            {
                ToggleCue(cue, true);
            }
        }
        else
            Debug.LogError($"Tower '{towerName}' not found in dictionary. Available towers: {string.Join(", ", towers.Keys)}");
    }

    public void DeactivateCue(string towerName)
    {
        if (towers.TryGetValue(towerName, out GameObject tower))
        {
            Transform cue = tower.transform.Find("Cue");
            if (cue != null)
            {
                ToggleCue(cue, false);
            }
        }
        else 
            Debug.LogError($"Error when deactivting {towerName}");
    }

    private void ToggleCue(Transform cue, bool activate)
    {   // The cue itself:
        Renderer renderer = cue.GetComponent<Renderer>();
        
        Light cueLight = cue.Find("Light").GetComponent<Light>();
        
        if (renderer != null)
        {
            renderer.material = activate ? cueOnMaterial : cueOffMaterial;
        }
        
        if (cueLight != null)
        {
            cueLight.enabled = activate;
        }

        // but also the reflection:
        Transform reflection = cue.Find("reflection");
        
        Renderer reflectionRenderer = reflection.GetComponent<Renderer>();
        Light refectionLight = reflection.GetComponent<Light>();
        
        if (reflectionRenderer != null)
        {
            reflectionRenderer.material = activate ? cueOnMaterial : cueOffMaterial;
        }
        
        if (refectionLight != null)
        {
            refectionLight.enabled = activate;
        }


    }
}
