using Godot;

public class GiantActionPlayAnimation : IGiantAction
{
    private Giant giant;
    private string animation;
    private bool complete;

    public GiantActionPlayAnimation(Giant giant, string animation)
    {
        this.giant = giant;
        this.animation = animation;

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
    }

    public void Update(float delta)
    {
        if (giant.PlayerDetection.PlayerDetectionZone != PlayerDetection.DetectionZoneAreas.NEGATE &&
            giant.PlayerDetection.PlayerDetectionZone != PlayerDetection.DetectionZoneAreas.ON_GIANT && 
            giant.TrackPlayer)
        {
            giant.RotateTowardsPoint(delta, giant.PlayerDetection.PlayerPosition);
        }
    }

    public void AnimationComplete(StringName animation)
    {
        complete = true;
        giant.AnimPlayer.AnimationFinished -= AnimationComplete;
        giant.AgroMeter.Empty();
    }
}