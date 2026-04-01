using System.Collections.Generic;
using Godot;

public class CharacterStateAir : ICharacterState
{
    private readonly CharacterData characterData;
    private Transform3D dashTransform;
    private Vector3 lookAtDir;
    private Vector3 horizontalVelocity;
    private float verticalVelocity;
    private float coyoteTime;
    private float upDashBuffer;

    public CharacterStateAir(CharacterData characterData, Vector3 horizontalVelocity, float verticalVelocity, bool fromJump, float coyoteTime = 0.0f, bool fromChargeJump = false)
    {
        this.characterData = characterData;
        this.horizontalVelocity = horizontalVelocity;
        this.verticalVelocity = verticalVelocity;
        this.coyoteTime = coyoteTime;
        lookAtDir = characterData.Controller.GetLookAtDir(Vector3.Up, Vector3.Forward);

        GD.Print("ENTERING " + (fromJump || fromChargeJump ? "JUMP" : "AIR") + " STATE");

        if (characterData.GetState() == "CLIMB" || characterData.GetState() == "HANG")
            characterData.Controller.HorizontalReOrient(characterData.CameraController, Vector3.Up, true);
        
        if (characterData.IsDashing())
            dashTransform = characterData.Controller.GlobalTransform;

        characterData.Controller.EnteringNewState(GetState());
    }

    public ICharacterState ChangeState()
    {
        if (characterData.CanControl() && characterData.Controller.GroundedJumpInput() && coyoteTime > 0.0f)
        {
            return new CharacterStateAir(characterData, horizontalVelocity, characterData.JumpSpeed, true);
        }
        else if (verticalVelocity <= 0.0f && characterData.CanGrip() && characterData.Controller.GripInput() && characterData.Controller.DetectedClimbableWall())
        {
            return new CharacterStateClimb(characterData, null);
        }
        else if (verticalVelocity <= 0.0f && characterData.Controller.IsOnFloor())
        {
            return new CharacterStateGrounded(characterData, horizontalVelocity);
        }

        return null;
    }

    public void Move(Vector2 moveDirection)
    {
        Vector3 velocity;

        if (characterData.CanControl())
        {
            horizontalVelocity = characterData.Controller.CalculateMoveVelocity(moveDirection, Vector3.Up, characterData.CameraController.CameraUpRotation.GlobalBasis.Z, characterData.Speed);
            velocity = horizontalVelocity + (verticalVelocity * Vector3.Up);
        }
        else if (characterData.IsStunned())
            velocity = verticalVelocity * Vector3.Up;
        else if (characterData.IsDashing())
            velocity = characterData.DashSpeed * dashTransform.Basis.Z * Mathf.Min(characterData.DashMeter.NormalizedFill() * 2.0f, 1.0f);
        else
            velocity = horizontalVelocity + (verticalVelocity * Vector3.Up);

        characterData.Controller.Velocity = velocity;
        characterData.Controller.MoveAndSlide();

        if (characterData.IsDashing())
            lookAtDir = dashTransform.Basis.Z;
        else if (moveDirection != Vector2.Zero)
            lookAtDir = characterData.Controller.GetMoveLookAtDirection(moveDirection, Vector3.Up, characterData.CameraController.CameraUpRotation.GlobalBasis.Z);
        else
            lookAtDir = characterData.Controller.GetLookAtDir(Vector3.Up, Vector3.Forward);
    }

    public void Update(float delta)
    {
        if (lookAtDir != Vector3.Zero)
        {
            if (characterData.IsDashing())
                characterData.Controller.LookAt(characterData.Controller.GlobalPosition + lookAtDir, dashTransform.Basis.Y);
            else 
            {
                if (lookAtDir == Vector3.Up)
                    characterData.Controller.LookAt(characterData.Controller.GlobalPosition - characterData.CameraController.CameraUpRotation.GlobalBasis.Z, characterData.CameraController.CameraUpRotation.GlobalBasis.Y);
                else 
                    characterData.Controller.LookAt(characterData.Controller.GlobalPosition + lookAtDir, Vector3.Up);   
            }
        }

        upDashBuffer -= delta;
        upDashBuffer = Mathf.Max(upDashBuffer, 0.0f);
        
        if (characterData.CanDash() && characterData.Controller.DashInput())
        {
            Vector3 dashDir;
            dashTransform = characterData.CameraController.CameraRightRotation.GlobalTransform;

            if (horizontalVelocity != Vector3.Zero) 
            {
                Transform3D flatCameraTransform = characterData.CameraController.CameraUpRotation.GlobalTransform;
                float angleToHorizontalVel = flatCameraTransform.Basis.Z.SignedAngleTo(-horizontalVelocity.Normalized(), flatCameraTransform.Basis.Y.Normalized());
                dashDir = dashTransform.Basis.Z.Rotated(dashTransform.Basis.Y.Normalized(), angleToHorizontalVel);
            }
            else 
                dashDir = dashTransform.Basis.Z;

            dashTransform = dashTransform.LookingAt(dashTransform.Origin + dashDir, dashTransform.Basis.Y);
            horizontalVelocity = Vector3.Zero;
            verticalVelocity = 0.0f;
            upDashBuffer = 0.0f;
            characterData.Dash(dashTransform.Basis.Z);
        }
        else if (characterData.CanDash() && characterData.Controller.GroundedJumpInput())
        {
            if (upDashBuffer > 0.0f)
            {
                dashTransform = characterData.CameraController.CameraUpRotation.GlobalTransform;
                dashTransform = dashTransform.LookingAt(dashTransform.Origin - Vector3.Up, dashTransform.Basis.Z);
                horizontalVelocity = Vector3.Zero;
                verticalVelocity = 0.0f;
                upDashBuffer = 0.0f;
                characterData.Dash(dashTransform.Basis.Z);
            }
            else
            {
                upDashBuffer = 0.5f;
            }
        }
        else if (characterData.CanSwingSword())
        {
            if (characterData.Controller.ChargeSwordInput())
            {
                characterData.Controller.LookAt(characterData.Controller.GlobalPosition - characterData.CameraController.CameraUpRotation.GlobalBasis.Z, Vector3.Up);
                characterData.Controller.Sword.Charge(delta);
                horizontalVelocity = Vector3.Zero;
                upDashBuffer = 0.0f;
            }
            else
            {
                characterData.Controller.Sword.Swing();
            }
        }
        
        if (!characterData.IsDashing())
            verticalVelocity += CharacterController.GRAVITY * delta;

        coyoteTime -= delta;
        coyoteTime = Mathf.Max(coyoteTime, 0.0f);
    }

    public bool OnThisEntity(ClimbableEntity entity)
    {
        return false;
    }

    public HashSet<string> GetBoneImOnFromClimbable()
    {
        return null;
    }

    public void EndOfFrame()
    {
        
    }

    public string GetState()
    {
        return "AIR";
    }

    public void UpdatePositionAfterPoseUpdate()
    {
        return;
    }
}