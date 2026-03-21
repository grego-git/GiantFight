using Godot;
using System;

public partial class PlayerDetection : Node3D
{
    public enum DetectionZoneAreas
    {
        TOP,
        MIDDLE,
        FLOOR,
        ON_GIANT,
        NONE
    }

    [Export]
    public ShapeCast3D[] DetectionZones { get; set; }

    public DetectionZoneAreas PlayerDetectionZone { get; set; }
    public Vector3 PlayerPosition { get; set; }
    public float DistanceToPlayer { get; set; }
    public float AngleToPlayer { get; set; }
    public bool FacingPlayer { get; set; }

    public void Update(float delta)
    {
        PlayerDetectionZone = DetectionZoneAreas.NONE;

        for (int i = 0; i < DetectionZones.Length; i++)
        {
            DetectionZones[i].ForceUpdateTransform();
            
            if (DetectionZones[i].IsColliding())
            {
                PlayerDetectionZone = (DetectionZoneAreas)i;
                PlayerPosition = ((Node3D)DetectionZones[i].GetCollider(0)).GlobalPosition;
                
                Vector3 flatPlayerPosition = Utils.GetFlatSpatialVector(PlayerPosition, GlobalPosition.Y);
                Vector3 toPlayer = flatPlayerPosition - GlobalPosition;

                FacingPlayer = GlobalBasis.Z.Normalized().AngleTo(toPlayer.Normalized()) < Mathf.DegToRad(15.0f);

                DistanceToPlayer = toPlayer.Length();
                AngleToPlayer = Vector3.Back.SignedAngleTo(toPlayer.Normalized(), Vector3.Up);
                
                if (AngleToPlayer < 0.0f)
                    AngleToPlayer = (2.0f * Mathf.Pi) + AngleToPlayer;
                break;
            }
        }
    }
}
