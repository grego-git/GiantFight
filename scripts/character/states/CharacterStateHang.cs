using System.Collections.Generic;
using Godot;

public class CharacterStateHang : ICharacterState
{
    private const float Y_OFFSET = 1.5f;
    private readonly CharacterData characterData;
    private Vector3 dashDir;
    private HangFace faceOn;
    private Vector3 lookAtDir;
    private Vector3 climbVelocity;

    public CharacterStateHang(CharacterData characterData, SpecialFace attachedFace = null)
    {
        this.characterData = characterData;
        lookAtDir = characterData.Controller.GetLookAtDir(-Vector3.Up, Vector3.Forward);

        GD.Print("ENTERING HANG STATE");
        
        if (attachedFace != null && attachedFace.GetType() == typeof(ClimbFace) && ((ClimbFace)attachedFace).ChangeToHang)
        {
            faceOn = HangFace.CreateHangFaceFromClimbFace((ClimbFace)attachedFace, characterData.CameraController);
            lookAtDir = characterData.Controller.GetLookAtDir(faceOn.FaceNormal, Vector3.Up);
            
            if (characterData.IsDashing())
                dashDir = characterData.Controller.Velocity.Normalized();
        }
        else
        {
            faceOn = HangFace.CreateHangFaceFromRayCast(characterData.Controller.HangRayCast, characterData.CameraController);

            if (characterData.IsDashing())
            {
                switch (characterData.GetState())
                {
                    case "CLIMB":
                    case "CRAWL":
                    case "HANG":
                        dashDir = characterData.Controller.Velocity.Normalized();
                        break;
                    default:
                    characterData.DashMeter.Empty();
                        break;
                }
            }
        }

        if (characterData.GetState() == "CLIMB")
            characterData.Controller.HangHorizontalReOrient(characterData.CameraController, -Vector3.Up, true);

        characterData.Controller.EnteringNewState(GetState());

        faceOn.ClimbableEntity.CharacterData = characterData;
    }

    public ICharacterState ChangeState()
    {
        faceOn.Update();

        bool validHangFace = HangFace.ValidHangFaceNormal(faceOn.FaceNormal);

        if (!faceOn.ChangeToClimb && !validHangFace)
        {
            if (ClimbFace.ValidClimbFaceNormal(faceOn.FaceNormal))
            {
                faceOn.ChangeToClimb = true;
            }
        }

        if (characterData.CanControl() && characterData.Controller.GroundedJumpInput())
        {
            Vector3 horizontalVelocity = Utils.GetFlatDirectionalVector(climbVelocity) * characterData.ClimbSpeed;
            return new CharacterStateAir(characterData, horizontalVelocity, -characterData.JumpSpeed, false, fromChargeJump: true);
        }
        else if (!faceOn.ClimbableEntity.Destroyed && faceOn.ChangeToClimb)
        {
            return new CharacterStateClimb(characterData, faceOn);
        }
        else if (faceOn.ClimbableEntity.Destroyed || characterData.HasLostGrip() || (!characterData.IsStunned() && !characterData.Controller.GripInput()) || !validHangFace)
        {
            return new CharacterStateAir(characterData, Vector3.Zero, 0.0f, false);
        }
        else if (!characterData.IsStunned() && characterData.Controller.DetectedClimbableWall())
        {
            return new CharacterStateClimb(characterData);
        }
        else if (!characterData.IsStunned() && characterData.Controller.DetectedHangableCeiling())
        {
            return new CharacterStateHang(characterData);
        }

        return null;
    }

    public void Move(Vector2 moveDirection)
    {
        Vector3 oldPosition = characterData.Controller.GlobalPosition;
        Vector3 velocity;

        if (characterData.CanControl())
        {
            climbVelocity = characterData.Controller.CalculateMoveVelocity(moveDirection, faceOn.FaceNormal, characterData.CameraController.CameraUpRotation.GlobalBasis.Z, characterData.ClimbSpeed);
            velocity = climbVelocity;
        }
        else if (characterData.IsStunned())
        {
            climbVelocity = Vector3.Zero;
            velocity = Vector3.Zero;
        }
        else if (characterData.IsDashing())
            velocity = characterData.DashSpeed * Utils.ProjectOntoPlane(dashDir, faceOn.FaceNormal).Normalized() * Mathf.Min(characterData.DashMeter.NormalizedFill() * 2.0f, 1.0f);
        else 
            velocity = climbVelocity;

        characterData.Running = climbVelocity.Length() > 1.0f;

        characterData.Controller.Velocity = velocity;
        characterData.Controller.MoveAndSlideOnFace(oldPosition, faceOn);

        if (!faceOn.ChangeToClimb)
        {
            if (climbVelocity != Vector3.Zero)
                lookAtDir = climbVelocity.Normalized();
            else if (!characterData.IsStunned() && characterData.IsDashing())
                lookAtDir = characterData.Controller.Velocity.Normalized();
            else
            {
                Vector3 right = lookAtDir.Cross(faceOn.FaceNormal);
                Vector3 forward = faceOn.FaceNormal.Cross(right);    

                if (forward != Vector3.Zero)
                    lookAtDir = forward; 
            }
            
            if (lookAtDir != Vector3.Zero)
                characterData.Controller.PositionCharacterToFace(faceOn.FacePoint, faceOn.FaceNormal, lookAtDir, faceOn.FaceNormal);
        }
    }

    public void Update(float delta)
    {
        if (lookAtDir != Vector3.Zero)
            characterData.Controller.PositionCharacterToFace(faceOn.FacePoint, faceOn.FaceNormal, lookAtDir, faceOn.FaceNormal);
        
        if (characterData.CanDash() && characterData.Controller.DashInput())
        {
            dashDir = climbVelocity != Vector3.Zero ? climbVelocity.Normalized() : -characterData.CameraController.CameraUpRotation.GlobalBasis.Z;
            climbVelocity = Vector3.Zero;
            characterData.Dash(dashDir);
        }
        else if (characterData.CanSwingSword())
        {
            if (characterData.Controller.ChargeSwordInput())
            {
                characterData.Controller.LookAt(characterData.Controller.GlobalPosition - characterData.CameraController.CameraUpRotation.GlobalBasis.Z, Vector3.Up);
                characterData.Controller.Sword.Charge(delta);
                climbVelocity = Vector3.Zero;
            }
            else
            {
                characterData.Controller.Sword.Swing();
            }
        }
    }

    public bool OnThisEntity(ClimbableEntity entity)
    {
        if (faceOn == null)
            return false;
        
        return faceOn.ClimbableEntity == entity;
    }

    public HashSet<string> GetBoneImOnFromClimbable()
    {
        if (faceOn == null)
            return null;

        return faceOn.ClimbableEntity.GetFaceBones(faceOn.FaceID);
    }

    public void EndOfFrame()
    {
        
    }

    public string GetState()
    {
        return "HANG";
    }

    public void UpdatePositionAfterPoseUpdate()
    {
        faceOn.Update();
        
        if (lookAtDir != Vector3.Zero)
            characterData.Controller.PositionCharacterToFace(faceOn.FacePoint, faceOn.FaceNormal, lookAtDir, faceOn.FaceNormal);
        
        characterData.UpdateAfterGiantsPoseUpdate();
    }
}