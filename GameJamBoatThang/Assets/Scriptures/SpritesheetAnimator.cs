using UnityEngine;
using System.Collections;

public class SpritesheetAnimator : MonoBehaviour
{
    public Sprite[] sprites;
    SpriteRenderer myRenderer;
    int frameIndex = 0;

    public float frameInterval = 0.25f;
    float frameTimer;

	// Use this for initialization
	void Start ()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        frameTimer = frameInterval;
	}
	
	// Update is called once per frame
	void Update ()
    {
        frameTimer -= Time.deltaTime;
        if(frameTimer <= 0)
        {
            if (frameIndex < sprites.Length - 1)
                frameIndex++;
            else
                frameIndex = 0;

            myRenderer.sprite = sprites[frameIndex];
            frameTimer = frameInterval;
        }
	}
}
