using System.Collections.Generic;
using Godot;

public partial class ClimbableAnimatedEntity : ClimbableEntity
{
    [Export]
    public SkeletonIK3D[] IKs { get; set; }
    public Skeleton3D Skeleton { get; private set; }
    public AnimationPlayer AnimationPlayer { get; private set; }
    public Node3D EntityRootNode { get; private set; }

    private List<int> collidableFaces;
    private HashSet<int> memoizedFaces;
    private AnimatedMeshWorldTriangle[] faces;
    private MeshInstance3D debugMesh;
    private ConcavePolygonShape3D shape;

    private float timeSinceCreated;
    private int iksProcessed;
    private bool destroyed;

    public override void _Ready()
    {
        base._Ready();

        Skeleton = (Skeleton3D)GetNode("../../");
        Skeleton.SkeletonUpdated += Update;
        
        if (IKs != null)
        {
            foreach (var ik in IKs)
                ik.ModificationProcessed += Update;
        }

        faces = new AnimatedMeshWorldTriangle[meshDataTool.GetFaceCount()];
        debugMesh = (MeshInstance3D)GetNode("WireFrame");

        var collisionShape = (CollisionShape3D)GetNode("CollisionShape3D");
        shape = (ConcavePolygonShape3D)collisionShape.Shape;
        shape.SetFaces([]);

        EntityRootNode = (Node3D)Skeleton.GetNode("../../");
        AnimationPlayer = (AnimationPlayer)EntityRootNode.GetNode("AnimationPlayer");

        debugMesh.Scale = new Vector3(1.0f / EntityRootNode.Scale.X, 1.0f / EntityRootNode.Scale.Y, 1.0f / EntityRootNode.Scale.Z);

        Utils.UpdateAnimatedEntiyFaces(meshDataTool, Skeleton, MeshInstance.Skin, faces, timeSinceCreated);

        collidableFaces = new List<int>();
        memoizedFaces = new HashSet<int>();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        timeSinceCreated += (float)delta;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }

    public void Update()
    {
        if (destroyed)
            return;
        
        if (IKs != null && IKs.Length > 0) 
        {
            if (iksProcessed < IKs.Length + 1)
                iksProcessed++;

            if (iksProcessed != IKs.Length + 1)
                return;
        }
        
        if (!destroyed && collidableFaces != null && collidableFaces.Count > 0)
        {
            Utils.UpdateAnimatedEntiyFaces(meshDataTool, Skeleton, MeshInstance.Skin, faces, timeSinceCreated, memoizedFaces);
            Vector3[] verts = new Vector3[collidableFaces.Count * 3];

            for (int i = 0; i < collidableFaces.Count; i++)
            {
                verts[i * 3] = faces[collidableFaces[i]].Vertices[0];
                verts[(i * 3) + 1] = faces[collidableFaces[i]].Vertices[1];
                verts[(i * 3) + 2] = faces[collidableFaces[i]].Vertices[2];
            }

            shape.SetFaces(verts);
            Utils.DrawAnimatedDebugMesh(debugMesh, collidableFaces, faces);
        }
        else
        {
            shape.SetFaces([]);

            ImmediateMesh mesh = (ImmediateMesh)debugMesh.Mesh;
            mesh.ClearSurfaces();
        }

        if (CharacterData != null && CharacterData.IsInsideTree())
        {
            if (CharacterData.OnThisEntity(this))
                CharacterData.CurrentCharacterState.UpdatePositionAfterPoseUpdate();
            else
                CharacterData = null;
        }

        iksProcessed = 0;
    }

    public void GetCollidableFaces(List<string> bones)
    {
        if (destroyed)
            return;
        
        foreach (var face in faces)
        {
            foreach (string bone in bones) 
            {
                if (!memoizedFaces.Contains(face.FaceID) && face.InfluenceBones.Contains(bone))
                {
                    collidableFaces.Add(face.FaceID);
                    memoizedFaces.Add(face.FaceID);
                }
            }
        }
    }

    public void ResetCollidableFaces()
    {
        collidableFaces.Clear();
        memoizedFaces.Clear();
    }

    public override void Destroy()
    {
        destroyed = true;
        
        ((Node3D)GetParent()).Visible = false;
    }

    public override Vector3 GetWorldFaceNormal(int faceId)
    {
        return faces[faceId].Normal;
    }

    public override Vector3 GetWorldFaceVertexPosition(int faceId, int vertex)
    {
        return faces[faceId].WorldVertices[vertex];
    }

    public override int GetActualCollisionFaceID(int faceId)
    {
        if (faceId < 0 || faceId >= collidableFaces.Count)
            return -1;

        return collidableFaces[faceId];
    }

    public override bool CanStun(int faceId)
    {
        if (StunBonesPerAnimation == null || AnimationLibrary == null)
            return false;

        if (!StunBonesPerAnimation.ContainsKey(AnimationPlayer.CurrentAnimation.Replace(AnimationLibrary + "/", "")))
            return false;

        AnimatedMeshWorldTriangle face = faces[faceId];
        string[] stunBones = StunBonesPerAnimation[AnimationPlayer.CurrentAnimation.Replace(AnimationLibrary + "/", "")];

        foreach (var stunBone in stunBones)
        {
            if (face.InfluenceBones.Contains(stunBone))
            {
                return true;
            }
        }

        return false;
    }

    public override HashSet<string> GetFaceBones(int faceId)
    {
        return faces[faceId].InfluenceBones;
    }
}