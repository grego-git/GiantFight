using Godot;
using System;
using System.Collections.Generic;

public partial class Giant : Node3D
{
    public enum State
    {
        DETERMINING,
        ACTION
    }

    [Export]
    public CharacterData CharacterData { get; set; }
    [Export]
    public float TurnSpeed { get; set; }
    [Export]
    public float StompPadding { get; set; }
    [Export]
    public bool TrackPlayer { get; set; }

    [Export]
    public ClimbableAnimatedEntity[] ClimbAnimatedEntities { get; set; }

    [Export]
    public SkeletonIK3D LeftLegIK { get; set; }

    public State CurrentState { get; set; }
    public PlayerDetection PlayerDetection { get; set; }
    public AnimationPlayer AnimPlayer { get; set; }

    public Node3D LeftLegIKTarget { get; set; }

    private float yRot;

    private Vector3 StompTarget;

    public override void _Ready()
    {
        base._Ready();

        PlayerDetection = (PlayerDetection)GetNode("PlayerDetection");
        AnimPlayer = (AnimationPlayer)GetNode("AnimationPlayer");

        LeftLegIKTarget = (Node3D)GetNode("LeftLegIKTarget");

        LeftLegIK.Start();

        AnimPlayer.AnimationFinished += AnimationFinished;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        switch (CurrentState)
        {
            case State.DETERMINING:
                PlayerDetection.Update((float)delta);

                switch (PlayerDetection.PlayerDetectionZone)
                {
                    case PlayerDetection.DetectionZoneAreas.NONE:
                    case PlayerDetection.DetectionZoneAreas.ON_GIANT:
                        break;
                    case PlayerDetection.DetectionZoneAreas.FLOOR:
                        AnimPlayer.Play("pill_giant/stomp");
                        CurrentState = State.ACTION;
                        break;
                    default:
                        break;
                }
                break;
            case State.ACTION:
                switch (PlayerDetection.PlayerDetectionZone)
                {
                    case PlayerDetection.DetectionZoneAreas.NONE:
                    case PlayerDetection.DetectionZoneAreas.ON_GIANT:
                        break;
                    default:
                        if (TrackPlayer)
                            StompTarget = Utils.GetFlatSpatialVector(CharacterData.Controller.GlobalPosition, 1.5f);

                        if (TrackPlayer && PlayerDetection.DistanceToPlayer > 5.0f)
                            RotateTowardsPoint((float)delta, StompTarget);
                        
                        LeftLegIKTarget.GlobalPosition = StompTarget + (Vector3.Up * StompPadding);
                        break;
                }
                break;
        }
        
    }

    private void RotateTowardsPlayer(float delta)
    {
        yRot = (float)MoveTowardsAngle(yRot, PlayerDetection.AngleToPlayer, TurnSpeed * delta);
        GlobalRotation = new Vector3(GlobalRotation.X, yRot, GlobalRotation.Z);
    }

    private void RotateTowardsPoint(float delta, Vector3 point)
    {
        Vector3 toPoint = Utils.GetFlatSpatialVector(point, GlobalPosition.Y) - GlobalPosition;
        float angleToPoint = Vector3.Back.SignedAngleTo(toPoint.Normalized(), Vector3.Up);

        yRot = (float)MoveTowardsAngle(yRot, angleToPoint, TurnSpeed * delta);
        GlobalRotation = new Vector3(GlobalRotation.X, yRot, GlobalRotation.Z);
    }

    private void AnimationFinished(StringName animation)
    {
        if (animation != "pill_giant/idle")
        {
            AnimPlayer.Play("pill_giant/idle");
            CurrentState = State.DETERMINING;
        }
    }

    public static float MoveTowardsAngle(float current, float target, float step)
    {
        // 1. Normalize both to 0–360
        current = current % (2.0f * Mathf.Pi);
        
        if (current < 0) 
            current += 2.0f * Mathf.Pi;

        target = target % (2.0f * Mathf.Pi);
        
        if (target < 0) 
            target += 2.0f * Mathf.Pi;

        // 2. Find shortest signed difference (-180 to 180)
        float diff = (target - current + Mathf.Pi) % (2.0f * Mathf.Pi);
        
        if (diff < 0) 
            diff += 2.0f * Mathf.Pi;
        
        diff -= Mathf.Pi;

        // 3. Snap to target if within step distance
        if (Math.Abs(diff) <= step)
            return target;

        // 4. Step toward target and wrap again
        float direction = diff > 0 ? 1f : -1f;
        float result = current + direction * step;

        result = result % (2.0f * Mathf.Pi);
        
        if (result < 0) 
            result += 2.0f * Mathf.Pi;

        return result;
    }
}
