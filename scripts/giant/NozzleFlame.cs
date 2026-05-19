using Godot;
using System;

public partial class NozzleFlame : ShapeCast3D
{
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    
        ForceShapecastUpdate();

        Enabled = ((GpuParticles3D)GetParent()).Emitting;

        if (!Enabled)
            return;

        for (int i = 0; i < GetCollisionCount(); i++)
        {
            CollisionObject3D collisionObject = (CollisionObject3D)GetCollider(i);

            if (collisionObject.GetCollisionLayerValue((int)Constants.COLLIDER_LAYERS.PLAYER))
            {
                CharacterController characterController = (CharacterController)GetCollider(i);
                characterController.EmitSignal("Hit");
            }
            else
            {
                FireLimb limb = (FireLimb)((Node3D)GetCollider(i)).GetNode("FlameLimb");
                limb.HeatUp = true;
            }
        }
    }
}
