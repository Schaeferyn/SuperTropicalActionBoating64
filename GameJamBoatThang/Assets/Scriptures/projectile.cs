using UnityEngine;
using System.Collections;

public class projectile : MonoBehaviour {

	public GameObject particle;

	void OnCollisionEnter(Collision col){
		if (col.gameObject.tag == "Station") {
			col.gameObject.GetComponent<StationCannon>().health -= 25;
			Debug.Log(col.gameObject.GetComponent<StationCannon>().health);
		}

		GetComponent<SpriteRenderer> ().enabled = false;
		GetComponent<SphereCollider> ().enabled = false;
		GetComponent<AudioSource> ().Play ();
		Destroy (this.transform.gameObject, 1.0f);
		GameObject clone = (GameObject)Instantiate (particle, transform.position, Quaternion.identity);
		Destroy (clone, 1.0f);
	}
}
