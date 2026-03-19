using Godot;
using System;

public partial class CharacterModel : Node3D
{
    [Export]
    public Node3D LeftArmIKTarget { get; set; }
    [Export]
    public Node3D RightArmIKTarget { get; set; }
    [Export]
    public Node3D LeftLegIKTarget { get; set; }
    [Export]
    public Node3D RightLegIKTarget { get; set; }
    [Export]
    public string ModelNode;
    [Export]
    public string AnimationLibrary;

    public SkeletonIK3D LeftArmIK { get; private set; }
    public SkeletonIK3D RightArmIK { get; private set; }
    public SkeletonIK3D LeftLegIK { get; private set; }
    public SkeletonIK3D RightLegIK { get; private set; }
    public Skeleton3D Skeleton3D { get; private set; }

    private AnimationPlayer animationPlayer;
    private Transform3D targetTransform;
    private Vector3 target;
    private Vector3 climbDir;
    private bool goingLeft;
    private string lastAnimation;

    public override void _Ready()
    {
        base._Ready();
        animationPlayer = (AnimationPlayer)GetNode(ModelNode + "/AnimationPlayer");
        Skeleton3D = (Skeleton3D)GetNode(ModelNode + "/Armature/Skeleton3D");

        LeftArmIK = (SkeletonIK3D)Skeleton3D.GetNode("LeftArmIK");
        LeftArmIK.Start();
        LeftArmIK.Influence = 0.0f;
        LeftArmIK.TargetNode = LeftArmIKTarget.GetPath();

        RightArmIK = (SkeletonIK3D)Skeleton3D.GetNode("RightArmIK");
        RightArmIK.Start();
        RightArmIK.Influence = 0.0f;
        RightArmIK.TargetNode = RightArmIKTarget.GetPath();
        
        LeftLegIK = (SkeletonIK3D)Skeleton3D.GetNode("LeftLegIK");
        LeftLegIK.Start();
        LeftLegIK.Influence = 0.0f;
        LeftLegIK.TargetNode = LeftLegIKTarget.GetPath();

        RightLegIK = (SkeletonIK3D)Skeleton3D.GetNode("RightLegIK");
        RightLegIK.Start();
        RightLegIK.Influence = 0.0f;
        RightLegIK.TargetNode = RightLegIKTarget.GetPath();
    }

    public void Update(CharacterData characterData)
    {
        if (!characterData.IsInsideTree())
            return;

        string currentState = characterData.GetState();

        LeftArmIK.Influence = 0.0f;
        LeftArmIK.UseMagnet = false;
        RightArmIK.Influence = 0.0f;
        RightArmIK.UseMagnet = false;
        LeftLegIK.Influence = 0.0f;
        LeftLegIK.UseMagnet = false;
        RightLegIK.Influence = 0.0f;
        RightLegIK.UseMagnet = false;

        UpdateTargetPosition(currentState, characterData.Controller);

        switch (currentState)
        {
            default:
                if (characterData.IsFatigued)
                    animationPlayer.Play(AnimationLibrary + "/fatigued");
                else if (characterData.IsDashing())
                    animationPlayer.Play(AnimationLibrary + "/dash");
                else if (characterData.Running)
                    animationPlayer.Play(AnimationLibrary + "/run");
                else
                    animationPlayer.Play(AnimationLibrary + "/idle");
                break;
            case "DEAD":
                if (lastAnimation != AnimationLibrary + "/dead")
                    animationPlayer.Play(AnimationLibrary + "/dead");
                break;
            case "AIR":
                if (characterData.IsFatigued)
                    animationPlayer.Play(AnimationLibrary + "/falling");
                else if (characterData.IsDashing())
                    animationPlayer.Play(AnimationLibrary + "/dash");
                else if (characterData.Controller.Velocity.Y >= 0.0f)
                    animationPlayer.Play(AnimationLibrary + "/jump");
                else
                    animationPlayer.Play(AnimationLibrary + "/fall");
                break;
            case "THROWN":
                if (characterData.Controller.IsOnFloor())
                {
                    if (lastAnimation != AnimationLibrary + "/dead")
                        animationPlayer.Play(AnimationLibrary + "/dead");
                }
                else if (characterData.Controller.Velocity.Length() > 100.0f)
                    animationPlayer.Play(AnimationLibrary + "/launched");
                else
                    animationPlayer.Play(AnimationLibrary + "/falling");
                break;
            case "GRABBED":
                animationPlayer.Play(AnimationLibrary + "/grabbed");
                break;
        }

        if (!string.IsNullOrEmpty(animationPlayer.CurrentAnimation) && animationPlayer.CurrentAnimation != lastAnimation)
        {
            lastAnimation = animationPlayer.CurrentAnimation;
            GD.Print(lastAnimation);
        }

        GlobalTransform = GlobalTransform.InterpolateWith(targetTransform, characterData.CameraController.ReOrientWeight);
    }

