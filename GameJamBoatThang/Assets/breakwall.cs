using UnityEngine;
using System.Collections;

public class breakwall : MonoBehaviour {

	int health = 2;

	void Update(){
		if (health <= 0) {
			Destroy(this.gameObject);
		}
	}

	void OnCollisionEnter(Collision col){
		if(col.gameObject.tag == "projectile"){
			health--;
		}
	}
}
