using UnityEngine;
using System.Collections;

public class projectile : MonoBehaviour {

	public GameObject particle;

	void Start(){
		Physics.IgnoreLayerCollision (8, 6, true);
	}

	void OnCollisionEnter(Collision col){
        //if (col.gameObject.tag == "Station") {
        //	col.gameObject.GetComponent<StationCannon>().health -= 25;
        //	Debug.Log(col.gameObject.GetComponent<StationCannon>().health);
        //}

        if (col.gameObject.tag == "Boat")
        {
            //col.gameObject.GetComponent<StationCannon>().health -= 25;
            //Debug.Log(col.gameObject.GetComponent<StationCannon>().health);
            col.collider.attachedRigidbody.AddForce(GetComponent<Rigidbody>().velocity.normalized * 30.0f);
        }

        GetComponent<SpriteRenderer> ().enabled = false;
		GetComponent<SphereCollider> ().enabled = false;
		GetComponent<AudioSource> ().Play ();
		Destroy (this.transform.gameObject, 1.0f);
		GameObject clone = (GameObject)Instantiate (particle, transform.position, Quaternion.identity);
		Destroy (clone, 1.0f);
	}
}
