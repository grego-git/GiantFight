using System.Diagnostics;

public class Constants
{
    public enum COLLIDER_LAYERS
    {
        PLAYER = 1,
        DYNAMIC_COLLIDER_NON_CLIMBABLE,
        DYNAMIC_COLLIDER_CLIMBABLE,
        SWORD = 5,
        GROUND,
        GIANT_LIMB,
        GIANT_HITBOX,
        GIANT_HURTBOX,
        RUBBLE
    }

    public const bool DEBUG = false;
}