using System.Collections.Generic;
using Godot;

public class CharacterStateClimb : ICharacterState
{
    private const float Y_OFFSET = 0.0f;
    private readonly CharacterData characterData;
    private Vector3 dashDir;
    private ClimbFace faceOn;
    private Vector3 climbVelocity;
    private Vector3 lookAtDir;
    private Vector3 refDir;
    private bool fromClimb;

    public CharacterStateClimb(CharacterData characterData, SpecialFace attachedFace = null)
    {
        this.characterData = characterData;

        GD.Print("ENTERING CLIMB STATE");

        if ((attachedFace != null && attachedFace.GetType() == typeof(CrawlFace) && ((CrawlFace)attachedFace).ChangeToClimb) ||
            (attachedFace != null && attachedFace.GetType() == typeof(HangFace) && ((HangFace)attachedFace).ChangeToClimb))
        {
            faceOn = ClimbFace.CreateClimbFaceFromCrawlOrHangFace(attachedFace);
            lookAtDir = characterData.Controller.GetLookAtDir(faceOn.FaceNormal, Vector3.Up);
            
            if (characterData.IsDashing())
                dashDir = characterData.Controller.Velocity.Normalized();
        }
        else
        {
            faceOn = ClimbFace.CreateClimbFaceFromRayCast(characterData.Controller.ClimbRayCast);
            lookAtDir = Vector3.Up;

            characterData.DashMeter.Empty();
        }
        
        refDir = -Vector3.Up;
        
        if (characterData.GetState() != "CLIMB")
            characterData.Controller.VerticalOrient(characterData.CameraController, faceOn.FaceNormal, refDir, true);
        else
            fromClimb = true;

        GD.Print("FACE: " + faceOn.FaceID);

        characterData.Controller.EnteringNewState(GetState());

        faceOn.ClimbableEntity.CharacterData = characterData;
    }

    public ICharacterState ChangeState()
    {
        faceOn.Update();

        bool validClimbFace = ClimbFace.ValidClimbFaceNormal(faceOn.FaceNormal);

        if (!faceOn.ChangeToCrawl && !faceOn.ChangeToHang && !validClimbFace)
        {
            if (CrawlFace.ValidCrawlFaceNormal(faceOn.FaceNormal))
            {
                faceOn.ChangeToCrawl = true;
            }
            else if (HangFace.ValidHangFaceNormal(faceOn.FaceNormal))
            {
                faceOn.ChangeToHang = true;
            }
        }

        if (characterData.CanControl() && characterData.Controller.GroundedJumpInput())
        {
            Vector3 horizontalVelocity = Utils.GetFlatDirectionalVector(characterData.CameraController.CameraOrientation.GlobalBasis.Y) * characterData.JumpSpeed;
            return new CharacterStateAir(characterData, horizontalVelocity, characterData.ClimbSpeed, false, fromChargeJump: true);
        }
        else if (faceOn.ChangeToCrawl)
        {
            return new CharacterStateCrawl(characterData, faceOn);
        }
        else if (faceOn.ChangeToHang)
        {
            return new CharacterStateHang(characterData, faceOn);
        }
        else if (characterData.HasLostGrip() || !characterData.Controller.GripInput() || !validClimbFace)
        {
            return new CharacterStateAir(characterData, Vector3.Zero, 0.0f, false);
        }
        else if (characterData.Controller.DetectedCrawlableGround())
        {
            return new CharacterStateCrawl(characterData);
        }
        else if (!fromClimb && characterData.Controller.DetectedClimbableWall())
        {
            return new CharacterStateClimb(characterData);
        }
        else if (characterData.Controller.DetectedHangableCeiling())
        {
            return new CharacterStateHang(characterData);
        }

        GD.Print("NO NEXT STATE");
        fromClimb = false;

        return null;
    }

    public void Move(Vector2 moveDirection)
    {
        Vector3 oldPosition = characterData.Controller.GlobalPosition;
        int oldFace = faceOn.FaceID;
        Vector3 velocity;

        if (characterData.CanControl())
        {
            climbVelocity = characterData.Controller.CalculateMoveVelocity(moveDirection, faceOn.FaceNormal, characterData.CameraController.CameraUpRotation.GlobalBasis.Z, characterData.ClimbSpeed);
            velocity = climbVelocity;
        }
        else if (characterData.IsDashing())
            velocity = characterData.DashSpeed * Utils.ProjectOntoPlane(dashDir, faceOn.FaceNormal).Normalized() * Mathf.Min(characterData.DashMeter.NormalizedFill() * 2.0f, 1.0f);
        else 
            velocity = climbVelocity;
        
        characterData.Running = climbVelocity.Length() > 1.0f;

        characterData.Controller.Velocity = velocity;
        characterData.Controller.MoveAndSlideOnFace(oldPosition, faceOn);

        if (!faceOn.ChangeToCrawl && !faceOn.ChangeToHang)
        {
            if (climbVelocity != Vector3.Zero)
                lookAtDir = climbVelocity.Normalized();
            else if (characterData.IsDashing())
                lookAtDir = characterData.Controller.Velocity.Normalized();
            
            characterData.Controller.PositionCharacterToFace(faceOn.FacePoint, faceOn.FaceNormal, lookAtDir, faceOn.FaceNormal);
            
            if (oldFace != faceOn.FaceID)
            {
                GD.Print("FACE: " + faceOn.FaceID);
                characterData.Controller.VerticalOrient(characterData.CameraController, faceOn.FaceNormal, refDir, false);
            }
        } 
    }

    public void Update(float delta)
    {
        characterData.Controller.PositionCharacterToFace(faceOn.FacePoint, faceOn.FaceNormal, lookAtDir, faceOn.FaceNormal);
        
        if (characterData.CanDash() && characterData.Controller.DashInput())
        {
            dashDir = climbVelocity != Vector3.Zero ? climbVelocity.Normalized() : -characterData.CameraController.CameraUpRotation.GlobalBasis.Z;
            climbVelocity = Vector3.Zero;
            characterData.Dash(dashDir);
        }
        else if (characterData.CanSwingSword() && characterData.Controller.SwingSwordInput())
        {
            climbVelocity = Vector3.Zero;
            characterData.Controller.Sword.Swing();
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
        return "CLIMB";
    }

    public void UpdatePositionAfterPoseUpdate()
    {
        faceOn.Update();
        characterData.Controller.PositionCharacterToFace(faceOn.FacePoint, faceOn.FaceNormal, lookAtDir, faceOn.FaceNormal);
        characterData.UpdateAfterGiantsPoseUpdate();
    }
}