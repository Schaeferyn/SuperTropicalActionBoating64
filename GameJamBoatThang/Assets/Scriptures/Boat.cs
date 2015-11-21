using UnityEngine;
using System.Collections;

public class Boat : MonoBehaviour {

	public float baseSpeed = 1;
	public float maxSpeed = 0.2f;
	private Transform myBoat;

	// Use this for initialization
	void Start () {
		myBoat = this.transform.root;
	}
	
	// Update is called once per frame
	void Update () {
		if (myBoat.GetComponent<Rigidbody> ().velocity.z < maxSpeed) {
			myBoat.GetComponent<Rigidbody> ().AddForce (new Vector3 (0, 0, baseSpeed));
		}
	}
}
