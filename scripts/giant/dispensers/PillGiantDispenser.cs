using Godot;
using System;

public partial class PillGiantDispenser : GiantActionDispenser, IActionDispenser
{
    public override IGiantAction BottomAction(Giant giant)
    {
        return new GiantActionTrackStomp(giant);
    }

    public override IGiantAction MidAction(Giant giant)
    {
        return new GiantActionTrackClap(giant);
    }

    public override IGiantAction TopAction(Giant giant)
    {
        return new GiantActionTrackPunch(giant);
    }

    public override IGiantAction ExternalAction(Giant giant)
    {
        return new GiantActionPlayAnimation(giant, giant.GiantProfile.ExternalAttackAnimation);
    }

    public override IGiantAction NegateAction(Giant giant)
    {
        return new GiantActionPlayAnimation(giant, giant.GiantProfile.SlamAnimation);
    }

}
