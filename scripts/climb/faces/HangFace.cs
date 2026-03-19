using Godot;

public class HangFace : SpecialFace
{
    public bool ChangeToClimb { get; set; }

    private CameraController cameraController;

    public override void Update()
    {
        WorldSpaceFaceVerts = new Vector3[]
        {
            ClimbableEntity.GetWorldFaceVertexPosition(FaceID, 0),
            ClimbableEntity.GetWorldFaceVertexPosition(FaceID, 1),
            ClimbableEntity.GetWorldFaceVertexPosition(FaceID, 2)
        };

        FaceNormal = ClimbableEntity.GetWorldFaceNormal(FaceID);
        FacePoint = Utils.BaryToWorld(BarycentricCoords, WorldSpaceFaceVerts[0], WorldSpaceFaceVerts[1], WorldSpaceFaceVerts[2]);
        UpdateDirectionalVectors();
    }

    protected override void FlagForStateChange(Vector3 faceNormal)
    {
        if (faceNormal.Length() >= Mathf.Epsilon)
            ChangeToClimb = true;
    }

    protected override void UpdateDirectionalVectors()
    {
        Up = -FaceNormal;
        Right = Up.Cross(cameraController.CameraUpRotation.GlobalBasis.Z).Normalized();
        Forward = Up.Cross(Right).Normalized();
    }

    protected override bool ValidFaceNormal(Vector3 faceNormal)
    {
        return ValidHangFaceNormal(faceNormal);
    }

    public static HangFace CreateHangFaceFromRayCast(RayCast3D climbRayCast, CameraController cameraController)
    {
        HangFace result = new HangFace();
        result.cameraController = cameraController;
        result.ClimbableEntity = (ClimbableEntity)climbRayCast.GetCollider();
        result.EstablishNewFace(result.ClimbableEntity.GetActualCollisionFaceID(climbRayCast.GetCollisionFaceIndex()), climbRayCast.GetCollisionPoint(), climbRayCast.GetCollisionNormal());
        result.BarycentricCoords = Utils.Barycentric(result.FacePoint, result.WorldSpaceFaceVerts[0], result.WorldSpaceFaceVerts[1], result.WorldSpaceFaceVerts[2]);

        return result;
    }

    public static HangFace CreateHangFaceFromClimbFace(ClimbFace climbFace, CameraController cameraController)
    {
        HangFace result = new HangFace();
        result.cameraController = cameraController;
        result.ClimbableEntity = climbFace.ClimbableEntity;
        result.EstablishNewFace(climbFace.FaceID, climbFace.FacePoint, climbFace.FaceNormal);
        result.BarycentricCoords = Utils.Barycentric(result.FacePoint, result.WorldSpaceFaceVerts[0], result.WorldSpaceFaceVerts[1], result.WorldSpaceFaceVerts[2]);

        return result;
    }

    public static bool ValidHangFaceNormal(Vector3 normal)
    {
        if (normal.Length() < Mathf.Epsilon)
            return false;

        return normal.Dot(Vector3.Up) <= -0.5f;
    }
}