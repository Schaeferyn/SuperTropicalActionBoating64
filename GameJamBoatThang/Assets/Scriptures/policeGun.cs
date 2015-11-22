using UnityEngine;
using System.Collections;

public class policeGun : MonoBehaviour {

	GameObject[] boats;

	// Use this for initialization
	void Start () {
		boats = GameObject.FindGameObjectsWithTag ("Boat");
	}
	
	// Update is called once per frame
	void Update () {
		for (int i = 0; i < boats.Length; i++) {
			float dist = Vector3.Distance(transform.position, boats[i].transform.position);
			if(dist < 8){

			}
		}
	}
}
