using Godot;

public class GiantActionFlamethrower : IGiantAction
{
    private Giant giant;
    private Node3D particles;
    private Node3D hitbox;
    private Node3D warningCircle;
    private Vector3 target;
    private string animation;
    private bool complete;
    private bool start;
    private float turnSpeed;
    private float minTurnSpeed;
    private float maxTurnSpeed;
    private float acceleration;
    private float decceleration;

    public GiantActionFlamethrower(Giant giant, Node3D particles, Node3D hitbox, Node3D warningCircle, string animation)
    {
        this.giant = giant;
        this.particles = particles;
        this.hitbox = hitbox;
        this.warningCircle = warningCircle;
        this.animation = animation;

        complete = false;
        start = true;
        
        minTurnSpeed = 0.03f;
        maxTurnSpeed = 0.75f;
        turnSpeed = minTurnSpeed;

        acceleration = 0.1f;
        decceleration = 0.03f;
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
        giant.LeftArmIK.Start();
        giant.RightArmIK.Start();
    }

    public void Update(float delta)
    {
        if (giant.PlayerDetection.PlayerDetectionZone != PlayerDetection.DetectionZoneAreas.NEGATE &&
            giant.PlayerDetection.PlayerDetectionZone != PlayerDetection.DetectionZoneAreas.ON_GIANT)
        {
            warningCircle.Visible = true;

            if (giant.TrackPlayer)
            {
                start = false;
                giant.RotateTowardsPoint(delta, giant.PlayerDetection.PlayerPosition, turnSpeed);
            }
            else if (start)
            {
                giant.RotateTowardsPoint(delta, giant.PlayerDetection.PlayerPosition, giant.TurnSpeed);
            }
        }
        else
            warningCircle.Visible = false;

        GD.Print("TS: " + turnSpeed);

        if (giant.TrackPlayer)
        {
            particles.GlobalPosition = giant.ArmLimbs[1].GlobalPosition.Lerp(giant.ArmLimbs[3].GlobalPosition, 0.5f);
            particles.LookAt(target);
            hitbox.GlobalPosition = particles.GlobalPosition;
            hitbox.LookAt(target);
            CalculateIKPos(delta);
        }
    }

    public void AnimationComplete(StringName animation)
    {
        complete = true;
        giant.AnimPlayer.AnimationFinished -= AnimationComplete;
        giant.AgroMeter.Empty();
    }

    public void CalculateIKPos(float delta)
    {
        Vector3 targetOffset = giant.PlayerDetection.PlayerPosition - giant.GlobalPosition;
        float angleToPlayer = new Vector3(targetOffset.X, 0.0f, targetOffset.Z).SignedAngleTo(giant.GlobalBasis.Z, Vector3.Up);
        targetOffset = targetOffset.Rotated(Vector3.Up, angleToPlayer);

        target = giant.GlobalPosition + targetOffset + (Vector3.Down * 0.65f);

        if (target.DistanceTo(giant.PlayerDetection.PlayerPosition) > 10.0f)
            turnSpeed += delta * acceleration;
        else if (target.DistanceTo(giant.PlayerDetection.PlayerPosition) < 5.0f)
            turnSpeed -= delta * decceleration;

        turnSpeed = Mathf.Clamp(turnSpeed, minTurnSpeed, maxTurnSpeed);

        giant.LeftArmIKTarget.GlobalPosition = target;
        giant.RightArmIKTarget.GlobalPosition = target;
    }
}