using System.Collections.Generic;
using Godot;

public class CharacterStateThrown : ICharacterState
{
    private readonly CharacterData characterData;

    private RandomNumberGenerator rng;
    private Vector3 throwVelocity;
    private float verticalVelocity;
    private float checkForBounce;

    public CharacterStateThrown(CharacterData characterData, Vector3 throwVelocity, float verticalVelocity)
    {
        rng = new RandomNumberGenerator();

        this.characterData = characterData;
        this.throwVelocity = throwVelocity;
        this.verticalVelocity = verticalVelocity;
        characterData.Controller.Velocity = throwVelocity;
        checkForBounce = 0.5f;

        if (characterData.Controller.Velocity.Length() > 100.0f)
            ChangeLookAt();

        GD.Print("ENTERING THROWN STATE");

        if (characterData.GetState() == "CLIMB" || characterData.GetState() == "HANG")
            characterData.Controller.HorizontalReOrient(characterData.CameraController, Vector3.Up, true);

        characterData.Controller.EnteringNewState(GetState());

        characterData.EnterFatigue();
    }

    public ICharacterState ChangeState()
    {
        if (!characterData.IsFatigued)
            return new CharacterStateAir(characterData, throwVelocity, 0.0f, false);

        return null;
    }

    public void Move(Vector2 moveDirection)
    {
        characterData.Controller.Velocity = throwVelocity + (verticalVelocity * Vector3.Up);
        characterData.Controller.MoveAndSlide();

        if (checkForBounce == 0.0f && throwVelocity.Length() > 10.0f)
            Bounce(throwVelocity);
    }

    public void Update(float delta)
    {
        Vector3 lookAt = Vector3.Zero;

        if (characterData.Controller.Velocity.Length() <= 100.0f)
            lookAt = Utils.GetFlatDirectionalVector(characterData.Controller.Velocity);

        if (lookAt == Vector3.Zero)
            lookAt = -characterData.CameraController.CameraUpRotation.GlobalBasis.Z;

        characterData.Controller.LookAt(characterData.Controller.GlobalPosition + lookAt, Vector3.Up);

        throwVelocity = throwVelocity.MoveToward(Vector3.Zero, 5.0f * delta);

        if (!characterData.Controller.IsOnFloor())
            verticalVelocity += CharacterController.GRAVITY * delta;
        else
            verticalVelocity = 0.0f;

        checkForBounce -= delta;
        checkForBounce = Mathf.Max(checkForBounce, 0.0f);
    }

    public bool OnThisEntity(ClimbableEntity entity)
    {
        return false;
    }

    public HashSet<string> GetBoneImOnFromClimbable()
    {
        return null;
    }

    public void EndOfFrame()
    {

    }

    public string GetState()
    {
        return "THROWN";
    }

    public void UpdatePositionAfterPoseUpdate()
    {

    }

    private void ChangeLookAt()
    {
        Vector3 lookAt = Vector3.Forward;
        lookAt = lookAt.Rotated(Vector3.Up, Mathf.DegToRad(rng.RandfRange(0.0f, 360.0f)));

        Vector3 right = Vector3.Up.Cross(lookAt);
        lookAt = lookAt.Rotated(right, Mathf.DegToRad(rng.RandfRange(-60.0f, 60.0f)));

        characterData.Controller.LookAt(characterData.Controller.GlobalPosition + lookAt);
    }

    private void Bounce(Vector3 knockBackVelocity)
    {
        for (int i = 0; i < characterData.Controller.GetSlideCollisionCount(); i++)
        {
            KinematicCollision3D collision = characterData.Controller.GetSlideCollision(i);
            CollisionObject3D collider = (CollisionObject3D)collision.GetCollider(0);

            throwVelocity = collision.GetNormal(i) * knockBackVelocity.Length() * 0.5f;

            if (knockBackVelocity.Length() > 100.0f)
            {
                ChangeLookAt();
            }
            break;
        }
    }
}