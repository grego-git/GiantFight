using Godot;

public class GiantActionTrackClap : IGiantAction
{
    private Giant giant;
    private Vector3 clapTarget;
    private Vector3 rotatePoint;
    private bool complete;

    public GiantActionTrackClap(Giant giant)
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
        giant.AnimPlayer.Play(giant.GiantProfile.MidAnimation);
        giant.CurrentState = Giant.State.ACTION;
        giant.AnimPlayer.AnimationFinished += AnimationComplete;
        giant.LeftArmIK.Start();
        giant.RightArmIK.Start();
    }

    public void Update(float delta)
    {
        if (giant.PlayerDetection.PlayerDetectionZone == PlayerDetection.DetectionZoneAreas.MIDDLE && giant.TrackPlayer)
            clapTarget = giant.PlayerDetection.PlayerPosition;

        if (giant.PlayerDetection.PlayerDetectionZone != PlayerDetection.DetectionZoneAreas.NEGATE &&
            giant.PlayerDetection.PlayerDetectionZone != PlayerDetection.DetectionZoneAreas.ON_GIANT && 
            giant.TrackPlayer)
            rotatePoint = giant.PlayerDetection.PlayerPosition;
        
        giant.RotateTowardsPoint(delta, rotatePoint);
        giant.LeftArmIKTarget.GlobalPosition = clapTarget + 
            (giant.PlayerDetection.PlayerDetectionZone == PlayerDetection.DetectionZoneAreas.MIDDLE ? Vector3.Down * 15.0f : Vector3.Zero) + 
            (giant.GlobalBasis.X.Normalized() * giant.StompPadding);
        
        giant.RightArmIKTarget.GlobalPosition = clapTarget + 
            (giant.PlayerDetection.PlayerDetectionZone == PlayerDetection.DetectionZoneAreas.MIDDLE ? Vector3.Down * 15.0f : Vector3.Zero) - 
            (giant.GlobalBasis.X.Normalized() * giant.StompPadding);
    }

    public void AnimationComplete(StringName animation)
    {
        complete = true;
        giant.AnimPlayer.AnimationFinished -= AnimationComplete;
        giant.LeftArmIK.Stop();
        giant.RightArmIK.Stop();
    }
}