using Godot;
using System;

public partial class CameraTarget : Node3D
{
    public override void _Ready()
    {
        base._Ready();
        
        Visible = Constants.DEBUG;
    }

    public void Update(float delta, CharacterData characterData)
    {
        Visible = Constants.DEBUG;

        Vector3 target = characterData.Controller.GlobalPosition;

        switch (characterData.GetState())
        {
            default:
                target += Vector3.Up * GetPadding(characterData);
                break;
            case "CLIMB":
            case "CRAWL":
            case "HANG":
                target += characterData.Controller.GlobalBasis.Y * GetPadding(characterData);
                break;
        }

        float lerpSpeed = 8.0f;
        float maxOffset = 3.0f;
        float distanceToTarget = GlobalPosition.DistanceTo(target);

        if (distanceToTarget > (maxOffset + 0.1f))
            GlobalPosition = target + ((GlobalPosition - target).Normalized() * maxOffset);
        else
            GlobalPosition = GlobalPosition.MoveToward(target, lerpSpeed * (float)delta);
    }

    private float GetPadding(CharacterData characterData)
    {
        float padding = 2.0f;

        if (characterData.OnGiant)
        {
            if (characterData.Giant != null && characterData.Giant.Attacking)
                padding = 4.0f;
        }
        else if (characterData.InGiantProximity)
        {
            if (characterData.Giant != null && characterData.Giant.TrackPlayer)
                padding = 4.0f;
        }
        else
        {
            if (characterData.Giant != null && characterData.Giant.Attacking)
                padding = 4.0f;
        }

        return padding;
    }
}
