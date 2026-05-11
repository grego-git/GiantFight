using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

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

            if (limb.Monitorable)
            {
                limbsDetected.Add(limb.Bone);

                if (limb.AdditionalDetectBones == null || limb.AdditionalDetectBones.Count() == 0)
                    continue;
                
                foreach (string additionalBone in limb.AdditionalDetectBones)
                {
                    limbsDetected.Add(additionalBone);
                }
            }
        }

        return limbsDetected;
    }
}
