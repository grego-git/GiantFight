using Godot;
using System;

public partial class DavyModel : Node3D
{
    private const string ANIM_LIBRARY = "davy/";


    public Skeleton3D Skeleton { get; private set; }
    public AnimationPlayer AnimPlayer { get; private set; }
    
    private string currentAnimation;
    private bool inAir;

    public override void _Ready()
    {
        base._Ready();
        Skeleton = (Skeleton3D)GetNode("Armature/Skeleton3D");
        AnimPlayer = (AnimationPlayer)GetNode("AnimationPlayer");
        
        PlayAnimation("idle");
    }

    public void Update(CharacterData characterData)
    {
        inAir = false;

        if (characterData.GetState() == "DEAD")
            Visible = false;
        else if (characterData.GetState() == "AIR")
        {
            inAir = true;
            PlayAirAnimations(characterData);
        }
        else
            PlayGroundedAnimations(characterData);

        GlobalPosition = characterData.Controller.GlobalPosition + (characterData.Controller.GlobalBasis.Y.Normalized() * 0.7f);
        LookAt(GlobalPosition + characterData.Controller.GlobalBasis.Z, characterData.Controller.GlobalBasis.Y.Normalized());
    }

    public void PlayAnimation(string animation, bool checkCurrentAnimation = true)
    {
        GD.Print("PLAYING " + animation);

        if (checkCurrentAnimation && (ANIM_LIBRARY + animation) == currentAnimation)
            return;

        AnimPlayer.Play(ANIM_LIBRARY + animation);
        currentAnimation = ANIM_LIBRARY + animation;
    }

    public void PlaySwingAnimation(string animation, bool checkCurrentAnimation = true)
    {
        if (checkCurrentAnimation && (ANIM_LIBRARY + (inAir ? "air_" : "") + animation) == currentAnimation)
            return;

        AnimPlayer.Play(ANIM_LIBRARY + (inAir ? "air_" : "") + animation);
        currentAnimation = ANIM_LIBRARY + (inAir ? "air_" : "") + animation;
    }

    public void PlayAnimationSection(string animation, float section)
    {
        AnimPlayer.PlaySection(ANIM_LIBRARY + animation, section);
        currentAnimation = ANIM_LIBRARY + animation;
    }

    public void PlayAirAnimations(CharacterData characterData)
    {
        if (characterData.Controller.Sword.IsSwinging())
            return;

        if (characterData.Controller.Sword.GetCharge() > 0.25f)
        {
            PlayAnimationSection((inAir ? "air_" : "") + "charge", characterData.Controller.Sword.GetCharge());
            return;
        }

        if (characterData.IsDashing()) 
        {
            PlayAnimation("dash");
            return;
        }
        
        if (characterData.Controller.Velocity.Y > 0.0f)
            PlayAnimation("jump");
        else
            PlayAnimation("fall");
    }

    public void PlayGroundedAnimations(CharacterData characterData)
    {
        if (characterData.IsStunned())
        {
            GD.Print("IS STUNNED");
            PlayAnimation("stun", false);
            return;
        }

        if (characterData.Controller.Sword.IsSwinging())
        {
            if (currentAnimation.StartsWith(ANIM_LIBRARY + "air_"))
                PlayAnimationSection(currentAnimation.Replace(ANIM_LIBRARY + "air_", ""), (float)AnimPlayer.CurrentAnimationPosition);
            
            return;
        }

        if (characterData.Controller.Sword.GetCharge() > 0.25f)
        {
            PlayAnimationSection((inAir ? "air_" : "") + "charge", characterData.Controller.Sword.GetCharge());
            return;
        }

        if (characterData.IsDashing()) 
        {
            PlayAnimation("dash");
            return;
        }

        if (characterData.Running)
            PlayAnimation("run");
        else
            PlayAnimation("idle");
    }
}
