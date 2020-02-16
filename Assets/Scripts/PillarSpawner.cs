using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarSpawner : MonoBehaviour {
    public GameObject pillar;
    public int numPillarsToSpawn;
    public List<Vector2> usedXZPosns = new List<Vector2>();
    public bool initialized = false;

	// Use this for initialization
	public void Initialize(NavMapper navScript)
    {
        if(initialized)
        {
            return;
        }
        initialized = true;
		for (int i = 0; i < numPillarsToSpawn; i++)
        {
            bool isValidPosn = false;
            Vector2 currPos = Vector2.zero;
            while (!isValidPosn)
            {
                Vector3 currPosV3 = navScript.allValidNodes[Random.Range(0, navScript.allValidNodes.Count)].location;
                currPos = new Vector2(currPosV3.x, currPosV3.z);
                isValidPosn = true;
                foreach (Vector2 v in usedXZPosns)
                {
                    if (Vector2.SqrMagnitude(v - currPos) == 0)
                    {
                        isValidPosn = false;
                        break;
                    }
                }
            }
            usedXZPosns.Add(currPos);
            Instantiate(pillar,new Vector3(currPos.x, 0, currPos.y), Quaternion.identity);
        }
	}
}
