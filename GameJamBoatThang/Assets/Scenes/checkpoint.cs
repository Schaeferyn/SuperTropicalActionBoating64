using UnityEngine;
using System.Collections;

public class checkpoint : MonoBehaviour {

	bool isPassed = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void onTriggerEnter(){
		isPassed = true;
	}
}
