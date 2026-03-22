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
    [Export]
    public SkeletonIK3D LeftArmIK { get; set; }
    [Export]
    public SkeletonIK3D RightArmIK { get; set; }

    public IGiantAction CurrentAction { get; set; }
    public State CurrentState { get; set; }
    public Skeleton3D Skeleton { get; set; }
    public PlayerDetection PlayerDetection { get; set; }
    public AnimationPlayer AnimPlayer { get; set; }

    public Node3D LeftLegIKTarget { get; set; }
    public Node3D LeftArmIKTarget { get; set; }
    public Node3D RightArmIKTarget { get; set; }

    private float yRot;

    public override void _Ready()
    {
        base._Ready();

        Skeleton = (Skeleton3D)GetNode("Armature/Skeleton3D");
        PlayerDetection = (PlayerDetection)GetNode("PlayerDetection");
        AnimPlayer = (AnimationPlayer)GetNode("AnimationPlayer");

        LeftLegIKTarget = (Node3D)GetNode("LeftLegIKTarget");
        LeftArmIKTarget = (Node3D)GetNode("LeftArmIKTarget");
        RightArmIKTarget = (Node3D)GetNode("RightArmIKTarget");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        switch (CurrentState)
        {
            case State.DETERMINING:
                PlayerDetection.Update(CharacterData, ClimbAnimatedEntities);

                switch (PlayerDetection.PlayerDetectionZone)
                {
                    case PlayerDetection.DetectionZoneAreas.NONE:
                    case PlayerDetection.DetectionZoneAreas.ON_GIANT:
                        break;
                    case PlayerDetection.DetectionZoneAreas.FLOOR:
                        CurrentAction = new GiantActionTrackStomp(this);
                        CurrentAction.Init();
                        break;
                    case PlayerDetection.DetectionZoneAreas.MIDDLE:
                        CurrentAction = new GiantActionTrackClap(this);
                        CurrentAction.Init();
                        break;
                    case PlayerDetection.DetectionZoneAreas.TOP:
                        CurrentAction = new GiantActionTrackPunch(this);
                        CurrentAction.Init();
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
                        CurrentAction.Update((float)delta);

                        if (CurrentAction.Complete())
                        {
                            AnimPlayer.Play("pill_giant/idle");
                            CurrentState = State.DETERMINING;
                        }
                        break;
                }
                break;
        }
        
    }

    public void RotateTowardsPlayer(float delta)
    {
        yRot = (float)Utils.MoveTowardsAngle(yRot, PlayerDetection.AngleToPlayer, TurnSpeed * delta);
        GlobalRotation = new Vector3(GlobalRotation.X, yRot, GlobalRotation.Z);
    }

    public void RotateTowardsPoint(float delta, Vector3 point)
    {
        Vector3 toPoint = Utils.GetFlatSpatialVector(point, GlobalPosition.Y) - GlobalPosition;
        float angleToPoint = Vector3.Back.SignedAngleTo(toPoint.Normalized(), Vector3.Up);

        yRot = (float)Utils.MoveTowardsAngle(yRot, angleToPoint, TurnSpeed * delta);
        GlobalRotation = new Vector3(GlobalRotation.X, yRot, GlobalRotation.Z);
    }
}
