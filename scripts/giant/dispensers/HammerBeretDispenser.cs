using Godot;
using System;

public partial class HammerBeretDispenser : GiantActionDispenser, IActionDispenser
{
    public override IGiantAction BottomAction(Giant giant)
    {
        return new GiantActionPlayAnimation(giant, giant.GiantProfile.FloorAnimation);
    }

    public override IGiantAction MidAction(Giant giant)
    {
        return null;
    }

    public override IGiantAction TopAction(Giant giant)
    {
        return null;
    }

    public override IGiantAction ExternalAction(Giant giant)
    {
        return new GiantActionChargeAttack(giant, giant.GiantProfile.ChargeAttackAnimation, giant.GiantProfile.ExternalAttackAnimation, 200.0f);
    }

    public override IGiantAction NegateAction(Giant giant)
    {
        return null;
    }

}
