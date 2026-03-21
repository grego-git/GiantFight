using System.Collections.Generic;
using Godot;

public static class Utils
{
    public static float LineT(Vector3 p, Vector3 x, Vector3 y)
    {
        return (p - x).Dot(y - x) / (y - x).Dot(y - x);
    }

    public static Vector3 Barycentric(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 v0 = b - a;  // AB
        Vector3 v1 = c - a;  // AC
        Vector3 v2 = p - a;  // AP

        float d00 = v0.Dot(v0);  // |AB|^2
        float d01 = v0.Dot(v1);  // AB·AC
        float d11 = v1.Dot(v1);  // |AC|^2
        float d20 = v2.Dot(v0);  // AP·AB
        float d21 = v2.Dot(v1);  // AP·AC

        float denom = d00 * d11 - d01 * d01;

        // Protect against degenerate triangles
        if (Mathf.Abs(denom) < Mathf.Epsilon)
            return new Vector3(-1, -1, -1); // invalid

        float v = (d11 * d20 - d01 * d21) / denom;
        float w = (d00 * d21 - d01 * d20) / denom;
        float u = 1.0f - v - w;

        return new Vector3(u, v, w);
    }

    public static Vector3 ClosestPointOnSegment(Vector3 p, Vector3 x, Vector3 y)
    {
        return x + Mathf.Clamp(LineT(p, x, y), 0.0f, 1.0f) * (y - x);
    }

    public static Vector3 ClosestBaryOnEdge(Vector3 p, Vector3 x, Vector3 y, Vector3 baryX, Vector3 baryY)
    {
        float t = Mathf.Clamp(LineT(p, x, y), 0.0f, 1.0f);
        return baryX * (1 - t) + baryY * t;
    }

    public static Vector3 LineTToWorld(float t, Vector3 x, Vector3 y)
    {
        return x + ((y - x) * t);
    }

    public static Vector3 BaryToWorld(Vector3 bary, Vector3 a, Vector3 b, Vector3 c)
    {
        return (bary.X * a) + (bary.Y * b) + (bary.Z * c);
    }

    public static Vector3 ProjectOntoLine(Vector3 v, Vector3 lineDir)
    {
        // PERPENDICULAR CASE
        if (Mathf.Abs(v.Normalized().Dot(lineDir)) < Mathf.Epsilon)
            return Vector3.Zero;

        return lineDir * lineDir.Dot(v);
    }

    public static Vector3 ProjectOntoPlane(Vector3 v, Vector3 planeNormal)
    {
        return v - planeNormal * v.Dot(planeNormal);
    }

    public static void UpdateAnimatedEntiyFaces(MeshDataTool meshDataTool, Skeleton3D skeleton, Skin skin, AnimatedMeshWorldTriangle[] faces, float timeStamp, HashSet<int> specificFaces = null)
    {
        if (!skeleton.IsInsideTree())
            return;

        for (int i = 0; i < meshDataTool.GetFaceCount(); i++)
        {
            if (specificFaces != null && !specificFaces.Contains(i))
                continue;

            faces[i] = faces[i] ?? new AnimatedMeshWorldTriangle();
            faces[i].Vertices = new Vector3[3];
            faces[i].WorldVertices = new Vector3[3];
            faces[i].InfluenceBones = new System.Collections.Generic.HashSet<string>();

            for (int j = 0; j < 3; j++)
            {
                int vertIndex = meshDataTool.GetFaceVertex(i, j);

                Vector3 vertexPosition = meshDataTool.GetVertex(vertIndex);
                Vector3 vertexTranslation = Vector3.Zero;

                int[] bones = meshDataTool.GetVertexBones(vertIndex);
                float[] weights = meshDataTool.GetVertexWeights(vertIndex);

                for (int k = 0; k < bones.Length; k++)
                {
                    float weight = weights[k];

                    if (weight == 0)
                        continue;

                    string boneName = skeleton.GetBoneName(bones[k]);

                    if (!faces[i].InfluenceBones.Contains(boneName))
                        faces[i].InfluenceBones.Add(boneName);

                    Transform3D boneTransform = skeleton.GetBoneGlobalPose(bones[k]);
                    Transform3D inverseBindTransform = skin.GetBindPose(bones[k]);

                    vertexTranslation += ((boneTransform * inverseBindTransform * vertexPosition) - vertexPosition) * weight;
                }

                vertexPosition += vertexTranslation;

                Vector3 worldVertexPosition = skeleton.GlobalTransform * vertexPosition;
                faces[i].Vertices[j] = vertexPosition;
                faces[i].WorldVertices[j] = worldVertexPosition;
            }

            Vector3 edge1 = (faces[i].WorldVertices[1] - faces[i].WorldVertices[0]).Normalized();
            Vector3 edge2 = (faces[i].WorldVertices[2] - faces[i].WorldVertices[1]).Normalized();

            faces[i].Normal = edge2.Cross(edge1).Normalized();
            faces[i].Center = Utils.BaryToWorld(new Vector3(1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 3.0f), faces[i].WorldVertices[0], faces[i].WorldVertices[1], faces[i].WorldVertices[2]);
            faces[i].FaceID = i;
            faces[i].LastTimeStamp = timeStamp;
        }
    }

    public static void DrawAnimatedDebugMesh(MeshInstance3D debugMesh, List<int> drawFaces, AnimatedMeshWorldTriangle[] meshFaces)
    {
        ImmediateMesh mesh = (ImmediateMesh)debugMesh.Mesh;
        Transform3D inverseMesh = debugMesh.GlobalTransform.Inverse();

        mesh.ClearSurfaces();
        mesh.SurfaceBegin(Mesh.PrimitiveType.Lines);

        foreach (var face in drawFaces)
        {
            mesh.SurfaceSetColor(Colors.Blue);
            mesh.SurfaceAddVertex(inverseMesh * meshFaces[face].WorldVertices[0]);
            mesh.SurfaceAddVertex(inverseMesh * meshFaces[face].WorldVertices[1]);
            mesh.SurfaceAddVertex(inverseMesh * meshFaces[face].WorldVertices[1]);
            mesh.SurfaceAddVertex(inverseMesh * meshFaces[face].WorldVertices[2]);
            mesh.SurfaceAddVertex(inverseMesh * meshFaces[face].WorldVertices[2]);
            mesh.SurfaceAddVertex(inverseMesh * meshFaces[face].WorldVertices[0]);

            mesh.SurfaceSetColor(Colors.Green);
            mesh.SurfaceAddVertex(inverseMesh * meshFaces[face].Center);
            mesh.SurfaceAddVertex(inverseMesh * (meshFaces[face].Center + meshFaces[face].Normal));
        }

        mesh.SurfaceEnd();
    }

    public static Vector3 GetFlatDirectionalVector(Vector3 vector)
    {
        vector.Y = 0.0f;
        return vector.Normalized();
    }

    public static Vector3 GetFlatSpatialVector(Vector3 vector, float y = 0.0f)
    {
        vector.Y = y;
        return vector;
    }
}