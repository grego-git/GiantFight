using Godot;
using System;

public partial class BronzeGiantDispenser : GiantActionDispenser, IActionDispenser
{
    [Export]
    public Node3D FlamethrowerParticles { get; set; }
    [Export]
    public Node3D FlamethrowerHitBox { get; set; }

    public override IGiantAction BottomAction(Giant giant)
    {
        return new GiantActionFlamethrower(giant, FlamethrowerParticles, FlamethrowerHitBox, giant.GiantProfile.FloorAnimation);
    }

    public override IGiantAction MidAction(Giant giant)
    {
        return null;
    }

    public override IGiantAction TopAction(Giant giant)
    {
        return new GiantActionFlamethrower(giant, FlamethrowerParticles, FlamethrowerHitBox, giant.GiantProfile.FloorAnimation);
    }

    public override IGiantAction ExternalAction(Giant giant)
    {
        return new GiantActionPlayAnimation(giant, giant.GiantProfile.ExternalAttackAnimation);
    }

    public override IGiantAction NegateAction(Giant giant)
    {
        return null;
    }

    public override IGiantAction AttackBodyAction(Giant giant, string animation, bool useLeftHand)
    {
        return null;
    }
}
