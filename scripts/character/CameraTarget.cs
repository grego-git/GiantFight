using Godot;
using System;

public partial class CameraTarget : Node3D
{
    public override void _Ready()
    {
        base._Ready();
    }

    public void Update(float delta, CharacterData characterData)
    {
        Visible = characterData.Debug;

        Vector3 target = characterData.Controller.GlobalPosition;

        switch (characterData.GetState())
        {
            default:
                target += Vector3.Up * 3.0f;
                break;
            case "CLIMB":
            case "CRAWL":
            case "HANG":
                target += characterData.Controller.GlobalBasis.Y * 3.0f;
                break;
            case "LEDGE":
                target = characterData.Controller.Feet.GlobalPosition + characterData.Controller.GlobalBasis.Z;
                break;
        }

        float lerpSpeed = 8.0f;
        float maxOffset = 2.0f;
        float distanceToTarget = GlobalPosition.DistanceTo(target);

        if (distanceToTarget > (maxOffset + 0.1f))
            GlobalPosition = target + ((GlobalPosition - target).Normalized() * maxOffset);
        else
            GlobalPosition = GlobalPosition.MoveToward(target, lerpSpeed * (float)delta);
    }
}
