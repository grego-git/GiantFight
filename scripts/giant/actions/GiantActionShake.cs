using System.Collections.Generic;
using Godot;

public class GiantActionShake : IGiantAction
{
    private Giant giant;

    private string animation;

    private bool complete;

    public GiantActionShake(Giant giant, string animation)
    {
        this.giant = giant;
        this.animation = animation;

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
        giant.AnimPlayer.AnimationFinished += StompAnimationComplete;
    }

    public void Update(float delta)
    {
        
    }

    public void StompAnimationComplete(StringName animation)
    {
        complete = true;
        giant.AnimPlayer.AnimationFinished -= StompAnimationComplete;
    }
}