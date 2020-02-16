using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceCommandsManager : MonoBehaviour {
   public GameObject pauseMenu; 
   public void pauseMenuCalled()
   {
        Debug.Log("Pause Menu called");
        RaycastHit hit;
        LayerMask spatialLayer = 1 << 31;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 2.0f, spatialLayer))
        {
            instantiatePauseMenu(hit.transform.position);
            stopGameOperations();
        }
        else
        {
            Vector3 menuPosition = Camera.main.transform.position + Camera.main.transform.forward * 2.0f;
            instantiatePauseMenu(menuPosition);
            stopGameOperations();
        }
   }
    public void stopGameOperations()
    {
        //Need to stop bot movement and activity here  -> work with shiven on this part
    }
    public void instantiatePauseMenu(Vector3 position)
    {
        GameObject pauseMenuInstantiated = Instantiate(pauseMenu, position, Quaternion.identity);
    }
}
