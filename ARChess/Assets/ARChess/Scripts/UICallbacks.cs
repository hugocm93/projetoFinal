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
        bool arEnabled = ConfigModel._ar;
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

    public void playerColorChanged(int player)
    {
        ConfigModel._player = player == 0 ? SharpChess.Model.Player.PlayerColourNames.White : 
                                            SharpChess.Model.Player.PlayerColourNames.Black;
    }

    public void selectionModeChanged(int mode)
    {
        ConfigModel._selection = mode == 0 ? Selection.VirtualButton : 
                                             Selection.Time;
    }

    public void difficultyChanged(float difficulty)
    {
        ConfigModel._difficulty = (int)difficulty;
    }

    public void soundToggle(bool status)
    {
        ConfigModel._sound = status;
    }

    public void arToggle(bool status)
    {
        ConfigModel._ar = status;
    }

    public void showLegalMovesToggle(bool status)
    {
        ConfigModel._showLegalMoves = status;
    }
}
