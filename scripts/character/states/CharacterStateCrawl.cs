using System.Collections.Generic;
using Godot;

public class CharacterStateCrawl : ICharacterState
{
    private const float Y_OFFSET = -1.5f;
    private readonly CharacterData characterData;
    private Vector3 dashDir;
    private CrawlFace faceOn;
    private Vector3 lookAtDir;
    private Vector3 climbVelocity;

    public CharacterStateCrawl(CharacterData characterData, SpecialFace attachedFace = null)
    {
        this.characterData = characterData;
        lookAtDir = characterData.Controller.GetLookAtDir(Vector3.Up, Vector3.Forward);

        GD.Print("ENTERING CRAWL STATE");

        if (attachedFace != null && attachedFace.GetType() == typeof(ClimbFace) && ((ClimbFace)attachedFace).ChangeToCrawl)
        {
            faceOn = CrawlFace.CreateCrawlFaceFromClimbFace((ClimbFace)attachedFace, characterData.CameraController);
            
            if (characterData.IsDashing())
                dashDir = characterData.Controller.Velocity.Normalized();
        }
        else
        {
            faceOn = CrawlFace.CreateCrawlFaceFromRayCast(characterData.Controller.CrawlRayCast, characterData.CameraController);
            characterData.DashMeter.Empty();
        }

        if (characterData.GetState() == "CLIMB")
            characterData.Controller.HorizontalReOrient(characterData.CameraController, Vector3.Up, true);

        characterData.Controller.EnteringNewState(GetState());

        faceOn.ClimbableEntity.CharacterData = characterData;
    }

    public ICharacterState ChangeState()
    {
        faceOn.Update();

        bool validCrawlFace = CrawlFace.ValidCrawlFaceNormal(faceOn.FaceNormal);

        if (!faceOn.ChangeToClimb && !validCrawlFace)
        {
            if (ClimbFace.ValidClimbFaceNormal(faceOn.FaceNormal))
            {
                faceOn.ChangeToClimb = true;
            }
        }

        if (characterData.CanControl() && characterData.Controller.GroundedJumpInput())
        {
            Vector3 horizontalVelocity = Utils.GetFlatDirectionalVector(climbVelocity) * characterData.ClimbSpeed;
            return new CharacterStateAir(characterData, horizontalVelocity, characterData.JumpSpeed, false, fromChargeJump: true);
        }
        else if (faceOn.ChangeToClimb)
        {
            return new CharacterStateClimb(characterData, faceOn);
        }
        else if (characterData.HasLostGrip() || !characterData.Controller.GripInput() || !validCrawlFace)
        {
            return new CharacterStateAir(characterData, Vector3.Zero, 0.0f, false);
        }
        else if (characterData.Controller.DetectedCrawlableGround())
        {
            return new CharacterStateCrawl(characterData);
        }
        else if (characterData.Controller.DetectedClimbableWall())
        {
            return new CharacterStateClimb(characterData);
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
            else if (characterData.IsDashing())
                lookAtDir = characterData.Controller.Velocity.Normalized();

            characterData.Controller.PositionCharacterToFace(faceOn.FacePoint, faceOn.FaceNormal, lookAtDir, faceOn.FaceNormal);
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
        return "CRAWL";
    }

    public void UpdatePositionAfterPoseUpdate()
    {
        faceOn.Update();
        characterData.Controller.PositionCharacterToFace(faceOn.FacePoint, faceOn.FaceNormal, lookAtDir, faceOn.FaceNormal);
        characterData.UpdateAfterGiantsPoseUpdate();
    }
}