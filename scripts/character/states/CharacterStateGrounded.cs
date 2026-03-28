using System;
using System.Collections.Generic;
using Godot;

public class CharacterStateGrounded : ICharacterState
{
    private readonly CharacterData characterData;
    private GroundedFace faceOn;
    private Vector3 dashDir;
    private Vector3 groundNormal;
    private Vector3 lookAtDir;
    private Vector3 horizontalVelocity;
    private float verticalVelocity;

    public CharacterStateGrounded(CharacterData characterData, Vector3 horizontalVelocity)
    {
        this.characterData = characterData;
        this.horizontalVelocity = horizontalVelocity;
        groundNormal = Vector3.Up;
        lookAtDir = characterData.Controller.GetLookAtDir(Vector3.Up, Vector3.Forward);
        
        GD.Print("ENTERING GROUND STATE");

        if (characterData.GetState() == "CLIMB")
            characterData.Controller.HorizontalReOrient(characterData.CameraController, Vector3.Up, true);
        
        if (characterData.IsDashing())
            dashDir = Utils.GetFlatDirectionalVector(characterData.Controller.Velocity);

        characterData.Controller.EnteringNewState(GetState());
    }

    public ICharacterState ChangeState()
    {
        if (characterData.CanControl() && characterData.Controller.GroundedJumpInput())
        {
            return new CharacterStateAir(characterData, horizontalVelocity, characterData.JumpSpeed, true);
        }
        else if (characterData.CanGrip() && characterData.Controller.GripInput() && characterData.Controller.DetectedCrawlableGround())
        {
            return new CharacterStateCrawl(characterData, null);
        }
        else if (characterData.CanGrip() && characterData.Controller.GripInput() && characterData.Controller.DetectedClimbableWall())
        {
            return new CharacterStateClimb(characterData, null);
        }
        else if (!characterData.Controller.IsOnFloor() && !characterData.Controller.AboveValidGround() && faceOn == null)
        {
            return new CharacterStateAir(characterData, horizontalVelocity, verticalVelocity, false, 0.1f);
        }

        return null;
    }

    public void Move(Vector2 moveDirection)
    {
        Vector3 velocity;

        if (characterData.CanControl())
        {
            horizontalVelocity = characterData.Controller.CalculateMoveVelocity(moveDirection, groundNormal, characterData.CameraController.CameraUpRotation.GlobalBasis.Z, characterData.Speed);
            velocity = horizontalVelocity + (verticalVelocity * Vector3.Up);
        }
        else if (characterData.IsStunned())
            velocity = verticalVelocity * Vector3.Up;
        else if (characterData.IsDashing())
        {
            if (faceOn != null)
                velocity = characterData.DashSpeed * Utils.ProjectOntoPlane(dashDir, faceOn.FaceNormal).Normalized() * Mathf.Min(characterData.DashMeter.NormalizedFill() * 2.0f, 1.0f);
            else
                velocity = characterData.DashSpeed * dashDir * Mathf.Min(characterData.DashMeter.NormalizedFill() * 2.0f, 1.0f);
        }
        else
            velocity = horizontalVelocity + (verticalVelocity * Vector3.Up);

        characterData.Running = horizontalVelocity.Length() > 1.0f;

        characterData.Controller.Velocity = velocity;
        characterData.Controller.MoveAndSlide();

        faceOn = characterData.Controller.GetFaceWhenGrounded(faceOn);

        if (faceOn != null)
        {
            if (faceOn.Climbable != null)
            {
                faceOn.Climbable.CharacterData = characterData;
            }
        }

        if (characterData.IsDashing())
            lookAtDir = dashDir;
        else if (moveDirection != Vector2.Zero)
            lookAtDir = characterData.Controller.GetMoveLookAtDirection(moveDirection, Vector3.Up, characterData.CameraController.CameraUpRotation.GlobalBasis.Z);
        else
            lookAtDir = characterData.Controller.GetLookAtDir(Vector3.Up, Vector3.Forward);
    }

    public void Update(float delta)
    {
        if (lookAtDir != Vector3.Zero)
            characterData.Controller.LookAt(characterData.Controller.GlobalPosition + lookAtDir, Vector3.Up);
        
        if (characterData.CanDash() && characterData.Controller.DashInput())
        {
            dashDir = horizontalVelocity != Vector3.Zero ? horizontalVelocity.Normalized() : -characterData.CameraController.CameraUpRotation.GlobalBasis.Z;
            horizontalVelocity = Vector3.Zero;
            verticalVelocity = 0.0f;
            characterData.Dash();
        }
        else if (characterData.CanSwingSword())
        {
            if (characterData.Controller.ChargeSwordInput())
            {
                characterData.Controller.LookAt(characterData.Controller.GlobalPosition - characterData.CameraController.CameraUpRotation.GlobalBasis.Z, Vector3.Up);
                characterData.Controller.Sword.Charge(delta);
                horizontalVelocity = Vector3.Zero;
            }
            else
            {
                characterData.Controller.Sword.Swing();
            }
        }

        faceOn = characterData.Controller.UpdateAndValidateGroundedFace(faceOn);

        if (!characterData.Controller.IsOnFloor())
        {
            characterData.Controller.ReparentToInitParent();
            groundNormal = Vector3.Up;

            verticalVelocity += CharacterController.GRAVITY * delta;
        }
        else if (faceOn != null)
        {
            characterData.Controller.ReparentToInitParent();
            groundNormal = faceOn.FaceNormal;

            verticalVelocity = 0.0f;
        }
        else if (characterData.Controller.AboveValidGround())
        {
            characterData.Controller.ReparentToNode((Node3D)characterData.Controller.GroundRayCast.GetCollider());
            groundNormal = characterData.Controller.GroundRayCast.GetCollisionNormal();

            verticalVelocity = 0.0f;
        }
        else
        {
            characterData.Controller.ReparentToInitParent();
            groundNormal = Vector3.Up;

            verticalVelocity = 0.0f;
        }
    }

    public bool OnThisEntity(ClimbableEntity entity)
    {
        if (faceOn == null)
            return false;

        if (faceOn.Climbable == null)
            return false;

        return faceOn.Climbable == entity;
    }

    public HashSet<string> GetBoneImOnFromClimbable()
    {
        if (faceOn == null)
            return null;

        return faceOn.Climbable.GetFaceBones(faceOn.FaceID);
    }

    public void EndOfFrame()
    {
        
    }

    public string GetState()
    {
        return "GROUND";
    }

    public void UpdatePositionAfterPoseUpdate()
    {
        if (faceOn != null) 
        {
            Vector3 oldFacePoint = faceOn.FacePoint;
            faceOn.Update();
            Vector3 displacement = faceOn.FacePoint - oldFacePoint;
            characterData.Controller.GlobalPosition += displacement;

            Vector3 oldVelocity = characterData.Controller.Velocity;
            characterData.Controller.Velocity = Vector3.Zero;
            characterData.Controller.MoveAndSlide();
            characterData.Controller.Velocity = oldVelocity;
            
            if (displacement.Length() >= 1.0f) 
            {
                GD.Print("DISPLACEMENT LENGTH: " + displacement.Length());
                GD.Print("DISPLACEMENT DIR: " + displacement.Normalized());
                GD.Print("DISPLACEMENT: " + displacement);
            }
        }
    }
}