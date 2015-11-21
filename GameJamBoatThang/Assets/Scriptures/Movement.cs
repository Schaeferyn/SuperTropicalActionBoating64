using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour {
	
	public Vector2 speed = new Vector2(1,0);
	
	void Start(){
	}
	
	void Update () {
		
		//GetComponentInParent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX;
		
		float inputX = Input.GetAxis ("Horizontal");
		float inputY = Input.GetAxis ("Vertical");
		
		Vector3 movement = new Vector3 (speed.x * inputX, speed.y * inputY, 0);
		
		movement *= Time.deltaTime;
		transform.Translate (movement);
	}
}