using UnityEngine;
using System.Collections;

public class projectile : MonoBehaviour {
	void OnCollisionEnter(){
		Destroy (this.transform.gameObject);
	}
}
