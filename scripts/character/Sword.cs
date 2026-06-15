using Godot;
using System;

public partial class Sword : Node3D
{
    [Signal]
    public delegate void HitSomethingEventHandler();

    public enum State
    {
        IDLE,
        SWING
    }


    [Export]
    public DavyModel model { get; set; }

    public State CurrentState { get; set; }
    
    public int Damage { get; private set; }

    private Meter chargeMeter;
    private Area3D swordBox;
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

        model.AnimPlayer.AnimationFinished += AnimationFinished;
        swordBox.BodyEntered += BodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        int swordBoneId = model.Skeleton.FindBone("Sword_2");
        Transform3D swordBoneTransform = model.Skeleton.GlobalTransform * model.Skeleton.GetBoneGlobalPose(swordBoneId);

        GlobalPosition = swordBoneTransform.Origin;
        LookAt(GlobalPosition + swordBoneTransform.Basis.Z, swordBoneTransform.Basis.Y.Normalized());

        switch(CurrentState)
        {
            case State.IDLE:
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
            Damage = 3 + (int)(chargeMeter.NormalizedFill() * 3.0f);
            model.PlaySwingAnimation("charge_swing");
        }
        else 
        {
            Damage = 1;
            model.PlaySwingAnimation(SWING_ANIMATIONS[swing_index]);
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
            giantHitPoint.Hit(Damage);
            EmitSignal("HitSomething");
            hit = true;
        }
        else if (node.Name.ToString().ToLower().Contains("practicehealth"))
        {
            HitPoint giantHitPoint = (HitPoint)node;
            giantHitPoint.Hit(Damage);
            EmitSignal("HitSomething");
            hit = true;
        }
    }

    public float GetCharge()
    {
        return chargeMeter.Value;
    }
}
