using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class VirtualButtonsHandler : MonoBehaviour,  IVirtualButtonEventHandler{
    public GameObject _button;

	void Start()
    {
        _button = GameObject.Find("VirtualButton");
        _button.GetComponent<VirtualButtonBehaviour>().RegisterEventHandler(this);
	}   

    public void OnButtonPressed(VirtualButtonBehaviour vb)
    {
        print("Pressed" + vb.name);
        BoardManager._instance.selectButtonCicked();
    }

    public void OnButtonReleased(VirtualButtonBehaviour vb)
    {
    }
}
