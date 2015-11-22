using UnityEngine;
using System.Collections;
using InControl;
using UnityEngine.UI;

public class MenuControls : MonoBehaviour {

	public int playerIndex;
	InputDevice myDevice;
	bool aIn;
	public Image[] controllers;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

		if (InputManager.Devices.Count - 1 >= playerIndex)
			myDevice = InputManager.Devices[playerIndex];
		else
			myDevice = null;
		
		if (myDevice == null) return;

		aIn = myDevice.Action1.WasPressed;

		for (int i = 0; i < InputManager.Devices.Count; i++) {
			Debug.Log(controllers[i]);
		}

		if (aIn) {
			Launch ();
		}
	}

	void Launch(){
		Application.LoadLevel (1);
	}
}
