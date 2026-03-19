using Godot;

public class ClimbFace : SpecialFace
{
    public bool ChangeToCrawl { get; set; }
    public bool ChangeToHang { get; set; }

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
        if (faceNormal.Dot(Vector3.Up) > 0.0f)
            ChangeToCrawl = true;
        else
            ChangeToHang = true;
    }

    protected override void UpdateDirectionalVectors()
    {
        Forward = FaceNormal;
        Right = Vector3.Up.Cross(Forward).Normalized();
        Up = Forward.Cross(Right).Normalized();
    }

    protected override bool ValidFaceNormal(Vector3 faceNormal)
    {
        return ValidClimbFaceNormal(faceNormal);
    }

    public static ClimbFace CreateClimbFaceFromRayCast(RayCast3D climbRayCast)
    {
        ClimbFace result = new ClimbFace();
        result.ClimbableEntity = (ClimbableEntity)climbRayCast.GetCollider();
        result.EstablishNewFace(result.ClimbableEntity.GetActualCollisionFaceID(climbRayCast.GetCollisionFaceIndex()), climbRayCast.GetCollisionPoint(), climbRayCast.GetCollisionNormal());
        result.BarycentricCoords = Utils.Barycentric(result.FacePoint, result.WorldSpaceFaceVerts[0], result.WorldSpaceFaceVerts[1], result.WorldSpaceFaceVerts[2]);

        return result;
    }

    public static ClimbFace CreateClimbFaceFromCrawlOrHangFace(SpecialFace face)
    {
        ClimbFace result = new ClimbFace();
        result.ClimbableEntity = face.ClimbableEntity;
        result.EstablishNewFace(face.FaceID, face.FacePoint, face.FaceNormal);
        result.BarycentricCoords = Utils.Barycentric(result.FacePoint, result.WorldSpaceFaceVerts[0], result.WorldSpaceFaceVerts[1], result.WorldSpaceFaceVerts[2]);

        return result;
    }

    public static bool ValidClimbFaceNormal(Vector3 normal)
    {
        if (normal.Length() < Mathf.Epsilon)
            return false;

        return Mathf.Abs(normal.Dot(Vector3.Up)) <= 0.85f;
    }
}