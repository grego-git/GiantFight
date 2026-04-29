using Godot;

public class GiantActionBodyAttack : IGiantAction
{
    private Giant giant;

    private string animation;

    private bool complete;
    private bool useLeftHand;

    public GiantActionBodyAttack(Giant giant, string animation, bool useLeftHand)
    {
        this.giant = giant;
        this.animation = animation;
        this.useLeftHand = useLeftHand;

        complete = false;
    }

    public bool Complete()
    {
        return complete;
    }

    public void Init()
    {
        giant.AnimPlayer.Play(animation);
        giant.CurrentState = Giant.State.ACTION;
        giant.AnimPlayer.AnimationFinished += AnimationComplete;
    }

    public void Update(float delta)
    {
        if (giant.TrackPlayer)
        {
            for (int i = useLeftHand ? 0 : 2; i < (useLeftHand ? 2 : 4); i++)
            {
                giant.ArmLimbs[i].Monitorable = false;
                giant.ArmLimbs[i].Monitoring = false;
            }
        }
        else
        {
            for (int i = useLeftHand ? 0 : 2; i < (useLeftHand ? 2 : 4); i++)
            {
                giant.ArmLimbs[i].Monitorable = true;
                giant.ArmLimbs[i].Monitoring = true;
            }
        }
    }

    public void AnimationComplete(StringName animation)
    {
        complete = true;
        giant.AnimPlayer.AnimationFinished -= AnimationComplete;
        giant.AgroMeter.Empty();
    }
}