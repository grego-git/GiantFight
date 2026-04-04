using Godot;

public class GiantActionDead : IGiantAction
{
    private Giant giant;
    private string animation;

    public GiantActionDead(Giant giant, string animation)
    {
        this.giant = giant;
        this.animation = animation;
    }

    public bool Complete()
    {
        return false;
    }

    public void Init()
    {
        giant.AnimPlayer.Play(animation);
        giant.CurrentState = Giant.State.ACTION;
        
        giant.LeftArmIK.Influence = 0.0f;
        giant.RightArmIK.Influence = 0.0f;
        giant.LeftLegIK.Influence = 0.0f;

        giant.Fists[0].Monitorable = false;
        giant.Fists[0].Monitoring = false;

        giant.Fists[1].Monitorable = false;
        giant.Fists[1].Monitoring = false;

        giant.AnimPlayer.AnimationFinished += AnimationComplete;
    }

    public void Update(float delta)
    {
        
    }

    public void AnimationComplete(StringName animation)
    {
        giant.AnimPlayer.AnimationFinished -= AnimationComplete;
        
        foreach (var entity in giant.ClimbAnimatedEntities)
        {
            entity.Destroy();
        }

        giant.CharacterData.World.StageEnd = true;
    }
}