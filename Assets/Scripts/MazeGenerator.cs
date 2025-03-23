using System.Collections.Generic;
using UnityEngine;


public class MazeGenerator : MonoBehaviour
{
    public GameObject mazeParent;
    public GameObject towerPrefab;
    public GameObject bridgePrefab;
    public float towerSpacing = 18f; // Distance between tower centers
    public float towerWidth = 11f;
    public float bridgeWidth = 7f;
    private Dictionary<string, GameObject> towers = new Dictionary<string, GameObject>();
    public void GenerateMaze(List<string> connections)
    {
        towers.Clear();
        foreach (string connection in connections)
        {
            string[] parts = connection.Split('-');
            if (parts.Length == 2)
            {
                Vector3 pos1 = GetTowerPosition(parts[0]);
                Vector3 pos2 = GetTowerPosition(parts[1]);
                
                if (!towers.ContainsKey(parts[0]))
                    towers[parts[0]] = Instantiate(towerPrefab, pos1, Quaternion.identity);
                    towers[parts[0]].transform.parent = mazeParent.transform;
                    towers[parts[0]].name = parts[0];
                if (!towers.ContainsKey(parts[1]))
                    towers[parts[1]] = Instantiate(towerPrefab, pos2, Quaternion.identity);
                    towers[parts[1]].transform.parent = mazeParent.transform;
                    towers[parts[1]].name = parts[1];
                PlaceBridge(pos1, pos2);

            }
        }
    }

    private Vector3 GetTowerPosition(string label)
    {
        char column = label[0]; // A-G
        int row = int.Parse(label.Substring(1)); // 1-7

        float x = (column - 'A') * towerSpacing;
        float z = (row - 1) * towerSpacing;
        return new Vector3(x, 0.25f, z); // Assuming y = 0 for now
    }

    private void PlaceBridge(Vector3 pos1, Vector3 pos2)
    {   
        Vector3 midpoint = (pos1 + pos2) / 2;
        Quaternion rotation = Quaternion.identity;
        
        if (Mathf.Approximately(pos1.x, pos2.x)) // Same column, vertical bridge
            rotation = Quaternion.Euler(0, 90, 0); // Align along Z-axis
        else if (Mathf.Approximately(pos1.z, pos2.z)) // Same row, horizontal bridge
            rotation = Quaternion.Euler(0, 0, 0); // Align along X-axis

        var bridge = Instantiate(bridgePrefab, midpoint, rotation);
        bridge.transform.parent = mazeParent.transform;
    }
}
