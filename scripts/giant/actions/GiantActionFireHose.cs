using System;
using Godot;

public class GiantActionFireHose : IGiantAction
{
    private Giant giant;
    private GpuParticles3D particles;
    private Area3D hitbox;
    private Node3D warningCircle;
    
    private Vector3 currentTarget;
    
    private float desiredTargetDistance;
    private float currentTargetDistance;

    private float distanceSpeed;
    private float minDistanceSpeed;
    private float maxDistanceSpeed;
    private float acceleration;
    private float decceleration;
    
    private float rotSpeed;
    private float maxRotSpeed;
    private float rotAccel;

    private float attackTime;
    private string animation;
    private bool animationEnd;

    public GiantActionFireHose(Giant giant, GpuParticles3D particles, Area3D hitbox, Node3D warningCircle, string animation)
    {
        this.giant = giant;
        this.particles = particles;
        this.hitbox = hitbox;
        this.warningCircle = warningCircle;
        this.animation = animation;

        currentTargetDistance = -1.0f;

        maxRotSpeed = 3.0f;
        rotAccel = 0.5f;
        
        minDistanceSpeed = 5.0f;
        maxDistanceSpeed = 45.0f;
        distanceSpeed = minDistanceSpeed;

        acceleration = 10.0f;
        decceleration = 7.5f;
        
        attackTime = 20.0f;

        animationEnd = false;
    }

    public bool Complete()
    {
        return animationEnd && rotSpeed == 0.0f;
    }

    public void Init()
    {
        giant.AnimPlayer.Play(animation);
        giant.CurrentState = Giant.State.ACTION;
        warningCircle.Visible = true;
    
        GD.Print("HOSE INIT");
    }

    public void Update(float delta)
    {
        Vector3 playerPosition = giant.PlayerDetection.PlayerPosition;
        Vector3 giantPosition = Utils.GetFlatSpatialVector(giant.GlobalPosition, playerPosition.Y);
        desiredTargetDistance = playerPosition.DistanceTo(giantPosition);

        if (giant.TrackPlayer && attackTime > 0.0f)
        {
            attackTime -= delta;

            Spin(delta);

            if (attackTime <= 0.0f)
            {
                giant.AnimPlayer.Play(animation + "_end");
                giant.AnimPlayer.AnimationFinished += AnimationComplete;
            }

            particles.Emitting = true;
            hitbox.Monitorable = true;
            hitbox.Monitoring = true;
        }
        else if (attackTime <= 0.0f)
        {
            Spin(-delta);

            particles.Emitting = false;
            hitbox.Monitorable = false;
            hitbox.Monitoring = false;
        }
        
        if (currentTargetDistance == -1.0f)
            currentTargetDistance = desiredTargetDistance;
        
        particles.GlobalPosition = giant.ArmLimbs[1].GlobalPosition.Lerp(giant.ArmLimbs[3].GlobalPosition, 0.5f);
        particles.LookAt(currentTarget);
        hitbox.GlobalPosition = particles.GlobalPosition;
        hitbox.LookAt(currentTarget);
        warningCircle.Scale = new Vector3(warningCircle.Scale.X, warningCircle.Scale.Y, warningCircle.Scale.X / (Mathf.Sin(particles.Rotation.X) == 0.0f ? 1.0f : Mathf.Sin(particles.Rotation.X)));

        CalculateIKPos(delta);
        UpdateDistanceSpeed(delta);

        warningCircle.GlobalPosition = giantPosition + ((playerPosition - giantPosition).Normalized() * currentTargetDistance) + (Vector3.Down * 0.55f);
        warningCircle.GlobalRotation = new Vector3(0.0f, Vector3.Back.SignedAngleTo(playerPosition - giantPosition, Vector3.Up), 0.0f);
    }

    public void AnimationComplete(StringName animation)
    {
        animationEnd = true;
        giant.AnimPlayer.AnimationFinished -= AnimationComplete;
        giant.AgroMeter.Empty();
    }

    private void Spin(float delta)
    {
        rotSpeed += rotAccel * delta;
        rotSpeed = Mathf.Clamp(rotSpeed, 0.0f, maxRotSpeed);

        giant.RotateYRot(rotSpeed * Mathf.Abs(delta));
    }

    public void CalculateIKPos(float delta)
    {
        currentTargetDistance = Mathf.MoveToward(currentTargetDistance, desiredTargetDistance, distanceSpeed * delta);        
        currentTarget = giant.GlobalPosition + (giant.GlobalBasis.Z.Normalized() * currentTargetDistance);
        currentTarget.Y = giant.PlayerDetection.PlayerPosition.Y - 0.65f;

        giant.LeftArmIKTarget.GlobalPosition = currentTarget;
        giant.RightArmIKTarget.GlobalPosition = currentTarget;
    }

    public void UpdateDistanceSpeed(float delta)
    {
        if (Mathf.Abs(desiredTargetDistance - currentTargetDistance) > 10.0f)
            distanceSpeed += delta * acceleration;
        else if (Mathf.Abs(desiredTargetDistance - currentTargetDistance) < 5.0f)
            distanceSpeed -= delta * decceleration;

        distanceSpeed = Mathf.Clamp(distanceSpeed, minDistanceSpeed, maxDistanceSpeed);        
    }
}