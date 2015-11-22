using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class checkpoint : MonoBehaviour {
	
	bool isPassed = false;
	public bool finishLine = false;

	#if UNITY_EDITOR

	void Update(){
		if (Input.anyKeyDown) {
			YouWin();
		}
	}

	#endif

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
		AudioSource mainAudio = GameObject.Find ("BoatPartyAudio").GetComponent<AudioSource> ();
		AudioSource victAudio = GameObject.Find ("VictoryAudio").GetComponent<AudioSource> ();
		if (mainAudio.volume > 0) {
			mainAudio.volume-=0.1f;
		}

		victAudio.Play ();

		if (mainAudio.volume < 0.5f && victAudio.volume < 1f) {
			victAudio.volume+=0.1f;
		}


	}
}
