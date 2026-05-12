using Godot;

public class GiantActionTrackClap : IGiantAction
{
    private Giant giant;
    private Vector3 clapTarget;
    private Vector3 rotatePoint;
    private Vector3 leftMagnet;
    private Vector3 rightMagnet;
    private bool complete;

    public GiantActionTrackClap(Giant giant, Vector3 leftMagnet, Vector3 rightMagnet)
    {
        this.giant = giant;

        this.leftMagnet = leftMagnet;
        this.rightMagnet = rightMagnet;

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

        if (leftMagnet != Vector3.Zero)
        {
            giant.LeftArmIK.UseMagnet = true;
            giant.LeftArmIK.Magnet = leftMagnet;
        }
        
        if (rightMagnet != Vector3.Zero)
        {
            giant.RightArmIK.UseMagnet = true;
            giant.RightArmIK.Magnet = rightMagnet;
        }
    }

    public void Update(float delta)
    {
        if ((giant.PlayerDetection.PlayerDetectionZone == PlayerDetection.DetectionZoneAreas.MIDDLE || giant.PlayerDetection.PlayerDetectionZone == PlayerDetection.DetectionZoneAreas.TOP) && giant.TrackPlayer)
            clapTarget = giant.PlayerDetection.PlayerPosition + (Vector3.Down * 15.0f);

        if (giant.PlayerDetection.PlayerDetectionZone != PlayerDetection.DetectionZoneAreas.NEGATE &&
            giant.PlayerDetection.PlayerDetectionZone != PlayerDetection.DetectionZoneAreas.ON_GIANT && 
            giant.TrackPlayer)
            rotatePoint = giant.PlayerDetection.PlayerPosition + (Vector3.Down * 15.0f);
        
        giant.RotateTowardsPoint(delta, rotatePoint, giant.TurnSpeed);
        giant.LeftArmIKTarget.GlobalPosition = clapTarget + 
            (giant.GlobalBasis.X.Normalized() * giant.StompPadding);
        
        giant.RightArmIKTarget.GlobalPosition = clapTarget - 
            (giant.GlobalBasis.X.Normalized() * giant.StompPadding);
    }

    public void AnimationComplete(StringName animation)
    {
        complete = true;
        giant.AnimPlayer.AnimationFinished -= AnimationComplete;
        giant.LeftArmIK.Stop();
        giant.RightArmIK.Stop();
        giant.AgroMeter.Empty();
        
        if (leftMagnet != Vector3.Zero)
            giant.LeftArmIK.UseMagnet = false;
        
        if (rightMagnet != Vector3.Zero)
            giant.RightArmIK.UseMagnet = false;
    }
}