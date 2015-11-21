using UnityEngine;
using System.Collections;
using Rewired;

public class Boat : MonoBehaviour {

	public int playerIndex;
	Player player;
	
	public float baseSpeed = 10;
	private float maxSpeed = 2.5f;

	float rotation = 0;
	float xIn;
	float yIn;

	void Start(){
		player = ReInput.players.GetPlayer(playerIndex);
	}
	
	// Update is called once per frame
	void Update () {

		xIn = player.GetAxis("MoveHorizontal");
		yIn = player.GetAxis("MoveVertical");
		
		if (this.GetComponent<Rigidbody> ().velocity.z < maxSpeed) {
			this.GetComponent<Rigidbody> ().AddRelativeForce (new Vector3 (0, yIn, 0));
		}

		rotation += xIn;

		Quaternion temp = this.transform.localRotation;

		temp.eulerAngles = new Vector3 (90, rotation, 0);

		this.transform.localRotation = temp;
	}

}
