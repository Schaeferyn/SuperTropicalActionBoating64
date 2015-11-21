using UnityEngine;
using System.Collections;
using Rewired;

public class Boat : MonoBehaviour {

	public int playerIndex;
	Player player;

	private bool cannon = true;
	private bool mast = false;
	private bool wheel = false;

	public float baseSpeed = 2;
	private float maxSpeed = 2.5f;
	public Transform cannonBall;
	private Transform cannonInstance;

	float rotation = 0;
	float xIn;
	float yIn;
	bool aIn;
	bool keyPressed = false;

	void Start(){
		player = ReInput.players.GetPlayer(playerIndex);
	}
	
	// Update is called once per frame
	void Update () {


		xIn = player.GetAxis("MoveHorizontal");
		yIn = player.GetAxis("MoveVertical");
		aIn = player.GetButton ("ButtonA");

		if (cannon == true) {
			// Cannon Controls

			if(aIn){

				if(!keyPressed){
					keyPressed = true;
					fireCannon ();
				}
			} else {
				keyPressed = false;
			}

		} 
		else if (wheel == true) {
			// Wheel Controls

			rotation += xIn;
			
			Quaternion temp = this.transform.localRotation;
			
			temp.eulerAngles = new Vector3 (90, rotation/2, 0);
			
			this.transform.localRotation = temp;

		} 
		else if (mast == true) {
			// Mast Controls

			if (this.GetComponent<Rigidbody> ().velocity.z < maxSpeed) {
				this.GetComponent<Rigidbody> ().AddRelativeForce (new Vector3 (0, yIn, 0));
			}

		}
	}

	void fireCannon(){
		Transform thisCannon = this.transform.FindChild("Cannon");
		Debug.Log("cannoning");
		GameObject clone = (GameObject)Instantiate(cannonBall, thisCannon.transform.position, Quaternion.identity);
		clone.AddComponent<Rigidbody> ();
		clone.GetComponent<Rigidbody>().AddRelativeForce (new Vector3 (2, 0, 0));
	}

}
