using System;
using System.Collections.Generic;
using Godot;

public abstract class SpecialFace
{
    public int FaceID { get; protected set; }

    public Vector3[] WorldSpaceFaceVerts { get; protected set; }

    public Vector3 FacePoint { get; protected set; }
    public Vector3 FaceNormal { get; protected set; }

    public Vector3 Up { get; protected set; }
    public Vector3 Right { get; protected set; }
    public Vector3 Forward { get; protected set; }

    public Vector3 BarycentricCoords { get; protected set; }
    public ClimbableEntity ClimbableEntity { get; protected set; }

    public abstract void Update();

    protected abstract void FlagForStateChange(Vector3 faceNormal);

    protected abstract void UpdateDirectionalVectors();

    protected abstract bool ValidFaceNormal(Vector3 faceNormal);

    public void UpdateFacePoint(Vector3 newPoint, HashSet<int> ignoreEdges = null, int iterations = 0)
    {
        if (iterations >= 10)
        {
            GD.Print("HIT MAX ITERATIONS");
            return;
        }

        if (ignoreEdges != null)
            GD.Print("EDGES TO IGNORE: " + ignoreEdges.Count);

        Vector3 newPointBCCoords = Utils.Barycentric(newPoint, WorldSpaceFaceVerts[0], WorldSpaceFaceVerts[1], WorldSpaceFaceVerts[2]);

        if ((ignoreEdges == null || !ignoreEdges.Contains(1)) && newPointBCCoords.X < Mathf.Epsilon)
        {
            if (newPointBCCoords.Y < Mathf.Epsilon)
                newPointBCCoords = new Vector3(0.0f, 0.0f, 1.0f);
            else
                newPointBCCoords = Utils.ClosestBaryOnEdge(newPoint, WorldSpaceFaceVerts[1], WorldSpaceFaceVerts[2], Vector3.Up, Vector3.Back);

            GD.Print("CROSSING EDGE 2");
            Vector3 snappedFacePoint = Utils.BaryToWorld(newPointBCCoords, WorldSpaceFaceVerts[0], WorldSpaceFaceVerts[1], WorldSpaceFaceVerts[2]);
            Vector3 remainingTranslation = newPoint - snappedFacePoint;
            CrossingEdge(1, snappedFacePoint, remainingTranslation, ignoreEdges, iterations);
            return;
        }
        else if ((ignoreEdges == null || !ignoreEdges.Contains(2)) && newPointBCCoords.Y < Mathf.Epsilon)
        {
            if (newPointBCCoords.Z < Mathf.Epsilon)
                newPointBCCoords = new Vector3(1.0f, 0.0f, 0.0f);
            else
                newPointBCCoords = Utils.ClosestBaryOnEdge(newPoint, WorldSpaceFaceVerts[2], WorldSpaceFaceVerts[0], Vector3.Back, Vector3.Right);

            GD.Print("CROSSING EDGE 3");
            Vector3 snappedFacePoint = Utils.BaryToWorld(newPointBCCoords, WorldSpaceFaceVerts[0], WorldSpaceFaceVerts[1], WorldSpaceFaceVerts[2]);
            Vector3 remainingTranslation = newPoint - snappedFacePoint;
            CrossingEdge(2, snappedFacePoint, remainingTranslation, ignoreEdges, iterations);
            return;
        }
        else if ((ignoreEdges == null || !ignoreEdges.Contains(0)) && newPointBCCoords.Z < Mathf.Epsilon)
        {
            if (newPointBCCoords.X < Mathf.Epsilon)
                newPointBCCoords = new Vector3(0.0f, 1.0f, 0.0f);
            else
                newPointBCCoords = Utils.ClosestBaryOnEdge(newPoint, WorldSpaceFaceVerts[0], WorldSpaceFaceVerts[1], Vector3.Right, Vector3.Up);

            GD.Print("CROSSING EDGE 1");
            Vector3 snappedFacePoint = Utils.BaryToWorld(newPointBCCoords, WorldSpaceFaceVerts[0], WorldSpaceFaceVerts[1], WorldSpaceFaceVerts[2]);
            Vector3 remainingTranslation = newPoint - snappedFacePoint;
            CrossingEdge(0, snappedFacePoint, remainingTranslation, ignoreEdges, iterations);
            return;
        }

        BarycentricCoords = newPointBCCoords;
        FacePoint = Utils.BaryToWorld(BarycentricCoords, WorldSpaceFaceVerts[0], WorldSpaceFaceVerts[1], WorldSpaceFaceVerts[2]);
    }

    
    public Vector3 GetFacePoint(Vector3 newPoint, Vector3 oldPoint, HashSet<int> ignoreEdges = null, int iterations = 0)
    {
        if (iterations >= 3)
        {
            GD.Print("HIT MAX ITERATIONS");
            return oldPoint;
        }

        if (ignoreEdges != null)
            GD.Print("EDGES TO IGNORE: " + ignoreEdges.Count);

        Vector3 newPointBCCoords = Utils.Barycentric(newPoint, WorldSpaceFaceVerts[0], WorldSpaceFaceVerts[1], WorldSpaceFaceVerts[2]);

        if ((ignoreEdges == null || !ignoreEdges.Contains(1)) && newPointBCCoords.X < Mathf.Epsilon)
        {
            if (newPointBCCoords.Y < Mathf.Epsilon)
                newPointBCCoords = new Vector3(0.0f, 0.0f, 1.0f);
            else
                newPointBCCoords = Utils.ClosestBaryOnEdge(newPoint, WorldSpaceFaceVerts[1], WorldSpaceFaceVerts[2], Vector3.Up, Vector3.Back);

            GD.Print("CROSSING EDGE 2");
            Vector3 snappedFacePoint = Utils.BaryToWorld(newPointBCCoords, WorldSpaceFaceVerts[0], WorldSpaceFaceVerts[1], WorldSpaceFaceVerts[2]);
            Vector3 remainingTranslation = newPoint - snappedFacePoint;
            return CrossingEdgeToGetPoint(1, snappedFacePoint, remainingTranslation, ignoreEdges, iterations);
        }
        else if ((ignoreEdges == null || !ignoreEdges.Contains(2)) && newPointBCCoords.Y < Mathf.Epsilon)
        {
            if (newPointBCCoords.Z < Mathf.Epsilon)
                newPointBCCoords = new Vector3(1.0f, 0.0f, 0.0f);
            else
                newPointBCCoords = Utils.ClosestBaryOnEdge(newPoint, WorldSpaceFaceVerts[2], WorldSpaceFaceVerts[0], Vector3.Back, Vector3.Right);

            GD.Print("CROSSING EDGE 3");
            Vector3 snappedFacePoint = Utils.BaryToWorld(newPointBCCoords, WorldSpaceFaceVerts[0], WorldSpaceFaceVerts[1], WorldSpaceFaceVerts[2]);
            Vector3 remainingTranslation = newPoint - snappedFacePoint;
            return CrossingEdgeToGetPoint(2, snappedFacePoint, remainingTranslation, ignoreEdges, iterations);
        }
        else if ((ignoreEdges == null || !ignoreEdges.Contains(0)) && newPointBCCoords.Z < Mathf.Epsilon)
        {
            if (newPointBCCoords.X < Mathf.Epsilon)
                newPointBCCoords = new Vector3(0.0f, 1.0f, 0.0f);
            else
                newPointBCCoords = Utils.ClosestBaryOnEdge(newPoint, WorldSpaceFaceVerts[0], WorldSpaceFaceVerts[1], Vector3.Right, Vector3.Up);

            GD.Print("CROSSING EDGE 1");
            Vector3 snappedFacePoint = Utils.BaryToWorld(newPointBCCoords, WorldSpaceFaceVerts[0], WorldSpaceFaceVerts[1], WorldSpaceFaceVerts[2]);
            Vector3 remainingTranslation = newPoint - snappedFacePoint;
            return CrossingEdgeToGetPoint(0, snappedFacePoint, remainingTranslation, ignoreEdges, iterations);
        }

        return Utils.BaryToWorld(newPointBCCoords, WorldSpaceFaceVerts[0], WorldSpaceFaceVerts[1], WorldSpaceFaceVerts[2]);
    }

