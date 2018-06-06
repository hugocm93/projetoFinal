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
        back();
    }

    public void startGame()
    {
        bool arEnabled = true;
        SceneManager.LoadScene(arEnabled ? "ARChessBoard" : "NoARChessBoard");
    }

    public void configuration()
    {
        _MainPanel.SetActive(false);
        _ConfigurationPanel.SetActive(true);
    }

    public void help()
    {
        _MainPanel.SetActive(false);
        _HelpPanel.SetActive(true);
    }

    public void back()
    {
        _MainPanel.SetActive(true);
        _ConfigurationPanel.SetActive(false);
        _HelpPanel.SetActive(false);
    }

    public void playerColorChanged()
    {
    }

    public void selectionModeChanged()
    {
    }

    public void difficultyChanged(float difficulty)
    {
    }

    public void soundToggle(bool status)
    {
    }

    public void arToggle(bool status)
    {
    }

    public void showLegalMovesToggle(bool status)
    {
    }

}
