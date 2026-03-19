using System.Collections.Generic;
using System.ComponentModel;
using Godot;

public partial class CharacterData : Node3D
{
    [Export]
    public bool Debug { get; set; }
    [Export]
    public float Speed { get; set; }
    [Export]
    public float ClimbSpeed { get; set; }
    [Export]
    public float JumpSpeed { get; set; }
    [Export]
    public float DashSpeed { get; set; }
    [Export]
    public float GripDepleteRate { get; set; }
    [Export]
    public float GripReplenishRate { get; set; }
    [Export]
    public float FatigueReplenishRate { get; set; }
    [Export]
    public float MaxHealth { get; set; }
    [Export]
    public float MaxStamina { get; set; }
    [Export]
    public float MaxStun { get; set; }
    [Export]
    public float MaxAim { get; set; }
    [Export]
    public float MaxDash { get; set; }
    [Export]
    public float AccelerationStunThreshold { get; set; }
    [Export]
    public float AccelerationKnockBackThreshold { get; set; }

    public ICharacterState CurrentCharacterState { get; set; }
    public CharacterController Controller { get; private set; }
    public CameraController CameraController { get; private set; }

    public bool CanTakeDamage { get; set; }
    public bool IsOnGround { get; set; }
    public bool Running { get; set; }
    public bool IsFatigued { get; set; }
    public Meter HealthMeter { get; private set; }
    public Meter StaminaMeter { get; private set; }
    public Meter FatigueMeter { get; private set; }
    public Meter DashMeter { get; private set; }
    public Meter DeathMeter { get; private set; }
    public float FatigueNeedleSpeed { get; private set; }
    public float ShakeStaminaMeterTime { get; set; }
    public bool FillFatigueUp { get; private set; }
    public bool CanRelieveFatigue { get; private set; }

    public override void _Ready()
    {
        HealthMeter = new Meter(MaxHealth, fill:true);
        StaminaMeter = new Meter(MaxStamina, fill:true);
        DeathMeter = new Meter(5.0f, fill:true);
        FatigueMeter = new Meter(1.0f);
        DashMeter = new Meter(MaxDash);
        CanTakeDamage = true;
        FillFatigueUp = true;

        Controller = (CharacterController)GetNode("CharacterController");
        CameraController = (CameraController)GetNode("CameraOrientation");
        CurrentCharacterState = new CharacterStateGrounded(this, Vector3.Zero);
    
        Controller.Hit += Hit;
        Controller.Punched += Punched;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (Controller.DebugInput())
            Debug = !Debug;
        
        FillMeters((float)delta);
        ChangeState();

        Controller.Update((float)delta, CurrentCharacterState);
        IsOnGround = Controller.CheckIfOnGround();
        CurrentCharacterState.EndOfFrame();

        CameraController.Update((float)delta, this);
        CheckForDeath();

        Controller.Feet.Visible = Debug;
    }

    public void FillMeters(float delta)
    {
        if (IsFatigued)
        {
            FatigueNeedleSpeed = (StaminaMeter.NormalizedFill() * 1.5f) + 1.0f;
            FatigueMeter.FillMeter(delta * FatigueNeedleSpeed * (FillFatigueUp ? 1.0f : -1.0f));

            if (FatigueMeter.IsFilled() && FillFatigueUp)
            {
                FillFatigueUp = false;
                CanRelieveFatigue = true;
            }
            else if (FatigueMeter.IsEmpty() && !FillFatigueUp)
            {
                FillFatigueUp = true;
                CanRelieveFatigue = true;
            }
        }
        else
        {
            FatigueNeedleSpeed = 1.0f;
            FatigueMeter.FillToMiddle();
            FillFatigueUp = true;
            CanRelieveFatigue = true;
        }

        switch (GetState())
        {
            default:
                StaminaMeter.FillMeter((IsFatigued ? FatigueReplenishRate : GripReplenishRate) * delta);

                if (IsFatigued && CanRelieveFatigue && Controller.RelieveFatigueInput() && !IsDashing())
                    RelieveFatigue();
                break;
            case "HANG":
                //StaminaMeter.FillMeter(-GripDepleteRate * 1.25f * delta);
                break;
            case "CRAWL":
                //StaminaMeter.FillMeter(GripDepleteRate * 0.5f * delta);
                break;
            case "CLIMB":
            case "LEDGE":
                //StaminaMeter.FillMeter(-GripDepleteRate * delta);
                break;
            case "GRABBED":
                break;
            case "DEAD":
                DeathMeter.FillMeter(-delta);
                break;
        }
        
        if (StaminaMeter.IsFilled())
            IsFatigued = false;
        else if (StaminaMeter.IsEmpty())
            IsFatigued = true;

        if (!DashMeter.IsEmpty())
        {
            DashMeter.FillMeter(-delta);
        }
            
    }

    public bool HasGrip()
    {
        return !StaminaMeter.IsEmpty() && !IsFatigued;
    }

    public bool HasLostGrip()
    {
        return StaminaMeter.IsEmpty() || IsFatigued;
    }

    public bool IsDashing()
    {
        return !DashMeter.IsEmpty();
    }

