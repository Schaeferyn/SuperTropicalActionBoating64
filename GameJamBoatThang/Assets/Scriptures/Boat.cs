using UnityEngine;
using System.Collections;
using Rewired;

public class Boat : MonoBehaviour {

	public int playerIndex;
	Player player;

	private bool cannon = true;
	private bool mast = false;
	private bool wheel = true;

	public float baseSpeed = 2;
	private float maxSpeed = 2.5f;
	public Rigidbody cannonBall;

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
		if (wheel == true) {
			// Wheel Controls

			rotation += xIn;
			
			Quaternion temp = this.transform.localRotation;
			
			temp.eulerAngles = new Vector3 (90, rotation/2, 0);
			
			this.transform.localRotation = temp;

		} 
		if (mast == true) {
			// Mast Controls

			if (this.GetComponent<Rigidbody> ().velocity.z < maxSpeed) {
				this.GetComponent<Rigidbody> ().AddRelativeForce (new Vector3 (0, yIn, 0));
			}

		}
	}

	void fireCannon(){
		Transform thisCannon = this.transform.FindChild("Cannon");
		Rigidbody clone = (Rigidbody)Instantiate(cannonBall, thisCannon.transform.position, Quaternion.Euler(90, 0, 0));
		clone.AddForce (this.transform.root.right * 80f);
		Destroy(clone.transform.gameObject, 3f);
	}

}
