using Godot;

public class GiantActionTrackPunch : IGiantAction
{
    private Giant giant;
    private Vector3 punchTarget;
    private Vector3 rotatePoint;
    private bool complete;

    public GiantActionTrackPunch(Giant giant)
    {
        this.giant = giant;

        complete = false;
    }

    public bool Complete()
    {
        return complete;
    }

    public void Init()
    {
        giant.AnimPlayer.Play(giant.GiantProfile.TopAnimation);
        giant.CurrentState = Giant.State.ACTION;
        giant.AnimPlayer.AnimationFinished += AnimationComplete;
        giant.LeftArmIK.Start();
    }

    public void Update(float delta)
    {        
        if (giant.PlayerDetection.PlayerDetectionZone == PlayerDetection.DetectionZoneAreas.TOP && giant.TrackPlayer)
            punchTarget = giant.PlayerDetection.PlayerPosition + (Vector3.Down * 15.0f);

        if (giant.PlayerDetection.PlayerDetectionZone != PlayerDetection.DetectionZoneAreas.NEGATE && 
            giant.PlayerDetection.PlayerDetectionZone != PlayerDetection.DetectionZoneAreas.ON_GIANT && 
            giant.TrackPlayer)
            rotatePoint = giant.PlayerDetection.PlayerPosition;

        Vector3 shoulderPos = (giant.Skeleton.GlobalTransform * giant.Skeleton.GetBoneGlobalPose(giant.Skeleton.FindBone("Upperarm.L"))).Origin;
        Vector3 punchDir = (punchTarget - shoulderPos).Normalized();

        giant.RotateTowardsPoint(delta, rotatePoint);
        giant.LeftArmIKTarget.GlobalPosition = shoulderPos + (punchDir * 100.0f);
    }

    public void AnimationComplete(StringName animation)
    {
        complete = true;
        giant.AnimPlayer.AnimationFinished -= AnimationComplete;
        giant.LeftArmIK.Stop();
        giant.AgroMeter.Empty();
    }
}