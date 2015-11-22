using UnityEngine;
using System.Collections;

public enum EndState
{
    Loop,
    Stop
}

public class SpritesheetAnimator : MonoBehaviour
{
    public Sprite[] sprites;
    SpriteRenderer myRenderer;
    int frameIndex = 0;

    public float frameInterval = 0.25f;
    float frameTimer;

    public bool startEnabled = true;
    public EndState endState;
    bool isPlaying = false;

    public bool shouldPlayIdleSailAnimation = false;

	// Use this for initialization
	void Start ()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        frameTimer = frameInterval;

        if (startEnabled)
            PlayAnim();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!isPlaying) return;

        frameTimer -= Time.deltaTime;
        if(frameTimer <= 0)
        {
            if (frameIndex < sprites.Length - 1)
                frameIndex++;
            else
            {
                if (endState == EndState.Loop)
                    frameIndex = 0;
                else
                    AnimEnded();
            }

            myRenderer.sprite = sprites[frameIndex];
            frameTimer = frameInterval;
        }
	}

    public void AnimEnded()
    {
        isPlaying = false;

        Debug.Log("aksdjfkasdfh");
        if(shouldPlayIdleSailAnimation)
        {
            transform.parent.Find("Sail").GetComponent<SpritesheetAnimator>().PlayAnim();
            this.SetRendererVisibility(false);
            
        }
    }

    public void PlayAnim()
    {
        Debug.Log(name + " started at " + Time.time);

        SetRendererVisibility(true);

        myRenderer.sprite = sprites[0];
        frameIndex = 0;
        frameTimer = frameInterval;
        isPlaying = true;
    }

    public void PauseAnim()
    {
        isPlaying = false;
    }

    public void SetRendererVisibility(bool isVisible)
    {
        myRenderer.enabled = isVisible;
    }
}