    public void UpdateTargetPosition(string currentState, CharacterController controller)
    {
        switch (currentState)
        {
            default:
                GlobalPosition = controller.Feet.GlobalPosition + controller.GlobalBasis.Y;
                targetTransform = new Transform3D(controller.GlobalBasis, controller.Feet.GlobalPosition + controller.GlobalBasis.Y);
                break;
            case "GRABBED":
            case "AIR":
                GlobalPosition = controller.GlobalPosition;
                targetTransform = new Transform3D(controller.GlobalBasis, controller.GlobalPosition);
                break;
        }
    }
    
    public void PositionHandsOnFace(SpecialFace faceOn, Vector3 climbVelocity, float climbMeterNormalizedFill)
    {
        float climbWeight = Mathf.Lerp(0.0f, 1.0f, Mathf.Sin(Mathf.Pi * Mathf.Min(climbMeterNormalizedFill * 2.0f, 1.0f)));

        if (climbVelocity.Length() > 0.0f) 
        {
            if (Mathf.Abs(faceOn.Up.Dot(climbVelocity.Normalized())) < 0.75f)
                goingLeft = faceOn.Right.Dot(climbVelocity.Normalized()) < 0.0f;
            else
            {
                if (climbMeterNormalizedFill == 0.0f)
                    goingLeft = !goingLeft;
            }
        }

        Vector3 leftHandOffset = (-faceOn.Right * 0.25f) + (faceOn.Up * 0.5f) + (goingLeft ? climbVelocity.Normalized() * climbWeight * 0.5f : Vector3.Zero);
        Vector3 rightHandOffset = (faceOn.Right * 0.25f) + (faceOn.Up * 0.5f) + (!goingLeft ? climbVelocity.Normalized() * climbWeight * 0.5f : Vector3.Zero);
        Vector3 leftFootOffset = (-faceOn.Right * 0.5f) - (faceOn.Up * 0.75f) + (goingLeft ? climbVelocity.Normalized() * climbWeight : Vector3.Zero);
        Vector3 rightFootOffset = (faceOn.Right * 0.5f) - (faceOn.Up * 0.75f) + (!goingLeft ? climbVelocity.Normalized() * climbWeight : Vector3.Zero);

        LeftArmIKTarget.GlobalPosition = faceOn.GetFacePoint(faceOn.FacePoint + leftHandOffset, faceOn.FacePoint);
        LeftArmIKTarget.LookAt(LeftArmIKTarget.GlobalPosition - faceOn.Right, faceOn.Up);
        LeftArmIK.Magnet = -faceOn.Up;

        RightArmIKTarget.GlobalPosition = faceOn.GetFacePoint(faceOn.FacePoint + rightHandOffset, faceOn.FacePoint);
        RightArmIKTarget.LookAt(RightArmIKTarget.GlobalPosition + faceOn.Right, faceOn.Up);
        RightArmIK.Magnet = -faceOn.Up;

        LeftLegIKTarget.GlobalPosition = faceOn.GetFacePoint(faceOn.FacePoint + leftFootOffset, faceOn.FacePoint);
        LeftLegIKTarget.LookAt(LeftLegIKTarget.GlobalPosition - faceOn.Right, faceOn.Up);
        LeftLegIK.Magnet = faceOn.Up - faceOn.Right;

        RightLegIKTarget.GlobalPosition = faceOn.GetFacePoint(faceOn.FacePoint + rightFootOffset, faceOn.FacePoint);
        RightLegIKTarget.LookAt(RightLegIKTarget.GlobalPosition + faceOn.Right, faceOn.Up);
        RightLegIK.Magnet = faceOn.Up + faceOn.Right;
    }
}
