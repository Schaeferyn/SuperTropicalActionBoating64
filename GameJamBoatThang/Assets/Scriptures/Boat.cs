using UnityEngine;
using System.Collections;
using Rewired;

public class Boat : MonoBehaviour {

	Player player;

	public float baseSpeed = 10;
	private float maxSpeed = 20;

	float xIn;
	float yIn;

	// Update is called once per frame
	void Update () {

		xIn = player.GetAxis("MoveHorizontal");
		yIn = player.GetAxis("MoveVertical");

		if (this.GetComponent<Rigidbody> ().velocity.y < maxSpeed) {
			this.GetComponent<Rigidbody> ().AddRelativeForce (new Vector3 (0, baseSpeed, 0));
		} else {

		}
	}

	void MoveShip(float: dir;){

	}
}
