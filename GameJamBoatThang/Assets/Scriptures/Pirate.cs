using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using InControl;

[RequireComponent(typeof(Rigidbody))]
public class Pirate : MonoBehaviour
{
    public int playerIndex;

    Rigidbody myBody;

    float xIn;
    float yIn;

    public float moveSpeed;
    Vector3 velo = Vector3.zero;

    bool isNearStation = false;
    bool isBusy = false;

    Image myIcon;
    BoatStation nearbyStation;

    InputDevice myDevice;

	// Use this for initialization
	void Start ()
    {
        myBody = GetComponent<Rigidbody>();
        myIcon = UIManager.instance.transform.Find("p"+ (playerIndex+1).ToString() +"icon").GetComponent<Image>();
        myIcon.enabled = false;
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
                myIcon.transform.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3.forward * 0.5f));
            }
        }
        //else
        //{
        //    nearbyStation.ProcessControls(player);
        //}

        if (myDevice.Action1.WasPressed)
        {
            if (isNearStation && !isBusy)
            {
                isBusy = true;
                nearbyStation.Activate(this);
            }
        }

        if (myDevice.Action2.WasPressed)
        {
            if (isBusy)
            {
                isBusy = false;
                nearbyStation.Deactivate();

                if (isNearStation)
                    EnterStationTrigger(nearbyStation);

                //myIcon.enabled = true;
            }
        }

        if (isNearStation && nearbyStation.isActive && myIcon.enabled)
            myIcon.enabled = false;
    }

    void FixedUpdate()
    {
        velo.x = xIn * moveSpeed;
        velo.z = yIn * moveSpeed;

        myBody.velocity = velo;

        if (velo.magnitude > 0.1f)
        {
            transform.LookAt(transform.position + velo.normalized);
            transform.Rotate(90, 0, 0);
        }
    }

    public void EnterStationTrigger(BoatStation station)
    {
        isNearStation = true;
        myIcon.enabled = true;
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
}
