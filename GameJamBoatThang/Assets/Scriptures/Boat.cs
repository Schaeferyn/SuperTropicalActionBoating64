using UnityEngine;
using System.Collections;
//using InControl;

public class Boat : MonoBehaviour {

	public Transform lastCheckpoint;

	public int playerIndex;
	private bool cannon = true;
	private bool mast = true;
	private bool wheel = true;
	public float baseSpeed = 2;
	private float maxSpeed = 2.5f;
	public Rigidbody cannonBall;
	public int lastCheck = 0;

	float rotation = 0;
	public float xIn;
	public float yIn;
	public bool aIn;
	bool keyPressed = false;

    //InputDevice myDevice;

	// Update is called once per frame
	void Update () {

  //      if (InputManager.Devices.Count - 1 >= playerIndex)
  //          myDevice = InputManager.Devices[playerIndex];
  //      else
  //          myDevice = null;

  //      if (myDevice == null) return;
	
  //      xIn = myDevice.LeftStick.X;
  //      yIn = myDevice.LeftStick.Y;
  //      aIn = myDevice.Action1.WasPressed;

		//if (cannon == true) {
		//	// Cannon Controls

		//	if(aIn){
		//		if(!keyPressed){
		//			keyPressed = true;
		//			fireCannon ();
		//		}
		//	} else {
		//		keyPressed = false;
		//	}
		//}
		if (wheel == true) {
			// Wheel Controls

			rotation += xIn;
			Quaternion temp = this.transform.localRotation;
			temp.eulerAngles = new Vector3 (90, rotation/1.5f, 0);
			this.transform.localRotation = temp;
		} 
		if (mast == true) {
			// Mast Controls

			if (this.GetComponent<Rigidbody> ().velocity.z < maxSpeed) {
				this.GetComponent<Rigidbody> ().AddRelativeForce (new Vector3 (0, yIn, 0));
			}
		}
	}

	public void fireCannon(Transform cannon){
		//Transform thisCannon = this.transform.FindChild("Cannon");
		Rigidbody clone = (Rigidbody)Instantiate(cannonBall, cannon.transform.position, Quaternion.Euler(90, 0, 0));
		clone.AddForce (cannon.right * 80f);
		Destroy(clone.transform.gameObject, 5f);
        GetComponent<Rigidbody>().AddForceAtPosition(cannon.right * -10, cannon.position);
	}

	void backToCheckpoint(){
		if (lastCheckpoint != null) {
			this.transform.root.position = lastCheckpoint.position;
		} else {
			this.transform.root.position = new Vector3(0f, 0.5f, 3f);
		}
		this.transform.GetComponent<Rigidbody> ().velocity = Vector3.zero;
	}
}
