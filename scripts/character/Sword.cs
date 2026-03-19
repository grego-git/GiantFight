using Godot;
using System;

public partial class Sword : Node3D
{
    public enum State
    {
        IDLE,
        SWING
    }


    [Export]
    public AnimationPlayer anim { get; set; }

    public State CurrentState { get; set; }

    private string[] SWING_ANIMATIONS =
    {
        "sword_swing",
        "sword_swing_2"
    };

    private int swing_index;

    public override void _Ready()
    {
        base._Ready();

        anim.AnimationFinished += AnimationFinished;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        switch(CurrentState)
        {
            case State.IDLE:
                anim.Play("idle");
                break;
            case State.SWING:
                break;
        }
    }

    public bool IsSwinging()
    {
        return CurrentState == State.SWING;
    }

    public void Swing()
    {
        anim.Play(SWING_ANIMATIONS[swing_index]);

        swing_index = swing_index + 1 == SWING_ANIMATIONS.Length ? 0 : (swing_index + 1);

        CurrentState = State.SWING;
    }

    public void AnimationFinished(StringName animation)
    {
        if (CurrentState == State.SWING)
            CurrentState = State.IDLE;
    }
}
