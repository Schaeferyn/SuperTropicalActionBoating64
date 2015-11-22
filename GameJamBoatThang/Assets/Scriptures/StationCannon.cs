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

    public override void Activate(Pirate p)
    {
        base.Activate(p);

        myBoat.fireCannon(cannon);
        anim.PlayAnim();
        p.ExitStationTrigger();
        p.EnterStationTrigger(this);
    }
}
