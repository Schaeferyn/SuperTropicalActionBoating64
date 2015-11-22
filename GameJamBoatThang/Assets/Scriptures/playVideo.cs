using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class playVideo : MonoBehaviour {

	//the GUI texture  
	private GUITexture videoGUItex;  
	//the Movie texture  
	private MovieTexture mTex;  
	//the AudioSource  
	//private AudioSource movieAS;  
	//the movie name inside the resources folder  
	public string movieName;  
	
	void Awake()  
	{  
		//get the attached GUITexture  
		videoGUItex = this.GetComponent<GUITexture>();  
		//get the attached AudioSource  
		//movieAS = this.GetComponent<AudioSource>();  
		//load the movie texture from the resources folder  
		mTex = (MovieTexture)Resources.Load(movieName);  
		//set the AudioSource clip to be the same as the movie texture audio clip  
		//movieAS.clip = mTex.audioClip;  
		//anamorphic fullscreen  
		videoGUItex.pixelInset = new Rect(Screen.width/2, -Screen.height/2,0,0);  
	}  
	
	//On Script Start  
	void Start()  
	{  
		//set the videoGUItex.texture to be the same as mTex  
		videoGUItex.texture = mTex;  
		//Plays the movie  
		Invoke ("playMovie", 0.5f);
		//plays the audio from the movie  
		//movieAS.Play();  
	}  

	void playMovie(){
		mTex.Play();  
		mTex.loop = true;
	}
}
