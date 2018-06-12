//Autor: Hugo C. Machado

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UICallbacks : MonoBehaviour
{
    public GameObject _MainPanel;
    public GameObject _ConfigurationPanel;

    public GameObject _selectionModeDropdown;
    public GameObject _playerColorDropdown;
    public GameObject _difficultySlider;
    public GameObject _soundToggle;
    public GameObject _arToggle;
    public GameObject _glassesToggle;
    public GameObject _showLegalMovesToggle;

    void Start()
    {
        mainPanel();

        _playerColorDropdown.GetComponent<Dropdown>().value = 
                    ConfigModel._player == SharpChess.Model.Player.PlayerColourNames.White ? 0 : 1;
        _selectionModeDropdown.GetComponent<Dropdown>().value = 
                    ConfigModel._selection == Selection.VirtualButton ? 0 : 1;
        _difficultySlider.GetComponent<Slider>().value = ConfigModel._difficulty;
        _soundToggle.GetComponent<Toggle>().isOn = ConfigModel._sound;

        arToggle(ConfigModel._ar);
        _arToggle.GetComponent<Toggle>().isOn = ConfigModel._ar;

        _showLegalMovesToggle.GetComponent<Toggle>().isOn = ConfigModel._showLegalMoves;
        _glassesToggle.GetComponent<Toggle>().isOn = ConfigModel._glassesOn;

    }

    public void startGame()
    {
        bool arEnabled = ConfigModel._ar;
        SceneManager.LoadScene(arEnabled ? "ARChessBoard" : "NoARChessBoard");
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
        _selectionModeDropdown.GetComponent<Dropdown>().interactable = status;
        _glassesToggle.GetComponent<Toggle>().interactable = status;
    }

    public void showLegalMovesToggle(bool status)
    {
        ConfigModel._showLegalMoves = status;
    }

    public void glassesOn(bool status)
    {
        ConfigModel._glassesOn = status;
    }

    public void configuration()
    {
        _MainPanel.SetActive(false);
        _ConfigurationPanel.SetActive(true);
    }

    public void help()
    {
        Application.OpenURL("https://drive.google.com/open?id=1D2v0dHgShwX-5FHs6vvvL-V4bZMkfWH8mDIMCVVLfyA");
    }

    public void mainPanel()
    {
        _MainPanel.SetActive(true);
        _ConfigurationPanel.SetActive(false);
    }
}
