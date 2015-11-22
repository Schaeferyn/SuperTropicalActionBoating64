﻿using UnityEngine;
using System.Collections;

public class breakwall : MonoBehaviour {

	int health = 2;

	void Start(){

	}

	void Update(){
		if (health <= 0) {
			Destroy(this.gameObject, 0.2f);
		}
	}

	void OnCollisionEnter(Collision col){
		if(col.transform.root.tag == "Projectile"){
			health--;
		}
	}
}
