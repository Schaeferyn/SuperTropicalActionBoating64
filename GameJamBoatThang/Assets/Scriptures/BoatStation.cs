using UnityEngine;
using System.Collections;
//using Rewired;

public class BoatStation : MonoBehaviour
{
    public bool isActive = false;
    //Pirate activePirate;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Pirate")
        {
            Pirate p = other.GetComponent<Pirate>();
            //activePirate = other.GetComponent<Pirate>();
            //activePirate.EnterStationTrigger(this);
            p.EnterStationTrigger(this);
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("trigger exit");
        if (other.tag == "Pirate")
        {
            Debug.Log(other.name);

            //if(!isActive)
            //{
                Pirate p = other.GetComponent<Pirate>();
            //if(activePirate == p)
            //{
            //    activePirate.ExitStationTrigger();
            //}
            p.ExitStationTrigger();
            //}
        }
    }

    public virtual void Activate(Pirate p)
    {
        isActive = true;
        //activePirate = p;
    }

    public virtual void Deactivate()
    {
        Debug.Log(name + " deactivating at " + Time.time);
        isActive = false;
        //activePirate = null;
    }

    //public virtual void ProcessControls(Player player)
    //{

    //}
}
