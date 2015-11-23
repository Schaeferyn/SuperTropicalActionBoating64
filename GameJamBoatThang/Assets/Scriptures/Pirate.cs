using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using InControl;

public class Pirate : MonoBehaviour
{
    public int playerIndex;

    //Rigidbody myBody;

    float xIn;
    float yIn;

    public float moveSpeed;
    Vector3 velo = Vector3.zero;

    bool isNearStation = false;
    bool isBusy = false;

    Image myIcon;
    public Sprite goodIcon;
    public Sprite badIcon;

    BoatStation nearbyStation;

    InputDevice myDevice;

    bool isOnBoat = true;
    Boat myBoat;
    Camera pirateCam;

    public bool hasAmmo = false;
    SpriteRenderer ammoRenderer;

	float y;

	// Use this for initialization
	void Start ()
    {
        //myBody = GetComponent<Rigidbody>();
        myIcon = UIManager.instance.transform.Find("p"+ (playerIndex+1).ToString() +"icon").GetComponent<Image>();
        myIcon.enabled = false;

        myBoat = transform.parent.GetComponent<Boat>();
        pirateCam = myBoat.GetComponentInChildren<Camera>();
        ammoRenderer = transform.Find("Ammo").GetComponent<SpriteRenderer>();
        ammoRenderer.enabled = false;

		y = transform.position.y;
    }

    // Update is called once per frame
    void Update ()
    {
        if (InputManager.Devices.Count - 1 >= playerIndex)
            myDevice = InputManager.Devices[playerIndex];
        else
            myDevice = null;

        if (myDevice == null) return;

        if (!isBusy)
        {
            xIn = myDevice.LeftStick.X;
            yIn = myDevice.LeftStick.Y;

            if (myIcon.enabled)
            {
                myIcon.transform.position = pirateCam.WorldToScreenPoint(transform.position + (Vector3.forward * 0.05f));
            }
        }
        else
        {
            nearbyStation.ProcessControls(myDevice);
        }

        if (myDevice.Action1.WasPressed)
        {
            if (isNearStation && !isBusy && myIcon.sprite != badIcon)
            {
                isBusy = true;
                nearbyStation.Activate(this);
            }
        }

        if (myDevice.Action2.WasPressed)
        {
            if (isBusy)
            {
                
                nearbyStation.Deactivate();

                if (isNearStation)
                    EnterStationTrigger(nearbyStation);

                //myIcon.enabled = true;
            }
			isBusy = false;
        }

        if (isNearStation && nearbyStation.isActive && myIcon.enabled)
            myIcon.enabled = false;
    }

    void FixedUpdate()
    {
        velo.x = xIn * moveSpeed * Time.deltaTime;
        velo.y = yIn * moveSpeed * Time.deltaTime;

        //myBody.velocity = velo + myBoat.GetComponent<Rigidbody>().velocity;
        Vector3 newpos = transform.localPosition += velo;
        newpos.x = Mathf.Clamp(newpos.x, -0.315f, 0.315f);
        newpos.y = Mathf.Clamp(newpos.y, -0.925f, 0.177f);
        transform.localPosition = newpos;

		Vector3 pos = transform.position;
		pos.y = y;
		transform.position = pos;


        //Debug.Log(velo.magnitude);
        velo.z = velo.y;
        velo.y = 0;

        if (Mathf.Abs(xIn) > 0.1 || Mathf.Abs(yIn) > 0.1)
        {
            transform.LookAt(transform.position + velo.normalized);
            transform.Rotate(90, myBoat.transform.eulerAngles.y, 0);
        }
    }

    public void EnterStationTrigger(BoatStation station)
    {
        isNearStation = true;
        myIcon.enabled = true;
        if(station.GetType() == typeof(StationCannon))
        {
            if((station as StationCannon).hasAmmo)
            {
                if (hasAmmo)
                    myIcon.sprite = badIcon;
                else
                    myIcon.sprite = goodIcon;
            }
            else
            {
                if (hasAmmo)
                    myIcon.sprite = goodIcon;
                else
                    myIcon.sprite = badIcon;
            }
        }
        else
        {
            myIcon.sprite = goodIcon;
        }

        nearbyStation = station;
    }

    public void ExitStationTrigger()
    {
        if (isBusy)
            nearbyStation.Deactivate();

        isBusy = false;
        isNearStation = false;
        myIcon.enabled = false;
        nearbyStation = null; 
    }

    public void PickupAmmo()
    {
        hasAmmo = true;
        ammoRenderer.enabled = true;
    }

    public void DropAmmo()
    {
        hasAmmo = false;
        ammoRenderer.enabled = false;
    }
}
