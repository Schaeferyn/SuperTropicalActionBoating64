using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class checkpoint : MonoBehaviour {
	
	bool isPassed = false;
	bool winning = false;
    public bool finishLine = false;
	Color tempCol;
	public GameObject gameOverText;
    Text victoryText;
    Outline victoryOutline;
    Shadow victoryShadow;

    Color victoryColor;
    float lerpPercent = 0;
    bool isLerping = false;

	void Start(){
		tempCol = new Color(255, 255, 255, 0);

        if(gameOverText != null)
        {
            victoryText = gameOverText.GetComponent<Text>();
            //victoryText.enabled = false;
            victoryOutline = victoryText.GetComponent<Outline>();
            victoryShadow = victoryText.GetComponent<Shadow>();
        }
	}

	void Update(){
//#if UNITY_EDITOR
//        if (Input.anyKeyDown)
//        {
//            winning = true;
//            AudioSource victAudio = GameObject.Find("VictoryAudio").GetComponent<AudioSource>();
//            victAudio.Play();
//        }
//#endif

        if (winning == true) {
			AudioSource mainAudio = GameObject.Find ("BoatPartyAudio").GetComponent<AudioSource> ();
			if (mainAudio.volume > 0) {
				mainAudio.volume-=0.001f;
			}

            if (tempCol.a < 1.0f){
				tempCol.a = tempCol.a += 0.52f;
				//Debug.Log(tempCol);
				if(gameOverText){
					victoryText.color = tempCol;
				}
			}
            else
            {
                if(!isLerping)
                {
                    isLerping = true;
                    iTween.ValueTo(this.gameObject, iTween.Hash("from", 0.0f, "to", 1.0f, "easetype", iTween.EaseType.easeInOutQuad, "looptype", iTween.LoopType.pingPong, "time", 1.0f, 
                        "onupdate", "SetVictoryColors", "onupdatetarget", gameObject));
                }
            }
		}
	}

	void OnTriggerEnter(Collider col){

        //Debug.Log("asdf");

		if (col.tag == "Boat" /*&& isPassed == false*/) {

            //Debug.Log("asdf2 " + name);
            isPassed = true;
			GameObject TheBoat = col.transform.gameObject;
			Boat boat = TheBoat.GetComponent<Boat>();
			boat.lastCheckpoint = this.transform;
			boat.lastCheck++;

			if(finishLine && boat.lastCheck > 2 )
            {

                //Debug.Log("asdf3");
                winning = true;
				AudioSource victAudio = GameObject.Find ("VictoryAudio").GetComponent<AudioSource> ();
				victAudio.Play ();
                if (victoryText != null)
                {
                    //victoryText.enabled = true;
                    if (boat.teamIndex == 0)
                    {
                        victoryText.text = "Blue Team Wins!";
                        victoryColor = Color.blue;
                    }
                    else
                    {
                        victoryText.text = "Red Team Wins!";
                        victoryColor = Color.red;
                    }
                }
            }
		}
	}

    void SetVictoryColors(float percent)
    {
        lerpPercent = percent;

        victoryOutline.effectColor = Color.Lerp(Color.black, victoryColor, lerpPercent);
        victoryShadow.effectColor = Color.Lerp(Color.black, victoryColor, lerpPercent);
    }
}
