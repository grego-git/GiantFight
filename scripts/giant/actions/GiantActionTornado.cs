using Godot;

public class GiantActionTornado : IGiantAction
{
    private Giant giant;
    private string animation;
    private float maxRotSpeed;
    private float rotAccel;
    private float rotSpeed;
    private Vector3 chaseDir;
    private float maxChaseSpeed;
    private float chaseDeccel;
    private float chaseAccel;
    private float chaseSpeed;
    private float chaseTime;
    private bool animationEnd;

    public GiantActionTornado(Giant giant, string animation)
    {
        this.giant = giant;
        this.animation = animation;

        maxRotSpeed = 100.0f;
        rotAccel = 100.0f;

        chaseDir = Vector3.Zero;
        maxChaseSpeed = 100.0f;
        chaseDeccel = 50.0f;
        chaseAccel = 25.0f;
        chaseTime = 15.0f;

        animationEnd = false;
    }

    public bool Complete()
    {
        return animationEnd && rotSpeed == 0.0f && chaseSpeed == 0.0f;
    }

    public void Init()
    {
        giant.AnimPlayer.Play(animation);
        giant.CurrentState = Giant.State.ACTION;
    }

    public void Update(float delta)
    {
        if (giant.TrackPlayer && chaseTime > 0.0f)
        {
            chaseTime -= delta;

            Spin(delta);
            Chase(delta);

            if (chaseTime <= 0.0f)
            {
                giant.AnimPlayer.Play(animation + "_end");
                giant.AnimPlayer.AnimationFinished += AnimationComplete;
            }
        }
        else if (chaseTime <= 0.0f)
        {
            Spin(-delta);     
            chaseSpeed -= chaseAccel * delta;  
            chaseSpeed = Mathf.Clamp(chaseSpeed, 0.0f, maxChaseSpeed);  
            giant.GlobalPosition += chaseDir * chaseSpeed * delta;
        }
    }

    public void AnimationComplete(StringName animation)
    {
        animationEnd = true;
        giant.AnimPlayer.AnimationFinished -= AnimationComplete;
        giant.AgroMeter.Empty();
    }

    private void Spin(float delta)
    {
        rotSpeed += rotAccel * delta;
        rotSpeed = Mathf.Clamp(rotSpeed, 0.0f, maxRotSpeed);

        giant.RotateYRot(delta, rotSpeed);
    }

    private void Chase(float delta)
    {
        Vector3 target = Utils.GetFlatSpatialVector(giant.PlayerDetection.PlayerPosition, giant.GlobalPosition.Y);
        
        if (target.DistanceTo(giant.GlobalPosition) > 75.0f)
        {
            chaseDir = (target - giant.GlobalPosition).Normalized();
            chaseSpeed += chaseAccel * delta;
        }
        else
        {
            if (chaseSpeed > maxChaseSpeed * 0.75f)
            {
                chaseSpeed -= chaseAccel * delta;
                chaseSpeed = Mathf.Max(maxChaseSpeed * 0.75f, chaseSpeed);
            }
            else
            {
                chaseSpeed += chaseDeccel * delta;
            }
        } 

        chaseSpeed = Mathf.Clamp(chaseSpeed, 0.0f, maxChaseSpeed);

        giant.GlobalPosition += chaseDir * chaseSpeed * delta;
    }
}