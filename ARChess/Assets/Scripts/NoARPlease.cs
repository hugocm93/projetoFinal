using System.Collections;
using System.Collections.Generic;
using Vuforia;

using UnityEngine;

public class NoARPlease : MonoBehaviour
{
	void Start()
    {
        Camera mainCamera = Camera.main;
        if(mainCamera)
        {
            if(mainCamera.GetComponent<VuforiaBehaviour>())
                mainCamera.GetComponent<VuforiaBehaviour>().enabled = false;

            if(mainCamera.GetComponent<VideoBackgroundBehaviour>())
                mainCamera.GetComponent<VideoBackgroundBehaviour>().enabled = false;
            
            if(mainCamera.GetComponent<DefaultInitializationErrorHandler>())
                mainCamera.GetComponent<DefaultInitializationErrorHandler>().enabled = false;
        }  
	}
}
