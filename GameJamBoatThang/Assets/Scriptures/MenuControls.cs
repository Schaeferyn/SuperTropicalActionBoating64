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
			if(i < 2){
				controllers[i].color = Color.blue;
			} else {
				controllers[i].color = Color.red;
			}
		}

		if (InputManager.Devices.Count < 4) {
			Text prompt = GameObject.Find("Instructions").GetComponent<Text>();
			prompt.text = "Waiting for "+(4-InputManager.Devices.Count)+" more players...";
		}

		if (aIn) {
			Launch ();
		}

	}
	
	void Launch(){
		Application.LoadLevel (1);
	}
}
