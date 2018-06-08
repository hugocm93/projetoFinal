using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class VirtualButtonsHandler : MonoBehaviour,  IVirtualButtonEventHandler{
    public GameObject _button;

	void Start()
    {
        if(BoardManager._instance._virtualButton != null)
            BoardManager._instance._virtualButton.GetComponent<VirtualButtonBehaviour>().RegisterEventHandler(this);
	}   

    public void OnButtonPressed(VirtualButtonBehaviour vb)
    {
        BoardManager._instance.selectVirtualButtonClicked();
    }

    public void OnButtonReleased(VirtualButtonBehaviour vb)
    {
    }
}
