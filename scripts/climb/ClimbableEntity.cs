using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Reflection.Metadata.Ecma335;

public partial class ClimbableEntity : StaticBody3D
{
    [Export]
    public string MaxSwordChargeTriggerAnimation { get; set; }
    [Export]
    public bool CheckForDuplicateVerts { get; set; }
    
    public CharacterData CharacterData { get; set; }
    public Vector3 KnockedBack { get; set; }
    public Vector3 KnockedBackNormal { get; set; }
    public string BoneHit { get; set; }
    public Dictionary<string, string[]> StunBonesPerAnimation { get; set; }
    public string AnimationLibrary { get; set; }
    public MeshInstance3D MeshInstance { get; private set; }
    public Dictionary<int, int> EdgeFaceDicitonary { get; private set; }
    public Dictionary<int, int> CounterpartEdges { get; private set; }

    protected MeshDataTool meshDataTool;

    public override void _Ready()
    {
        MeshInstance = (MeshInstance3D)GetNode("../");

        var arrMesh = new ArrayMesh();
        arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, MeshInstance.Mesh.SurfaceGetArrays(0));

        meshDataTool = new MeshDataTool();
        meshDataTool.CreateFromSurface(arrMesh, 0);

        EdgeFaceDicitonary = new Dictionary<int, int>();
        CounterpartEdges = new Dictionary<int, int>();
        List<int> edgesWithOneFace = new List<int>();

        for (int i = 0; i < meshDataTool.GetEdgeCount(); i++)
        {
            int[] faces = meshDataTool.GetEdgeFaces(i);

            if (faces.Length == 1)
                edgesWithOneFace.Add(i);
        }

        if (CheckForDuplicateVerts)
        {
            int iterations = 0;

            foreach (int edgeId in edgesWithOneFace)
            {
                if (EdgeFaceDicitonary.ContainsKey(edgeId))
                    continue;

                Vector3 p1 = GetEdgeVertexPosition(edgeId, 0);
                Vector3 p2 = GetEdgeVertexPosition(edgeId, 1);

                if (p1 == p2)
                    continue;

                foreach (int otherEdgeId in edgesWithOneFace)
                {
                    if (otherEdgeId == edgeId)
                        continue;

                    Vector3 op1 = GetEdgeVertexPosition(otherEdgeId, 0);
                    Vector3 op2 = GetEdgeVertexPosition(otherEdgeId, 1);

                    if (op1 == op2)
                        continue;

                    iterations++;

                    if (p1.DistanceSquaredTo(op1) < Mathf.Epsilon && p2.DistanceSquaredTo(op2) < Mathf.Epsilon)
                    {
                        EdgeFaceDicitonary.Add(edgeId, meshDataTool.GetEdgeFaces(otherEdgeId)[0]);
                        EdgeFaceDicitonary.Add(otherEdgeId, meshDataTool.GetEdgeFaces(edgeId)[0]);
                        CounterpartEdges.Add(edgeId, otherEdgeId);
                        CounterpartEdges.Add(otherEdgeId, edgeId);
                    }
                }
            }
        }
    }

    protected Vector3 GetEdgeVertexPosition(int edgeId, int vertex)
    {
        return meshDataTool.GetVertex(meshDataTool.GetEdgeVertex(edgeId, vertex));
    }

    public int GetFaceEdge(int faceId, int edge)
    {
        return meshDataTool.GetFaceEdge(faceId, edge);
    }

    public int[] GetEdgeFaces(int faceId)
    {
        return meshDataTool.GetEdgeFaces(faceId);
    }

    public void Stabbed(float damage)
    {
        
    }

    public virtual void Destroy()
    {
        ((Node3D)GetParent()).Visible = false;
    }

    public virtual Vector3 GetWorldFaceNormal(int faceId)
    {
        return (GlobalTransform.Basis * meshDataTool.GetFaceNormal(faceId)).Normalized();
    }

    public virtual Vector3 GetWorldFaceVertexPosition(int faceId, int vertex)
    {
        return GlobalTransform * meshDataTool.GetVertex(meshDataTool.GetFaceVertex(faceId, vertex));
    }

    public virtual int GetActualCollisionFaceID(int faceId)
    {
        return faceId;
    }

    public virtual bool CanStun(int faceId)
    {
        return true;
    }

    public virtual HashSet<string> GetFaceBones(int faceId)
    {
        return null;
    }
}
