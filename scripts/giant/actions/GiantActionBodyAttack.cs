using System.Linq;
using Godot;

public class GiantActionBodyAttack : IGiantAction
{
    private Giant giant;
    private SkeletonIK3D ik;
    private Node3D ikTarget;

    private Transform3D targetTransform;
    private string lastBoneOne;

    private float paddingDirLerpWeight;
    private string animation;

    private bool complete;
    private bool useLeftHand;

    public GiantActionBodyAttack(Giant giant, string animation, bool useLeftHand)
    {
        this.giant = giant;
        this.animation = animation;
        this.useLeftHand = useLeftHand;

        if (useLeftHand)
        {
            ik = giant.LeftArmIK;
            ikTarget = giant.LeftArmIKTarget;
        }
        else
        {
            ik = giant.RightArmIK;
            ikTarget = giant.RightArmIKTarget; 
        }

        complete = false;
    }

    public bool Complete()
    {
        return complete;
    }

    public void Init()
    {
        giant.AnimPlayer.Play(animation);
        giant.CurrentState = Giant.State.ACTION;
        giant.AnimPlayer.AnimationFinished += AnimationComplete;

        ik.Start();
        ik.Influence = 0.0f;
        ik.UseMagnet = true;

        paddingDirLerpWeight = -1.0f;
        
        foreach (var bone in giant.BonesPlayerIsOn)
        {
            targetTransform = giant.CharacterData.Controller.GlobalTransform;
            lastBoneOne = bone;
            break;
        }
    }

    public void Update(float delta)
    {
        if (giant.TrackPlayer)
        {
            if (giant.PlayerDetection.PlayerDetectionZone == PlayerDetection.DetectionZoneAreas.ON_GIANT){
                foreach (var bone in giant.BonesPlayerIsOn)
                {
                    if (!giant.GiantProfile.AttackAnimations[animation].Contains(bone))
                        continue;

                    targetTransform = giant.CharacterData.Controller.GlobalTransform;
                    lastBoneOne = bone;
                    break;
                }
            }

            for (int i = useLeftHand ? 0 : 2; i < (useLeftHand ? 2 : 4); i++)
            {
                giant.ArmLimbs[i].Monitorable = false;
                giant.ArmLimbs[i].Monitoring = false;
            }
        }
        else
        {
            for (int i = useLeftHand ? 0 : 2; i < (useLeftHand ? 2 : 4); i++)
            {
                giant.ArmLimbs[i].Monitorable = true;
                giant.ArmLimbs[i].Monitoring = true;
            }
        }
        
        Transform3D currentBoneTransform = giant.Skeleton.GlobalTransform * giant.Skeleton.GetBoneGlobalPose(giant.Skeleton.FindBone(lastBoneOne));
        Vector3 right = currentBoneTransform.Basis.X.Normalized();
        Vector3 up = currentBoneTransform.Basis.Y.Normalized();

        float dot = right.Dot(targetTransform.Basis.Y.Normalized());

        Vector3 displacementFromBase = Utils.ProjectOntoLine(targetTransform.Origin - currentBoneTransform.Origin, up);
        Vector3 pointUpBone = currentBoneTransform.Origin + displacementFromBase;

        ik.Magnet = Vector3.Right * 100.0f * (useLeftHand ? 1.0f : -1.0f);

        if (paddingDirLerpWeight == -1.0f)
            paddingDirLerpWeight = dot < 0.0f ? 1.0f : 0.0f;
        else
        {
            paddingDirLerpWeight += delta * Mathf.Sign(dot) * 3.0f;
            paddingDirLerpWeight = Mathf.Clamp(paddingDirLerpWeight, 0.0f, 1.0f);
        }

        Vector3 targetPoint = pointUpBone + ((-right).Rotated(up, (-right).AngleTo(right) * paddingDirLerpWeight) * giant.StompPadding);

        ikTarget.GlobalPosition = targetPoint;
    }

    public void AnimationComplete(StringName animation)
    {
        complete = true;
        giant.AnimPlayer.AnimationFinished -= AnimationComplete;
        giant.AgroMeter.Empty();

        ik.Stop();
        ik.Influence = 0.0f;
        ik.UseMagnet = false;
    }
}