    private void CrossingEdge(int edge, Vector3 pointOnEdge, Vector3 remainingTranslation, HashSet<int> ignoreEdges, int iterations)
    {
        int newFace = GetNewFaceThroughSharedEdge(edge);

        if (newFace == -1)
            RideEdge(edge, pointOnEdge, remainingTranslation, ignoreEdges, iterations);
        else
        {
            Vector3 newFaceNormal = ClimbableEntity.GetWorldFaceNormal(newFace);
            Vector3 newTranslation;

            GD.Print("OLD FACE: " + FaceID + "|NEW FACE: " + newFace);

            if (newFaceNormal.Dot(FaceNormal) < Mathf.Epsilon)
            {
                RideEdge(edge, pointOnEdge, remainingTranslation, ignoreEdges, iterations);
                return;
            }
            else if (!ValidFaceNormal(newFaceNormal))
            {
                GD.Print("THIS WAS THE NORMAL: " + newFaceNormal);
                FlagForStateChange(newFaceNormal);
                RideEdge(edge, pointOnEdge, remainingTranslation, ignoreEdges, iterations);
                return;
            }

            GD.Print("OLD NORMAL: " + FaceNormal + "|NEW NORMAL: " + newFaceNormal);

            newTranslation = Utils.ProjectOntoPlane(remainingTranslation, newFaceNormal);

            GD.Print("OLD TRANSLATION: " + remainingTranslation + "|NEW TRANSLATION: " + newTranslation);

            if (ignoreEdges != null)
            {
                int edgePassed = ClimbableEntity.GetFaceEdge(FaceID, edge);

                if (ClimbableEntity.GetEdgeFaces(edgePassed).Length > 1)
                {
                    int newIgnoreEdgeIndex = GetNewEdgeIgnoreIndex(newFace, edgePassed);
                    ignoreEdges.Clear();
                    ignoreEdges.Add(newIgnoreEdgeIndex);
                }
                else if (ClimbableEntity.CounterpartEdges.ContainsKey(edgePassed))
                {
                    int counterPartEdge = ClimbableEntity.CounterpartEdges[edgePassed];
                    int newIgnoreEdgeIndex = GetNewEdgeIgnoreIndex(newFace, counterPartEdge);
                    ignoreEdges.Clear();
                    ignoreEdges.Add(newIgnoreEdgeIndex);
                }
                else
                    ignoreEdges = null;
            }

            EstablishNewFace(newFace, pointOnEdge, newFaceNormal);
            UpdateFacePoint(pointOnEdge + newTranslation, ignoreEdges, iterations + 1);
        }
    }

