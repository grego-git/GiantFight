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

    private Meter chargeMeter;
    private Area3D swordBox;
    private int damage;
    private bool hit;

    private string[] SWING_ANIMATIONS =
    {
        "sword_swing",
        "sword_swing_2"
    };

    private int swing_index;

    public override void _Ready()
    {
        base._Ready();

        swordBox = (Area3D)GetNode("SwordBox");

        chargeMeter = new Meter(1.5f);

        anim.AnimationFinished += AnimationFinished;
        swordBox.BodyEntered += BodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        switch(CurrentState)
        {
            case State.IDLE:
                if (chargeMeter.Value > 0.25f)
                    anim.PlaySection("charge", chargeMeter.Value);
                else
                    anim.Play("idle");

                swordBox.Monitorable = false;
                swordBox.Monitoring = false;
                hit = false;
                break;
            case State.SWING:
                swordBox.Monitorable = true;
                swordBox.Monitoring = true;
                break;
        }
    }

    public bool IsSwinging()
    {
        return CurrentState == State.SWING;
    }

    public bool ChargingMeter()
    {
        return !chargeMeter.IsEmpty();
    }

    public void Charge(float charge)
    {
        chargeMeter.FillMeter(charge);
    }

    public void EmptyCharge()
    {
        chargeMeter.Empty();
    }

    public void Swing()
    {
        if (chargeMeter.IsEmpty())
            return;
        
        if (chargeMeter.Value > 0.25f)
        {
            damage = 2 + (int)(chargeMeter.NormalizedFill() * 3.0f);
            anim.Play("charge_swing");
        }
        else 
        {
            damage = 1;
            anim.Play(SWING_ANIMATIONS[swing_index]);
            swing_index = swing_index + 1 == SWING_ANIMATIONS.Length ? 0 : (swing_index + 1);
        }

        chargeMeter.Empty();
        CurrentState = State.SWING;
    }

    public void AnimationFinished(StringName animation)
    {
        if (CurrentState == State.SWING)
            CurrentState = State.IDLE;
    }

    public void BodyEntered(Node3D node)
    {
        if (hit)
            return;
        
        GD.Print("HIT: " + node.Name);

        if (node.Name.ToString().ToLower().Contains("hitpoint"))
        {
            GiantHitPoint giantHitPoint = (GiantHitPoint)node;
            giantHitPoint.Hit(damage);
            hit = true;
        }
    }
}
