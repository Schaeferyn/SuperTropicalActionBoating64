using UnityEngine;
using System.Collections;
//using Rewired;

public class StationCannon : BoatStation
{
    //public override void ProcessControls(Player player)
    //{

    //}

    public Transform cannon;
    public SpritesheetAnimator anim;

    public bool hasAmmo = false;

    public override void Activate(Pirate p)
    {
        base.Activate(p);

        
        if(hasAmmo)
        {
            myBoat.fireCannon(cannon);
            anim.PlayAnim();
            hasAmmo = false;
        }
        else if(p.hasAmmo)
        {
            p.DropAmmo();
            hasAmmo = true;
        }

        p.ExitStationTrigger();
        p.EnterStationTrigger(this);
    }
}
