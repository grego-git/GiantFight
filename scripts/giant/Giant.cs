using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class Giant : Node3D
{
    public enum State
    {
        DETERMINING,
        ACTION
    }

    [Export]
    public CharacterData CharacterData { get; set; }
    [Export]
    public AudioStreamPlayer3D StompSound { get; set; }
    [Export]
    public string GiantJson { get; set; }
    [Export]
    public float TurnSpeed { get; set; }
    [Export]
    public float StompPadding { get; set; }
    [Export]
    public bool TrackPlayer { get; set; }
    [Export]
    public bool StunPlayer { get; set; }
    [Export]
    public bool ShakeCamera { get; set; }

    [Export]
    public ClimbableAnimatedEntity[] ClimbAnimatedEntities { get; set; }
    [Export]
    public GiantLimb[] ArmLimbs { get; set; }
    [Export]
    public GiantHitBox[] Fists { get; set; }
    [Export]
    public GiantHitPoint[] HitPoints  { get; set; }

    [Export]
    public SkeletonIK3D LeftLegIK { get; set; }
    [Export]
    public SkeletonIK3D LeftArmIK { get; set; }
    [Export]
    public SkeletonIK3D RightArmIK { get; set; }

    public IGiantAction CurrentAction { get; set; }
    public State CurrentState { get; set; }
    public Skeleton3D Skeleton { get; set; }
    public MeshInstance3D Mesh { get; set; }
    public PlayerDetection PlayerDetection { get; set; }
    public AnimationPlayer AnimPlayer { get; set; }
    public GiantProfile GiantProfile { get; set; }
    public Meter AgroMeter { get; set; }
    public Meter SlamTimer { get; set; }
    public HashSet<string> BonesPlayerIsOn { get; set; }

    public int MaxHP { get; private set; }
    public int CurrentHP { get; private set; }

    public Node3D LeftLegIKTarget { get; set; }
    public Node3D LeftArmIKTarget { get; set; }
    public Node3D RightArmIKTarget { get; set; }

    private StandardMaterial3D material;
    private float yRot;

    public override void _Ready()
    {
        base._Ready();

        Skeleton = (Skeleton3D)GetNode("Armature/Skeleton3D");
        Mesh = (MeshInstance3D)Skeleton.GetNode("Sphere_001");
        PlayerDetection = (PlayerDetection)GetNode("PlayerDetection");
        AnimPlayer = (AnimationPlayer)GetNode("AnimationPlayer");

        LeftLegIKTarget = (Node3D)GetNode("LeftLegIKTarget");
        LeftArmIKTarget = (Node3D)GetNode("LeftArmIKTarget");
        RightArmIKTarget = (Node3D)GetNode("RightArmIKTarget");

        string json = Godot.FileAccess.GetFileAsString("res://giant_jsons/" + GiantJson);
        GiantProfile = JsonConvert.DeserializeObject<GiantProfile>(json);

        AgroMeter = new Meter(3.0f);
        SlamTimer = new Meter(10.0f);

        material = (StandardMaterial3D)Mesh.Mesh.SurfaceGetMaterial(0);
        
        foreach (var hitPoint in HitPoints)
        {
            MaxHP += hitPoint.HP;
            CurrentHP += hitPoint.HP;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        bool dead;
        
        CurrentHP = 0;

        foreach (var hitPoint in HitPoints)
            CurrentHP += hitPoint.HP;

        dead = CurrentHP <= 0;

        if (!dead) 
        {
            BonesPlayerIsOn = GetBonesPlayerIsOn();

            if (PlayerDetection.PlayerDetectionZone == PlayerDetection.DetectionZoneAreas.ON_GIANT)
                material.AlbedoColor = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            else
                material.AlbedoColor = Colors.White;

            if (StunPlayer && !string.IsNullOrEmpty(AnimPlayer.CurrentAnimation) && BonesPlayerIsOn != null && BonesPlayerIsOn.Count > 0)
                StunPlayerOnGiant();

            PlayerDetection.Update(CharacterData, BonesPlayerIsOn);

            if (ShakeCamera)
            {
                if (BonesPlayerIsOn != null && BonesPlayerIsOn.Count > 0)
                    CharacterData.CameraController.Shake(1.0f, 5.0f);
                
                ShakeCamera = false;
            }
        }
        else
        {
            if (CurrentAction.GetType() != typeof(GiantActionDead))
            {
                CurrentAction = new GiantActionDead(this, GiantProfile.DeadAnimation);
                CurrentAction.Init();
            }

            material.AlbedoColor = Colors.White;

            CharacterData.InGiantProximity = false;
        }

        switch (CurrentState)
        {
            case State.DETERMINING:
                switch (PlayerDetection.PlayerDetectionZone)
                {
                    case PlayerDetection.DetectionZoneAreas.NEGATE:
                        SlamTimer.FillMeter((float)delta);

                        if (SlamTimer.IsFilled())
                        {
                            SlamTimer.Empty();
                            CurrentAction = new GiantActionPlayAnimation(this, GiantProfile.SlamAnimation);
                            CurrentAction.Init();
                        }
                        break;
                    case PlayerDetection.DetectionZoneAreas.NONE:
                        SlamTimer.Empty();
                        AgroMeter.FillMeter((float)delta);

                        if (AgroMeter.IsFilled())
                        {
                            CurrentAction = new GiantActionPlayAnimation(this, GiantProfile.ExternalAttackAnimation);
                            CurrentAction.Init();
                        }
                        break;
                    case PlayerDetection.DetectionZoneAreas.ON_GIANT:  
                        SlamTimer.Empty();
                        string shakeAnimation = GetShakeAnimation(BonesPlayerIsOn);
                        string attackAnimation = GetAttackAnimation(BonesPlayerIsOn);

                        if (AgroMeter.IsFilled() && !string.IsNullOrEmpty(attackAnimation))
                        {
                            bool useLeftHand;

                            if (attackAnimation.ToLower().Contains("_arm"))
                            {
                                if (attackAnimation.ToLower().Contains("_left"))
                                    useLeftHand = false;
                                else
                                    useLeftHand = true;
                            }
                            else
                            {
                                if (attackAnimation.ToLower().Contains("_left"))
                                    useLeftHand = true;
                                else
                                    useLeftHand = false;
                            }

                            CurrentAction = new GiantActionBodyAttack(this, attackAnimation,  useLeftHand);
                            CurrentAction.Init();
                        }
                        else if (!string.IsNullOrEmpty(shakeAnimation)) 
                        {
                            CurrentAction = new GiantActionShake(this, shakeAnimation);
                            CurrentAction.Init();
                        }
                        break;
                    case PlayerDetection.DetectionZoneAreas.FLOOR:
                        SlamTimer.Empty();
                        CurrentAction = new GiantActionTrackStomp(this);
                        CurrentAction.Init();
                        break;
                    case PlayerDetection.DetectionZoneAreas.MIDDLE:
                        SlamTimer.Empty();
                        CurrentAction = new GiantActionTrackClap(this);
                        CurrentAction.Init();
                        break;
                    case PlayerDetection.DetectionZoneAreas.TOP:
                        SlamTimer.Empty();
                        CurrentAction = new GiantActionTrackPunch(this);
                        CurrentAction.Init();
                        break;
                    case PlayerDetection.DetectionZoneAreas.DEAD:
                        SlamTimer.Empty();
                        break;
                }
                break;
            case State.ACTION:
                switch (PlayerDetection.PlayerDetectionZone)
                {
                    default:
                        AgroMeter.FillMeter((float)delta);
                        break;
                }

                if (CurrentAction != null)
                {
                    CurrentAction.Update((float)delta);

                    if (CurrentAction.Complete())
                    {
                        AnimPlayer.Play(GiantProfile.IdleAnimation);
                        CurrentState = State.DETERMINING;
                    }
                }
                break;
        }
        
    }

    public void RotateTowardsPoint(float delta, Vector3 point)
    {
        Vector3 toPoint = Utils.GetFlatSpatialVector(point, GlobalPosition.Y) - GlobalPosition;
        float angleToPoint = Vector3.Back.SignedAngleTo(toPoint.Normalized(), Vector3.Up);

        yRot = (float)Utils.MoveTowardsAngle(yRot, angleToPoint, TurnSpeed * delta);
        GlobalRotation = new Vector3(GlobalRotation.X, yRot, GlobalRotation.Z);
    }

    private void StunPlayerOnGiant()
    {
        foreach (var stunBones in GiantProfile.StunBones)
        {
            bool stunnedPlayer = false;

            if (AnimPlayer.CurrentAnimation == stunBones.Key)
            {
                foreach (var boneOn in BonesPlayerIsOn)
                {
                    if (stunBones.Value.Contains(boneOn))
                        CharacterData.Stun();
                        stunnedPlayer = true;
                        break;
                }
            }

            if (stunnedPlayer)
                break;
        }
    }
    
    private HashSet<string> GetBonesPlayerIsOn()
    {
        foreach (var climbable in ClimbAnimatedEntities)
        {
            if (CharacterData.OnThisEntity(climbable))
                return CharacterData.GetBoneImOnFromClimbable();
        }

        return null;
    }

    private string GetShakeAnimation(HashSet<string> bonesPlayerIsOn)
    {
        if (bonesPlayerIsOn == null || bonesPlayerIsOn.Count == 0)
            return "";

        foreach (var shakeAnimation in GiantProfile.ShakeAnimations)
        {
            foreach (var bone in shakeAnimation.Value)
            {
                if (bonesPlayerIsOn.Contains(bone))
                    return shakeAnimation.Key;
            }
        }

        return "";
    }

    private string GetAttackAnimation(HashSet<string> bonesPlayerIsOn)
    {
        if (bonesPlayerIsOn == null || bonesPlayerIsOn.Count == 0)
            return "";

        foreach (var attackAnimation in GiantProfile.AttackAnimations)
        {
            foreach (var bone in attackAnimation.Value)
            {
                if (bonesPlayerIsOn.Contains(bone))
                    return attackAnimation.Key;
            }
        }

        return "";
    }
}
