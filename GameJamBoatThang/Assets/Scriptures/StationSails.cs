using UnityEngine;
using System.Collections;
using Rewired;

public class StationSails : BoatStation
{
    public SpritesheetAnimator sailOpen;
    public SpritesheetAnimator sailClosed;
    public SpritesheetAnimator sailIdle;

    public override void Activate(Pirate p)
    {
        base.Activate(p);

        sailClosed.SetRendererVisibility(false);
        sailIdle.SetRendererVisibility(false);
        sailOpen.SetRendererVisibility(true);
        sailOpen.PlayAnim();
        //sailOpen.AnimEnded() => 
        //{
            
        //}
    }

    public override void Deactivate()
    {
        base.Deactivate();

        sailOpen.SetRendererVisibility(false);
        sailIdle.SetRendererVisibility(false);
        sailClosed.SetRendererVisibility(true);
        sailClosed.PlayAnim();
    }

    public override void ProcessControls(Player player)
    {

    }
}
