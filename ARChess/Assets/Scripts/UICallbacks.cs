using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UICallbacks : MonoBehaviour
{
    public GameObject _MainPanel;
    public GameObject _ConfigurationPanel;
    public GameObject _HelpPanel;

    void Start()
    {
        Back();
    }

    public void startGame()
    {
        bool arEnabled = true;
        SceneManager.LoadScene(arEnabled ? "ARChessBoard" : "NoARChessBoard");
    }

    public void Configuration()
    {
        _MainPanel.SetActive(false);
        _ConfigurationPanel.SetActive(true);
    }

    public void Help()
    {
        _MainPanel.SetActive(false);
        _HelpPanel.SetActive(true);
    }

    public void Back()
    {
        _MainPanel.SetActive(true);
        _ConfigurationPanel.SetActive(false);
        _HelpPanel.SetActive(false);
    }

}
