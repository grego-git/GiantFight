using System.Collections.Generic;
using Godot;

public partial class CharacterController : CharacterBody3D
{
    public static readonly float GRAVITY = -9.81f;

    [Signal]
    public delegate void HitEventHandler();

    [Export]
    public Node3D Feet { get; set; }

    public Sword Sword { get; private set; }
    public RayCast3D GroundRayCast { get; private set; }
    public RayCast3D ClimbRayCast { get; private set; }
    public RayCast3D CrawlRayCast { get; private set; }
    public RayCast3D HangRayCast { get; private set; }
    public LimbChecker LimbChecker { get; private set; }

    private Node3D initParent;
    private RandomNumberGenerator rng;

    public override void _Ready()
    {
        Sword = (Sword)GetNode("Sword");
        GroundRayCast = (RayCast3D)GetNode("GroundRayCast");
        ClimbRayCast = (RayCast3D)GetNode("ClimbRayCast");
        CrawlRayCast = (RayCast3D)GetNode("CrawlRayCast");
        HangRayCast = (RayCast3D)GetNode("HangRayCast");
        LimbChecker = (LimbChecker)GetNode("LimbChecker");

        initParent = (Node3D)GetParent();

        rng = new RandomNumberGenerator();
    }

    public void Update(float delta, ICharacterState currentState)
    {
        Vector2 moveDirection = Input.GetVector("left", "right", "down", "up");
        Feet.GlobalPosition = GlobalPosition - GlobalBasis.Y;

        currentState.Update(delta);
        currentState.Move(moveDirection);
    }

    public void EnteringNewState(string state)
    {
        switch (state)
        {
            default:
                if (ClimbRayCast.IsInsideTree())
                {
                    ClimbRayCast.GlobalPosition = GlobalPosition + (Vector3.Up * 0.5f);
                    ClimbRayCast.TargetPosition = Vector3.Forward * 3.0f;
                }

                if (CrawlRayCast.IsInsideTree())
                {
                    CrawlRayCast.GlobalPosition = GlobalPosition;
                    CrawlRayCast.TargetPosition = Vector3.Down * 2.0f;
                }

                if (HangRayCast.IsInsideTree())
                {
                    HangRayCast.GlobalPosition = GlobalPosition;
                    HangRayCast.TargetPosition = Vector3.Up * 2.0f;
                }
                break;
            case "CLIMB":
            case "CRAWL":
            case "HANG":
                if (ClimbRayCast.IsInsideTree())
                {
                    ClimbRayCast.GlobalPosition = GlobalPosition + (-GlobalBasis.Y.Normalized() * 0.5f);
                    ClimbRayCast.TargetPosition = Vector3.Forward * 2.0f;
                }

                if (CrawlRayCast.IsInsideTree())
                {
                    CrawlRayCast.GlobalPosition = GlobalPosition + (-GlobalBasis.Y.Normalized() * 0.5f);
                    CrawlRayCast.TargetPosition = Vector3.Forward * 2.0f;
                }

                if (HangRayCast.IsInsideTree())
                {
                    HangRayCast.GlobalPosition = GlobalPosition;
                    HangRayCast.TargetPosition = Vector3.Forward * 2.0f;
                }
                break;
        }

        ReparentToInitParent();
    }

    public bool CheckIfOnGround()
    {
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            if (GetSlideCollision(i).GetCollider().GetType() == typeof(StaticBody3D))
            {
                StaticBody3D body = (StaticBody3D)GetSlideCollision(i).GetCollider();

                if (body.GetCollisionLayerValue((int)Constants.COLLIDER_LAYERS.GROUND))
                    return true;
            }
        }

