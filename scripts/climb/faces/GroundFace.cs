using Godot;

public class GroundedFace
{
    public ClimbableAnimatedEntity Climbable { get; set; }
    public Vector3[] WorldSpaceFaceVerts { get; set; }
    public Vector3 FaceNormal { get; set; }
    public Vector3 FacePoint { get; set; }
    public Vector3 BarycentricCoords { get; set; }
    public int FaceID { get; set; }

    public GroundedFace(ClimbableAnimatedEntity climbable, Vector3 facePoint, int faceId)
    {
        Climbable = climbable;
        FaceID = faceId;

        UpdateFacePoint(facePoint);
    }

    public void Update()
    {
        WorldSpaceFaceVerts = new Vector3[]
        {
            Climbable.GetWorldFaceVertexPosition(FaceID, 0),
            Climbable.GetWorldFaceVertexPosition(FaceID, 2),
            Climbable.GetWorldFaceVertexPosition(FaceID, 1)
        };

        FaceNormal = Climbable.GetWorldFaceNormal(FaceID);
        FacePoint = Utils.BaryToWorld(BarycentricCoords, WorldSpaceFaceVerts[0], WorldSpaceFaceVerts[1], WorldSpaceFaceVerts[2]);
    }

    public void UpdateFacePoint(Vector3 facePoint)
    {
        WorldSpaceFaceVerts = new Vector3[]
        {
            Climbable.GetWorldFaceVertexPosition(FaceID, 0),
            Climbable.GetWorldFaceVertexPosition(FaceID, 2),
            Climbable.GetWorldFaceVertexPosition(FaceID, 1)
        };

        FaceNormal = Climbable.GetWorldFaceNormal(FaceID);
        FacePoint = facePoint;
        BarycentricCoords = Utils.Barycentric(FacePoint, WorldSpaceFaceVerts[0], WorldSpaceFaceVerts[1], WorldSpaceFaceVerts[2]);
    }
}