    private Vector3 CrossingEdgeToGetPoint(int edge, Vector3 pointOnEdge, Vector3 remainingTranslation, HashSet<int> ignoreEdges, int iterations)
    {
        int newFace = GetNewFaceThroughSharedEdge(edge);

        if (newFace == -1)
            return RideEdgePoint(edge, pointOnEdge, remainingTranslation, ignoreEdges, iterations);
        else
        {
            Vector3 newFaceNormal = ClimbableEntity.GetWorldFaceNormal(newFace);
            Vector3 newTranslation;

            GD.Print("OLD FACE: " + FaceID + "|NEW FACE: " + newFace);

            if (newFaceNormal.Dot(FaceNormal) < 1.0f - Mathf.Epsilon)
                return RideEdgePoint(edge, pointOnEdge, remainingTranslation, ignoreEdges, iterations);

            GD.Print("OLD NORMAL: " + FaceNormal + "|NEW NORMAL: " + newFaceNormal);

            newTranslation = Utils.ProjectOntoPlane(remainingTranslation, newFaceNormal);

            GD.Print("OLD TRANSLATION: " + remainingTranslation + "|NEW TRANSLATION: " + newTranslation);

            if (ignoreEdges != null)
            {
                int edgePassed = ClimbableEntity.GetFaceEdge(FaceID, edge);

                if (ClimbableEntity.GetEdgeFaces(edgePassed).Length > 1)
                {
                    int newIgnoreEdgeIndex = GetNewEdgeIgnoreIndex(newFace, edgePassed);
                    ignoreEdges.Clear();
                    ignoreEdges.Add(newIgnoreEdgeIndex);
                }
                else if (ClimbableEntity.CounterpartEdges.ContainsKey(edgePassed))
                {
                    int counterPartEdge = ClimbableEntity.CounterpartEdges[edgePassed];
                    int newIgnoreEdgeIndex = GetNewEdgeIgnoreIndex(newFace, counterPartEdge);
                    ignoreEdges.Clear();
                    ignoreEdges.Add(newIgnoreEdgeIndex);
                }
                else
                    ignoreEdges = null;
            }

            return CreateClimbFace(newFace, ClimbableEntity, pointOnEdge, newFaceNormal).GetFacePoint(pointOnEdge + newTranslation, pointOnEdge, ignoreEdges, iterations + 1);
        }
    }

