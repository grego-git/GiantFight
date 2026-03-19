using System.Collections.Generic;
using Godot;

public interface ICharacterState
{
    public void Update(float delta);
    public void Move(Vector2 moveDirection);
    public ICharacterState ChangeState();
    public bool OnThisEntity(ClimbableEntity entity);
    public HashSet<string> GetBoneImOnFromClimbable();
    public void EndOfFrame();
    public string GetState();
    public void UpdatePositionAfterPoseUpdate();
}