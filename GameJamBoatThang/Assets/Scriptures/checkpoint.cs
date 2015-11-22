using UnityEngine;
using System.Collections;

public class checkpoint : MonoBehaviour {
	
	bool isPassed = false;
	public bool finishLine = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider col){

		if (col.tag == "Boat" && isPassed == false) {
			isPassed = true;
			GameObject TheBoat = col.transform.gameObject;
			Boat boat = TheBoat.GetComponent<Boat>();
			boat.lastCheckpoint = this.transform;
			boat.lastCheck++;

			Debug.Log(boat.lastCheck);
			if(finishLine == true && boat.lastCheck > 2){
				YouWin();
			}
		}
	}
	void YouWin(){
		Debug.Log ("Game Over!  You win!");
	}
}
