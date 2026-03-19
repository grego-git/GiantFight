using Godot;
using System;
using System.Collections.Generic;

public partial class LimbChecker : ShapeCast3D
{
    public override void _Ready()
    {
        base._Ready();
    }

    public List<string> CheckForLimbs()
    {
        List<string> limbsDetected = new List<string>();

        ForceShapecastUpdate();

        for (int i = 0; i < GetCollisionCount(); i++)
        {
            GiantLimb limb = (GiantLimb)GetCollider(i);
            limbsDetected.Add(limb.Bone);
        }

        return limbsDetected;
    }
}