        return false;
    }

    public bool AboveValidGround()
    {
        return GroundRayCast.IsColliding() && GroundRayCast.GetCollisionNormal().Dot(Vector3.Up) >= 0.25f;
    }

    public bool DebugInput()
    {
        return Input.IsActionJustPressed("debug");
    }

    public bool CallBallInput()
    {
        return Input.IsActionJustPressed("call");
    }

    public bool GroundedJumpInput()
    {
        return Input.IsActionJustPressed("ui_accept");
    }

    public bool GripInput()
    {
        return Input.IsActionPressed("right_click");
    }

    public bool AimingInput()
    {
        return Input.IsActionPressed("right_click");
    }

    public bool ChargeSwordInput()
    {
        return Input.IsActionPressed("click");
    }

    public bool ChargeSwordInputRelease()
    {
        return !Input.IsActionPressed("click");
    }

    public bool RelieveFatigueInput()
    {
        return Input.IsActionJustPressed("click") || Input.IsActionJustPressed("ui_accept");
    }

    public bool DashInput()
    {
        return Input.IsActionJustPressed("dash");
    }

    public bool DetectedClimbableWall()
    {
        ClimbRayCast.ForceRaycastUpdate();

        if (ClimbRayCast.IsColliding() &&
            ((CollisionObject3D)ClimbRayCast.GetCollider()).GetCollisionLayerValue((int)Constants.COLLIDER_LAYERS.DYNAMIC_COLLIDER_CLIMBABLE) &&
            ClimbFace.ValidClimbFaceNormal(ClimbRayCast.GetCollisionNormal()))
            return true;

        return false;
    }

    public bool DetectedCrawlableGround()
    {
        CrawlRayCast.ForceRaycastUpdate();

        if (CrawlRayCast.IsColliding() &&
            ((CollisionObject3D)CrawlRayCast.GetCollider()).GetCollisionLayerValue((int)Constants.COLLIDER_LAYERS.DYNAMIC_COLLIDER_CLIMBABLE) &&
            CrawlFace.ValidCrawlFaceNormal(CrawlRayCast.GetCollisionNormal()))
            return true;

        return false;
    }

    public bool DetectedHangableCeiling()
    {
        HangRayCast.ForceRaycastUpdate();

        if (HangRayCast.IsColliding() &&
            ((CollisionObject3D)HangRayCast.GetCollider()).GetCollisionLayerValue((int)Constants.COLLIDER_LAYERS.DYNAMIC_COLLIDER_CLIMBABLE) &&
            HangFace.ValidHangFaceNormal(HangRayCast.GetCollisionNormal()))
            return true;

        return false;
    }
    
    public bool SurfaceDetectedOnDifferentBone(ClimbableEntity climbableEntity, RayCast3D rayCast3D, HashSet<string> bonesOn)
    {
        if (climbableEntity != rayCast3D.GetCollider())
            return true;

        if (bonesOn == null || bonesOn.Count == 0)
            return true;

        int newFaceId = climbableEntity.GetActualCollisionFaceID(rayCast3D.GetCollisionFaceIndex());
        HashSet<string> bonesForNewFace = climbableEntity.GetFaceBones(newFaceId);

        if (bonesForNewFace == null || bonesForNewFace.Count == 0)
            return true;

        foreach (var bone in bonesOn)
        {
            if (bonesForNewFace.Contains(bone))
                return false;
        }

        return true;
    }

    public Vector3 GetMoveLookAtDirection(Vector2 moveDirection, Vector3 upDir, Vector3 forwardDir)
    {
        return CalculateMoveVelocity(moveDirection, upDir, forwardDir, 1.0f);
    }

    public Vector3 GetAimLookAtDirection(Vector3 aimPoint)
    {
        Vector3 flatAimPoint = aimPoint;
        flatAimPoint.Y = GlobalPosition.Y;

        return flatAimPoint - GlobalPosition;
    }

    public void ReparentToInitParent()
    {
        if ((Node3D)GetParent() != initParent)
            Reparent(initParent, true);
    }

    public void ReparentToNode(Node3D newParent)
    {
        if ((Node3D)GetParent() != newParent)
            Reparent(newParent, true);
    }

    public Vector3 CalculateMoveVelocity(Vector2 moveDirection, Vector3 upDir, Vector3 forwardDir, float speed)
    {
        Vector3 up = upDir;
        Vector3 right = up.Cross(forwardDir);
        Vector3 forward = up.Cross(right);

        return ((forward * moveDirection.Y) + (right * moveDirection.X)).Normalized() * speed;
    }

    public Vector3 CalculateFaceVelocity(Vector2 moveDirection, Vector3 upDir, Vector3 rightDir, float speed)
    {
        return ((upDir * moveDirection.Y) + (rightDir * moveDirection.X)).Normalized() * speed;
    }

    public void MoveAndSlideOnFace(Vector3 oldPosition, SpecialFace faceOn)
    {
        MoveAndSlide();

        Vector3 actualTranslation = GlobalPosition - oldPosition;
        Vector3 projectedTranslation = Utils.ProjectOntoPlane(actualTranslation, faceOn.FaceNormal);
        faceOn.UpdateFacePoint(faceOn.FacePoint + projectedTranslation);
    }

    public void PositionCharacterToFace(Vector3 point, Vector3 normal, Vector3 forward, Vector3 up, float offsetFromFace = 1.0f)
    {
        GlobalPosition = point;
        GlobalPosition += normal * offsetFromFace;
        LookAt(GlobalPosition + forward, up);

        Feet.GlobalPosition = point;
    }

    public GroundedFace GetFaceWhenGrounded(GroundedFace currentFace)
    {
        GroundedFace face = null;
        GroundRayCast.ForceRaycastUpdate();

        if (AboveValidGround() &&
            GroundRayCast.GetCollisionFaceIndex() > -1)
        {
            Vector3 groundPoint = GroundRayCast.GetCollisionPoint();

            if (GroundRayCast.GetCollider().GetType() == typeof(ClimbableAnimatedEntity))
            {
                ClimbableAnimatedEntity climbable = (ClimbableAnimatedEntity)GroundRayCast.GetCollider();
                int faceId = climbable.GetActualCollisionFaceID(GroundRayCast.GetCollisionFaceIndex());

                if (faceId >= 0)
                {
                    if (currentFace != null && currentFace.FaceID == faceId)
                    {
                        GD.Print(string.Format("KEEP FACE ({0})", faceId));
                        currentFace.UpdateFacePoint(groundPoint);
                        face = currentFace;
                    }
                    else
                    {
                        GD.Print(string.Format("CHANGE FACE ({0}) -> ({1})", currentFace?.FaceID ?? -1, faceId));
                        face = new GroundedFace(climbable, groundPoint, faceId);
                    }
                }
            }
        }

        return face;
    }

    public GroundedFace UpdateAndValidateGroundedFace(GroundedFace faceOn)
    {
        if (faceOn == null)
            return null;

        faceOn.Update();

        if (faceOn.FaceNormal.Dot(Vector3.Up) < 0.25f)
        {
            GD.Print("NO FACE: STEEP");
            return null;
        }

        return faceOn;
    }

    public Vector3 GetLookAtDir(Vector3 up, Vector3 fallBack)
    {
        Vector3 projectedVel = Utils.ProjectOntoPlane(Velocity, up).Normalized();

        if (projectedVel != Vector3.Zero)
            return projectedVel;

        Vector3 projectedForward = Utils.ProjectOntoPlane(-GlobalBasis.Z, up).Normalized();

        if (projectedForward != Vector3.Zero)
            return projectedForward;

        return fallBack;
    }


    public void HorizontalReOrient(CameraController cameraController, Vector3 up, bool readjustYRot)
    {
        cameraController.ReOrient(up, -Vector3.Forward, readjustYRot);
    }

    public void HangHorizontalReOrient(CameraController cameraController, Vector3 up, bool readjustYRot)
    {
        cameraController.ReOrient(up, -Vector3.Forward, readjustYRot);
    }

    public void VerticalOrient(CameraController cameraController, Vector3 up, Vector3 forward, bool readjustYRot)
    {
        Vector3 newUp = Utils.ProjectOntoPlane(up, Vector3.Up).Normalized();
        cameraController.ReOrient(newUp, forward, readjustYRot);
    }
}
