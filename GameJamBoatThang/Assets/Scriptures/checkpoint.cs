using UnityEngine;
using System.Collections;

public class checkpoint : MonoBehaviour {
	
	bool isPassed = false;
	public bool finishLine = false;

	void OnTriggerEnter(Collider col){

		if (col.tag == "Boat" && isPassed == false) {
			isPassed = true;
			GameObject TheBoat = col.transform.gameObject;
			Boat boat = TheBoat.GetComponent<Boat>();
			boat.lastCheckpoint = this.transform;
			boat.lastCheck++;

			if(finishLine == true && boat.lastCheck > 2){
				YouWin();
			}
		}
	}
	void YouWin(){
		Debug.Log ("Game Over!  You win!");
	}
}
