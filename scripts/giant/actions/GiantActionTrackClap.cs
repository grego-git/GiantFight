using Godot;

public class GiantActionTrackClap : IGiantAction
{
    private Giant giant;
    private Vector3 clapTarget;
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
        giant.AnimPlayer.AnimationFinished += StompAnimationComplete;
        giant.LeftArmIK.Start();
        giant.RightArmIK.Start();
    }

    public void Update(float delta)
    {
        if (giant.PlayerDetection.PlayerDetectionZone != PlayerDetection.DetectionZoneAreas.NEGATE && giant.TrackPlayer)
            clapTarget = giant.CharacterData.Controller.GlobalPosition + (Vector3.Down * 15.0f);

        if (giant.PlayerDetection.PlayerDetectionZone != PlayerDetection.DetectionZoneAreas.NEGATE && giant.TrackPlayer)
            giant.RotateTowardsPoint(delta, clapTarget);

        giant.LeftArmIKTarget.GlobalPosition = clapTarget + (giant.GlobalBasis.X.Normalized() * giant.StompPadding);
        giant.RightArmIKTarget.GlobalPosition = clapTarget - (giant.GlobalBasis.X.Normalized() * giant.StompPadding);
    }

    public void StompAnimationComplete(StringName animation)
    {
        complete = true;
        giant.AnimPlayer.AnimationFinished -= StompAnimationComplete;
        giant.LeftArmIK.Stop();
        giant.RightArmIK.Stop();
    }
}