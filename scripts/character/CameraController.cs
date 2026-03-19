using Godot;
using System;

public partial class CameraController : Node3D
{
    [Export]
    public Camera3D Camera3D { get; set; }
    [Export]
    public CameraTarget Target { get; set; }
    [Export]
    public RayCast3D TargetToCameraRayCast { get; set; }
    [Export]
    public float PanSpeed { get; set; }
    [Export]
    public float Distance { get; set; }
    [Export]
    public float MaxXRotation { get; set; }
    [Export]
    public float NormalFOV { get; set; }
    [Export]
    public float ChargingFOV { get; set; }
    [Export]
    public float DashFOV { get; set; }
    [Export]
    public float ClimbFOV { get; set; }

    public Basis TargetBasis { get; private set; }
    public Node3D CameraOrientation { get; private set; }
    public Node3D CameraUpRotation { get; private set; }
    public Node3D CameraRightRotation { get; private set; }
    public RayCast3D AimRayCast { get; private set; }

    public bool Lock { get; set; }
    public float ReOrientWeight { get; set; }

    private Vector2 mouseVelocity;
    
    private float shakeStrength;
    private Meter shakeTimer;
    private RandomNumberGenerator rng;
    private Vector3 currentUp;
    private Vector2 rotations;

    public override void _Ready()
    {
        CameraOrientation = this;
        CameraUpRotation = (Node3D)GetNode("CameraUpRotation");
        CameraRightRotation = (Node3D)CameraUpRotation.GetNode("CameraRightRotation");
        AimRayCast = (RayCast3D)CameraRightRotation.GetNode("SlingShotAim");

        TargetToCameraRayCast.TargetPosition = Vector3.Forward * (Distance - 0.5f);
        
        ReOrient(Vector3.Up, -Vector3.Forward, false);

        Camera3D.GlobalTransform = CameraRightRotation.GlobalTransform;

        shakeStrength = 0.0f;
        shakeTimer = new Meter(1.0f);
        rng = new RandomNumberGenerator();

        ReOrientWeight = 1.0f;
        
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Input.IsActionJustPressed("pause"))
        {
            if (!GetTree().Paused)
            {
                GetTree().Paused = true;
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
            else
            {
                GetTree().Paused = false;
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }
        }
    }


    public void Update(float delta, CharacterData characterData)
    {
        if (Lock)
        {
            UpdateFOV(delta, characterData);
            shakeTimer.FillMeter(-(float)delta);
            return;
        }
        
        Target.Update(delta, characterData);
        CameraOrientation.GlobalTransform = new Transform3D(TargetBasis.Orthonormalized(), characterData.GlobalPosition);

        ReOrientWeight += delta;
        ReOrientWeight = Mathf.Clamp(ReOrientWeight, 0.0f, 1.0f);

        UpdateRotations((float)delta);

        CameraUpRotation.GlobalPosition = Target.GlobalPosition;
        RotateCamera();
        ReAdjustCameraPosition((float)delta);
        
        Camera3D.GlobalTransform = Camera3D.GlobalTransform.InterpolateWith(CameraRightRotation.GlobalTransform, ReOrientWeight);

        UpdateFOV(delta, characterData);

        shakeTimer.FillMeter(-(float)delta);
        
        mouseVelocity = Vector2.Zero;
    }

    private void UpdateRotations(float delta)
    {
        Vector2 moveDir = mouseVelocity;

        if (moveDir == Vector2.Zero)
        {
            moveDir = Input.GetVector("right_stick_left", "right_stick_right", "right_stick_up", "right_stick_down");
            moveDir = moveDir * 12.0f;
        }

        rotations += new Vector2(-moveDir.Y / 10.0f * PanSpeed * delta, -moveDir.X / 10.0f * PanSpeed * delta);
        
        if (Mathf.Abs(Mathf.RadToDeg(rotations.X)) >= MaxXRotation)
            rotations.X = Mathf.DegToRad(MaxXRotation) * Mathf.Sign(CameraRightRotation.RotationDegrees.X);
    }

    private void RotateCamera()
    {        
        CameraUpRotation.Rotation = new Vector3(0.0f, rotations.Y, 0.0f);
        CameraRightRotation.Rotation = new Vector3(rotations.X, 0.0f, 0.0f);
    }

    private void ReAdjustCameraPosition(float delta)
    {
        float targetDistance = Distance;
        Vector3 desiredPos = CameraUpRotation.GlobalPosition + (CameraRightRotation.GlobalBasis.Z * Distance);

        TargetToCameraRayCast.GlobalPosition = Target.GlobalPosition;
        TargetToCameraRayCast.LookAt(desiredPos, Vector3.Up);
        TargetToCameraRayCast.GlobalPosition -= TargetToCameraRayCast.GlobalBasis.Z * 0.5f;
        TargetToCameraRayCast.ForceRaycastUpdate();

        if (TargetToCameraRayCast.IsColliding())
            targetDistance = TargetToCameraRayCast.GetCollisionPoint().DistanceTo(CameraUpRotation.GlobalPosition);

        CameraUpRotation.GlobalPosition += (CameraRightRotation.GlobalBasis.Z * targetDistance) - (CameraRightRotation.GlobalBasis.Z * 0.5f);
        CameraRightRotation.Position = (Vector3.Up * rng.RandfRange(-0.25f, 0.25f) + Vector3.Right * rng.RandfRange(-0.25f, 0.25f)) * shakeTimer.NormalizedFill() * shakeStrength;
    }

    private void UpdateFOV(float delta, CharacterData characterData)
    {
        float targetFOV = NormalFOV;

        if (characterData.IsDashing())
        {
            float weight = Mathf.Min(characterData.DashMeter.NormalizedFill() * 2.0f, 1.0f);
            targetFOV = Mathf.Lerp(NormalFOV, DashFOV, weight);
        }
        else if (characterData.GetState() == "CLIMB" || characterData.GetState() == "CRAWL" || characterData.GetState() == "HANG" || characterData.GetState() == "GRABBED")
        {
            targetFOV = ClimbFOV;
        }

        Camera3D.Fov = Mathf.MoveToward(Camera3D.Fov, targetFOV, 100.0f * delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion eventMouseMotion)
            mouseVelocity = eventMouseMotion.Relative;
    }

    public void Shake(float time, float strength = 1.0f)
    {
        shakeTimer = new Meter(time, fill: true);
        shakeStrength = strength;
    }

    public void ReOrient(Vector3 up, Vector3 forward, bool adjustYRot)
    {
        if (adjustYRot)
        {
            float dot = up.Dot(Vector3.Up);
            
            if (dot > 0.975f)
            {
                float angleToRefForward = TargetBasis.Y.SignedAngleTo(forward, up);

                rotations.Y -= angleToRefForward;
            }
            else if (dot < -0.975f)
            {
                float angleToRefForward = TargetBasis.Y.SignedAngleTo(forward, up);

                rotations.Y -= Mathf.Pi + angleToRefForward;
            }
            else
            {
                float signedAngleToFaceWall = TargetBasis.Z.SignedAngleTo(up, forward);

                if (Mathf.Sign(TargetBasis.Y.Dot(Vector3.Up)) == 1.0f)
                    rotations.Y += signedAngleToFaceWall;
                else
                    rotations.Y += Mathf.Pi - signedAngleToFaceWall;
            }
            
            rotations.X  = 0.0f;
        }

        Vector3 right = up.Cross(forward);

        TargetBasis = new Basis(right, up, forward);
        ReOrientWeight = 0.0f;
    }
}
