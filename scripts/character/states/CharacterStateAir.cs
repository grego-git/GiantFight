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

        if (characterData.CanControl() && verticalVelocity <= 0.0f)
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
                characterData.Controller.LookAt(characterData.Controller.GlobalPosition + lookAtDir, Vector3.Up);
        }
        
        if (characterData.CanDash() && characterData.Controller.DashInput())
        {
            dashTransform = characterData.CameraController.CameraRightRotation.GlobalTransform;
            dashTransform = dashTransform.LookingAt(dashTransform.Origin + dashTransform.Basis.Z, dashTransform.Basis.Y);
            horizontalVelocity = Vector3.Zero;
            verticalVelocity = 0.0f;
            characterData.Dash();
        }
        else if (characterData.CanSwingSword() && characterData.Controller.SwingSwordInput())
        {
            characterData.Controller.LookAt(characterData.Controller.GlobalPosition - characterData.CameraController.CameraUpRotation.GlobalBasis.Z, Vector3.Up);
            horizontalVelocity = Vector3.Zero;
            characterData.Controller.Sword.Swing();
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