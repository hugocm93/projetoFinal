using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneManager : MonoBehaviour {

    public void changeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
