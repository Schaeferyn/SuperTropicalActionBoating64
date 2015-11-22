using UnityEngine;
using System.Collections;

public class floating : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void Move(){
		Vector3 currPos = this.transform.position;
		Vector3 newPos = this.transform.position + Vector3.up;

		transform.position = Vector3.Lerp (currPos, newPos, 2f);
	}
}
