using Godot;

public class GiantActionTrackStomp : IGiantAction
{
    private Giant giant;
    private Vector3 stompTarget;
    private Vector3 rotatePoint;
    private bool complete;

    public GiantActionTrackStomp(Giant giant)
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
        giant.AnimPlayer.Play(giant.GiantProfile.FloorAnimation);
        giant.CurrentState = Giant.State.ACTION;
        giant.AnimPlayer.AnimationFinished += AnimationComplete;
        giant.LeftLegIK.Start();
    }

    public void Update(float delta)
    {
        if (giant.PlayerDetection.PlayerDetectionZone != PlayerDetection.DetectionZoneAreas.NEGATE && giant.TrackPlayer)
            stompTarget = Utils.GetFlatSpatialVector(giant.PlayerDetection.PlayerPosition, 1.5f);

        if (giant.PlayerDetection.PlayerDetectionZone != PlayerDetection.DetectionZoneAreas.NEGATE &&
            giant.PlayerDetection.PlayerDetectionZone != PlayerDetection.DetectionZoneAreas.ON_GIANT &&
            giant.TrackPlayer)
            rotatePoint = giant.PlayerDetection.PlayerPosition;
        
        giant.RotateTowardsPoint(delta, rotatePoint);
        giant.LeftLegIKTarget.GlobalPosition = stompTarget + (Vector3.Up * giant.StompPadding);
    }

    public void AnimationComplete(StringName animation)
    {
        complete = true;
        giant.AnimPlayer.AnimationFinished -= AnimationComplete;
        giant.LeftLegIK.Stop();
        giant.AgroMeter.Empty();
    }
}