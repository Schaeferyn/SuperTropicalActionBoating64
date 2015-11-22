using UnityEngine;
using System.Collections;

public class StationAmmo : BoatStation
{

    public override void Activate(Pirate p)
    {
        base.Activate(p);

        //p.hasAmmo = true;
        p.PickupAmmo();
        p.ExitStationTrigger();
        p.EnterStationTrigger(this);
    }
}
