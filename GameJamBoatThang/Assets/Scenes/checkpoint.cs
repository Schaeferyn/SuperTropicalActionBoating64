using UnityEngine;
using System.Collections;

public class checkpoint : MonoBehaviour {


	int lastCheck = 0;
	bool isPassed = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider col){

		if (col.gameObject.name == "Boat" && isPassed == false) {
			isPassed = true;
			lastCheck++;
		}
	}
}