    public bool CanControl()
    {
        if (IsDashing())
            return false;

        if (Controller.Sword.IsSwinging())
            return false;
        
        if (IsFatigued)
            return false;
        
        return true;
    }

    public bool CanGrip()
    {
        if (!HasGrip())
            return false;
        
        return true;
    }

    public bool CanDash()
    {
        if (IsDashing())
            return false;

        if (Controller.Sword.IsSwinging())
            return false;
        
        if (StaminaMeter.IsEmpty() || IsFatigued)
            return false;
        
        return true;
    }

    public bool CanSwingSword()
    {
        if (IsDashing())
            return false;
        
        if (Controller.Sword.IsSwinging())
            return false;
        
        if (StaminaMeter.IsEmpty() || IsFatigued)
            return false;
        
        return true;
    }

    public bool CanEnterStun(float acceleration)
    {
        if (acceleration > AccelerationStunThreshold)
            return true;
        
        return false;
    }

    public bool CanEnterKnockedBack(float acceleration)
    {
        return acceleration > AccelerationKnockBackThreshold;
    }

    public void TakeDamageFromFall(float fallSpeed)
    {
        if (!CanTakeDamage)
            return;
        
        float damage = Mathf.Lerp(0.0f, MaxHealth, Mathf.Clamp((fallSpeed / 25.0f) - 1.0f, 0.0f, 1.0f));

        TakeDamage(damage);

        if (damage > MaxHealth * 0.25f)
            Stun();
    }

    public void TakeDamage(float damage)
    {
        if (!CanTakeDamage)
            return;
        
        HealthMeter.FillMeter(-damage);
    }

    public void Dash(Vector3 dir)
    {
        DashMeter.FillToMax();
        StaminaMeter.FillMeter(GetState() == "AIR" ? -7.5f : -5.0f);
        CameraController.Shake(0.125f, 2.0f);

        if (StaminaMeter.IsEmpty())
            IsFatigued = true;
    }

    public void EnterFatigue()
    {
        StaminaMeter.Empty();
        IsFatigued = true;
    }

    public void RelieveFatigue()
    {
        float needlePoint = FatigueMeter.NormalizedFill();
        needlePoint = (needlePoint - 0.5f) * 2.0f;

        if (Mathf.Abs(needlePoint) < 0.25f)
        {
            ShakeStaminaMeterTime = 1.0f;
            StaminaMeter.FillMeter(7.5f);
        }
        else if (Mathf.Abs(needlePoint) < 0.75f)
        {
            ShakeStaminaMeterTime = 0.5f;
            StaminaMeter.FillMeter(5.0f);
        }
        
        CanRelieveFatigue = false;
    }

    public void Stun()
    {
        
    }

    public void KnockedBack(Vector3 knockBackVelocity)
    {
        
    }

    public string GetState()
    {
        return CurrentCharacterState?.GetState() ?? "NONE";
    }

    public bool OnThisEntity(ClimbableEntity entity)
    {
        if (CurrentCharacterState == null)
            return false;

        return CurrentCharacterState.OnThisEntity(entity);
    }

    public HashSet<string> GetBoneImOnFromClimbable()
    {
        if (CurrentCharacterState == null)
            return null;

        return CurrentCharacterState.GetBoneImOnFromClimbable();
    }
    
    public void ChangeState()
    {
        var newState = CurrentCharacterState.ChangeState();

        if (newState != null)
        {
            GD.Print("CHANGED STATE");
            CurrentCharacterState = newState;
        }
    }

    public void UpdateAfterGiantsPoseUpdate()
    {
        
    }

    public void CheckForDeath()
    {
        if (HealthMeter.Value <= 0.0f)
        {
            if (GetState() != "DEAD")
                CurrentCharacterState = new CharacterStateDead(this);
            else if (DeathMeter.IsEmpty())
            {
                return;
            }
        }
    }

    public void Hit(Vector3 attackPoint, float damage, float knockBackSpeed, bool checkIfStunned, bool attackInTheAir)
    {
        string state = GetState();

        if (!attackInTheAir && state == "AIR")
            return;
        
        switch (state)
        {
            case "HANG":
            case "CRAWL":
            case "CLIMB":
            case "LEDGE":
                Stun();
                TakeDamage(damage);
                break;
            default:
                Vector3 toPlayer = Utils.GetFlatDirectionalVector(Controller.GlobalPosition - attackPoint);
                Vector3 velocity = toPlayer * knockBackSpeed;

                if (velocity == Vector3.Zero)
                    velocity = Vector3.Forward * knockBackSpeed;

                KnockedBack(velocity);
                TakeDamage(damage);
                break;
        }
    }

    public void Punched(Vector3 knockBack, float damage, float cameraShakeTime, float cameraShakeStrength, float drainMeter)
    {
        CurrentCharacterState = new CharacterStateThrown(this, knockBack, 0.0f);
        CameraController.Shake(cameraShakeTime, cameraShakeStrength);
        CameraController.Update(0.0f, this);
        TakeDamage(damage);
        StaminaMeter.FillMeter(MaxStamina * (1.0f - drainMeter));
    }
}
