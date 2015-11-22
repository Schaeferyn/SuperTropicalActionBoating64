using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class checkpoint : MonoBehaviour {
	
	bool isPassed = false;
	bool winning = false;
	public bool finishLine = false;
	Color tempCol;
	public GameObject gameOverText;

	void Start(){
		tempCol = new Color(255, 255, 255, 0);
	}

	void Update(){
//		#if UNITY_EDITOR
//		if (Input.anyKeyDown) {
//			winning = true;
//			AudioSource victAudio = GameObject.Find ("VictoryAudio").GetComponent<AudioSource> ();
//			victAudio.Play ();
//		}
//		#endif

		if (winning == true) {
			AudioSource mainAudio = GameObject.Find ("BoatPartyAudio").GetComponent<AudioSource> ();
			if (mainAudio.volume > 0) {
				mainAudio.volume-=0.001f;
			}
			
			if(tempCol.a < 255){
				tempCol.a = tempCol.a += 0.02f;
				Debug.Log(tempCol);
				if(gameOverText){
					gameOverText.GetComponent<Text>().color = tempCol;
				}
			}
		}
	}

	void OnTriggerEnter(Collider col){

		if (col.tag == "Boat" && isPassed == false) {
			isPassed = true;
			GameObject TheBoat = col.transform.gameObject;
			Boat boat = TheBoat.GetComponent<Boat>();
			boat.lastCheckpoint = this.transform;
			boat.lastCheck++;

			if(finishLine == true && boat.lastCheck > 2){
				winning = true;
				AudioSource victAudio = GameObject.Find ("VictoryAudio").GetComponent<AudioSource> ();
				victAudio.Play ();
			}
		}
	}
}
