//Origem: https://forum.unity.com/threads/use-ar-camera-vuforia-core-in-individual-scene-not-entire-project.498489/#post-3242022
//Autor: theolagendijk

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