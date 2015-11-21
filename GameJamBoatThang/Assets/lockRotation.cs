using UnityEngine;
using System.Collections;

public class lockRotation : MonoBehaviour {

	// Update is called once per frame
	void LateUpdate () {
		GetComponent<Camera> ().transform.rotation = Quaternion.Euler (new Vector3 (90, 0, 0));
	}
}
