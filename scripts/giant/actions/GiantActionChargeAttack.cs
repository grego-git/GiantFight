using Godot;

public class GiantActionChargeAttack : IGiantAction
{
    private Giant giant;
    private string chargeAnimation;
    private string endAnimation;
    private float chargeSpeed;
    private bool chargeAtPlayer;
    private bool atPlayerPos;
    private bool complete;

    public GiantActionChargeAttack(Giant giant, string chargeAnimation, string endAnimation, float chargeSpeed)
    {
        this.giant = giant;
        this.chargeAnimation = chargeAnimation;
        this.endAnimation = endAnimation;
        this.chargeSpeed = chargeSpeed;

        complete = false;
    }

    public bool Complete()
    {
        return complete;
    }

    public void Init()
    {
        giant.AnimPlayer.Play(chargeAnimation);
        giant.CurrentState = Giant.State.ACTION;
        giant.AnimPlayer.AnimationFinished += AnimationComplete;
    }

    public void Update(float delta)
    {
        if (atPlayerPos)
            return;
        
        giant.RotateTowardsPoint(delta, giant.PlayerDetection.PlayerPosition);
        
        if (chargeAtPlayer) 
        {
            Vector3 flatPlayerPos = Utils.GetFlatSpatialVector(giant.PlayerDetection.PlayerPosition, giant.GlobalPosition.Y);
            
            if (flatPlayerPos.DistanceTo(giant.GlobalPosition) < 90.0f)
            {
                atPlayerPos = true;

                if (string.IsNullOrEmpty(endAnimation))
                {
                    complete = true;
                    giant.AnimPlayer.AnimationFinished -= AnimationComplete;
                    giant.AgroMeter.Empty();
                }
                else
                    giant.AnimPlayer.Play(endAnimation);
            }
            else 
                giant.GlobalPosition += giant.GlobalBasis.Z.Normalized() * chargeSpeed * delta;
        }
    }

    public void AnimationComplete(StringName animation)
    {
        if (!string.IsNullOrEmpty(chargeAnimation) && animation == chargeAnimation)
            chargeAtPlayer = true;
        else if (!string.IsNullOrEmpty(endAnimation) && animation == endAnimation)
        {
            complete = true;
            giant.AnimPlayer.AnimationFinished -= AnimationComplete;
            giant.AgroMeter.Empty();
        }

    }
}