    private void RideEdge(int edge, Vector3 pointOnEdge, Vector3 remainingTranslation, HashSet<int> ignoreEdges, int iterations)
    {
        Vector3 edgeVector = WorldSpaceFaceVerts[(edge + 1) % 3] - WorldSpaceFaceVerts[edge];
        Vector3 edgeDir = edgeVector.Normalized();
        Vector3 newTranslation = Utils.ProjectOntoLine(remainingTranslation, edgeDir);

        GD.Print("NO NEIGBHOR FACE, SNAPPING TO EDGE");
        GD.Print("REMAINING TRANSLATION: " + remainingTranslation);
        GD.Print("NEW TRANSLATION: " + newTranslation);

        Vector3 newPoint = pointOnEdge + newTranslation;

        if ((newPoint - WorldSpaceFaceVerts[edge]).Normalized().Dot(edgeDir) < 0.0f)
            newPoint = WorldSpaceFaceVerts[edge];
        else if ((newPoint - WorldSpaceFaceVerts[edge]).Length() > edgeVector.Length())
            newPoint = WorldSpaceFaceVerts[(edge + 1) % 3];

        if (ignoreEdges == null)
            ignoreEdges = new HashSet<int>();

        ignoreEdges.Add(edge);

        UpdateFacePoint(newPoint, ignoreEdges, iterations + 1);
        return;
    }

    private Vector3 RideEdgePoint(int edge, Vector3 pointOnEdge, Vector3 remainingTranslation, HashSet<int> ignoreEdges, int iterations)
    {
        Vector3 edgeVector = WorldSpaceFaceVerts[(edge + 1) % 3] - WorldSpaceFaceVerts[edge];
        Vector3 edgeDir = edgeVector.Normalized();
        Vector3 newTranslation = Utils.ProjectOntoLine(remainingTranslation, edgeDir);

        GD.Print("NO NEIGBHOR FACE, SNAPPING TO EDGE");
        GD.Print("REMAINING TRANSLATION: " + remainingTranslation);
        GD.Print("NEW TRANSLATION: " + newTranslation);

        Vector3 newPoint = pointOnEdge + newTranslation;

        if ((newPoint - WorldSpaceFaceVerts[edge]).Normalized().Dot(edgeDir) < 0.0f)
            newPoint = WorldSpaceFaceVerts[edge];
        else if ((newPoint - WorldSpaceFaceVerts[edge]).Length() > edgeVector.Length())
            newPoint = WorldSpaceFaceVerts[(edge + 1) % 3];

        if (ignoreEdges == null)
            ignoreEdges = new HashSet<int>();

        ignoreEdges.Add(edge);

        return GetFacePoint(newPoint, pointOnEdge, ignoreEdges, iterations + 1);
    }

    protected int GetNewFaceThroughSharedEdge(int edge)
    {
        int edgePassed = ClimbableEntity.GetFaceEdge(FaceID, edge);
        int[] edgeFaces = ClimbableEntity.GetEdgeFaces(edgePassed);

        int newFace = -1;

        if (edgeFaces.Length == 1 && ClimbableEntity.EdgeFaceDicitonary.ContainsKey(edgePassed))
            newFace = ClimbableEntity.EdgeFaceDicitonary[edgePassed];
        else
        {
            foreach (int face in edgeFaces)
            {
                if (face != FaceID)
                    newFace = face;
            }
        }

        return newFace;
    }

    protected int GetNewEdgeIgnoreIndex(int faceId, int edgeId)
    {
        for (int i = 0; i < 3; i++)
        {
            if (edgeId == ClimbableEntity.GetFaceEdge(faceId, i))
                return i;
        }

        return -1;
    }

    protected void EstablishNewFace(int newFaceId, Vector3 point, Vector3 normal)
    {
        FaceNormal = normal;
        FacePoint = point;
        UpdateDirectionalVectors();

        FaceID = newFaceId;

        WorldSpaceFaceVerts = new Vector3[]
        {
            ClimbableEntity.GetWorldFaceVertexPosition(FaceID, 0),
            ClimbableEntity.GetWorldFaceVertexPosition(FaceID, 1),
            ClimbableEntity.GetWorldFaceVertexPosition(FaceID, 2)
        };
    }

    protected static ClimbFace CreateClimbFace(int newFaceId, ClimbableEntity entity, Vector3 point, Vector3 normal)
    {
        ClimbFace face = new ClimbFace();
        face.ClimbableEntity = entity;
        face.FaceNormal = normal;
        face.FacePoint = point;
        face.UpdateDirectionalVectors();

        face.FaceID = newFaceId;

        face.WorldSpaceFaceVerts = new Vector3[]
        {
            entity.GetWorldFaceVertexPosition(face.FaceID, 0),
            entity.GetWorldFaceVertexPosition(face.FaceID, 1),
            entity.GetWorldFaceVertexPosition(face.FaceID, 2)
        };

        return face;
    }
}