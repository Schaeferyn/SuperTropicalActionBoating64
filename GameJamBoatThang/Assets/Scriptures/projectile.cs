using UnityEngine;
using System.Collections;

public class projectile : MonoBehaviour {

	public GameObject particle;

	void OnCollisionEnter(Collision col){
		GetComponent<SpriteRenderer> ().enabled = false;
		GetComponent<SphereCollider> ().enabled = false;
		GetComponent<AudioSource> ().Play ();
		Destroy (this.transform.gameObject, 1.0f);
		GameObject clone = (GameObject)Instantiate (particle, transform.position, Quaternion.identity);
		Destroy (clone, 1.0f);
	}
}
