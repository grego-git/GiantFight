using Godot;

public class GiantActionTrackStomp : IGiantAction
{
    private Giant giant;
    private Vector3 stompTarget;
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
        giant.AnimPlayer.AnimationFinished += StompAnimationComplete;
        giant.LeftLegIK.Start();
    }

    public void Update(float delta)
    {
        if (giant.PlayerDetection.PlayerDetectionZone != PlayerDetection.DetectionZoneAreas.NEGATE && giant.TrackPlayer)
            stompTarget = Utils.GetFlatSpatialVector(giant.CharacterData.Controller.GlobalPosition, 1.5f);

        if (giant.PlayerDetection.PlayerDetectionZone != PlayerDetection.DetectionZoneAreas.NEGATE && giant.TrackPlayer)
            giant.RotateTowardsPoint(delta, stompTarget);

        giant.LeftLegIKTarget.GlobalPosition = stompTarget + (Vector3.Up * giant.StompPadding);
    }

    public void StompAnimationComplete(StringName animation)
    {
        complete = true;
        giant.AnimPlayer.AnimationFinished -= StompAnimationComplete;
        giant.LeftLegIK.Stop();
    }
}