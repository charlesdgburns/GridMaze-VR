using UnityEngine;

public class Reflections : MonoBehaviour
{
    public LayerMask reflectionLayer; // Set this to a dedicated "Reflection" layer
    public Transform mazeParent; // The parent object containing all towers & bridges
    
    void Start()
    {
        GenerateMirroredWorld();
    }

    void GenerateMirroredWorld()
    {
        if (!mazeParent) return;
        int reflectionLayerInt = (int)Mathf.Log(reflectionLayer.value, 2);
        GameObject mirroredParent = new GameObject("MirroredWorld");
        mirroredParent.layer = reflectionLayerInt;

        foreach (Transform child in mazeParent)
        {
            foreach (Transform grandchild in child)
            {
                // Check if the object's name contains "cue" (case-insensitive)
                if (!grandchild.name.ToLower().Contains("cue")) continue;

                // Create a mirrored version of "cue"
                GameObject mirroredObject = Instantiate(grandchild.gameObject, grandchild.position, grandchild.rotation);
                mirroredObject.name = "reflection";
                mirroredObject.transform.parent = grandchild.transform;

                // Flip along Y-axis
                Vector3 mirroredScale = mirroredObject.transform.localScale;
                mirroredScale.y = -mirroredScale.y;
                mirroredObject.transform.localScale = mirroredScale;

                // Flip the y-coordinate
                Vector3 mirroredPosition = mirroredObject.transform.position;
                mirroredPosition.y = -mirroredPosition.y;
                mirroredObject.transform.position = mirroredPosition;
                
                //For the imported CAD a rotation is also required:
                mirroredObject.transform.Rotate(180f,0f,0f);
                
                // Set mirrored object's layer
                mirroredObject.layer = reflectionLayerInt;
            }
        }
    }

}
