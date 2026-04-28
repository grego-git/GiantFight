using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerDetection : Node3D
{
    public enum DetectionZoneAreas
    {
        NEGATE,
        TOP,
        MIDDLE,
        FLOOR,
        ON_GIANT,
        NONE,
        DEAD
    }

    [Export]
    public ShapeCast3D[] DetectionZones { get; set; }

    public DetectionZoneAreas PlayerDetectionZone { get; set; }
    public Vector3 PlayerPosition { get; set; }
    public float DistanceToPlayer { get; set; }
    public float AngleToPlayer { get; set; }
    public bool FacingPlayer { get; set; }

    public void Update(CharacterData characterData, HashSet<string> bonesPlayerIsOn)
    {
        if (characterData.GetState() == "DEAD")
        {
            PlayerDetectionZone = DetectionZoneAreas.DEAD;
        }
        else if (bonesPlayerIsOn != null && bonesPlayerIsOn.Count > 0)
        {
            PlayerDetectionZone = DetectionZoneAreas.ON_GIANT;
            PlayerPosition = characterData.Controller.GlobalPosition;
            DistanceToPlayer = 0.0f;
            FacingPlayer = true;
            
            characterData.InGiantProximity = true;
        }
        else 
        {
            PlayerDetectionZone = DetectionZoneAreas.NONE;
            characterData.InGiantProximity = false;
            
            for (int i = 0; i < DetectionZones.Length; i++)
            {
                if (DetectionZones[i] == null)
                    continue;

                DetectionZones[i].ForceUpdateTransform();

                if (DetectionZones[i].IsColliding())
                {
                    PlayerDetectionZone = (DetectionZoneAreas)i;
                    PlayerPosition = ((Node3D)DetectionZones[i].GetCollider(0)).GlobalPosition;
                    characterData.InGiantProximity = true;
                    break;
                }
            }

            if (PlayerDetectionZone == DetectionZoneAreas.NONE)
                PlayerPosition = characterData.Controller.GlobalPosition;

            Vector3 flatPlayerPosition = Utils.GetFlatSpatialVector(PlayerPosition, GlobalPosition.Y);
            Vector3 toPlayer = flatPlayerPosition - GlobalPosition;

            FacingPlayer = GlobalBasis.Z.Normalized().AngleTo(toPlayer.Normalized()) < Mathf.DegToRad(15.0f);

            DistanceToPlayer = toPlayer.Length();
            AngleToPlayer = Vector3.Back.SignedAngleTo(toPlayer.Normalized(), Vector3.Up);

            if (AngleToPlayer < 0.0f)
                AngleToPlayer = (2.0f * Mathf.Pi) + AngleToPlayer;
        }
    }
}
