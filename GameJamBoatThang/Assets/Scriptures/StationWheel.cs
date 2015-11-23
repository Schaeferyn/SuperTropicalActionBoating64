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

	public override void Deactivate ()
	{
		base.Deactivate ();

		myBoat.xIn = 0;
		myBoat.yIn = 0;
	}
}
