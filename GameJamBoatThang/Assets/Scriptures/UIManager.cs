using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    void Awake() { instance = this; }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void startGame(){
		Application.LoadLevel (1);
	}

	public void Quit(){
		Application.Quit();
	}
}
