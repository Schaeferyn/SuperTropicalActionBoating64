using UnityEngine;
using System.Collections;
//using Rewired;
using InControl;

public class StationWheel : BoatStation
{
    public override void ProcessControls(InputDevice player)
    {
        myBoat.xIn = player.LeftStick.X;
        myBoat.yIn = player.LeftStick.Y;
    }
}
