using UnityEngine;
using System.Collections;

public class Boat : MonoBehaviour {

	public float baseSpeed = 10;
	public float maxSpeed = 20;
	private GameObject myBoat;

	// Use this for initialization
	void Start () {
		myBoat = this.transform.root;
	}
	
	// Update is called once per frame
	void Update () {
		myBoat.GetComponent<Rigidbody>().AddForce(new Vector3(0, 0, baseSpeed/10));
	}
}
