using System.Collections.Generic;
using Godot;

public class CharacterStateDead : ICharacterState
{
    private readonly CharacterData characterData;
    private float verticalVelocity;

    public CharacterStateDead(CharacterData characterData)
    {
        this.characterData = characterData;
    }

    public ICharacterState ChangeState()
    {
        return null;
    }

    public void Move(Vector2 moveDirection)
    {
        characterData.Controller.Velocity = verticalVelocity * Vector3.Up;
        characterData.Controller.MoveAndSlide();
    }

    public void Update(float delta)
    {
        if (characterData.Controller.IsOnFloor())
            verticalVelocity = 0.0f;
        else
            verticalVelocity += CharacterController.GRAVITY * delta;
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
        return "DEAD";
    }

    public void UpdatePositionAfterPoseUpdate()
    {
        return;
    }
}