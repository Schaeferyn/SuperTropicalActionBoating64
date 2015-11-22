using UnityEngine;
using System.Collections;

public class MenuControls : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (Input.anyKeyDown) {
			Launch ();
		}
	}

	void Launch(){
		Application.LoadLevel (1);
	}
}
