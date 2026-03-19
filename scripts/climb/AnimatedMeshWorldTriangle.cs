using System.Collections.Generic;
using Godot;

public class AnimatedMeshWorldTriangle
{
    public int FaceID { get; set; }
    public Vector3[] Vertices { get; set; }
    public Vector3[] WorldVertices { get; set; }
    public Vector3 Normal { get; set; }
    public Vector3 Center { get; set; }
    public HashSet<string> InfluenceBones { get; set; }
    public float LastTimeStamp { get; set; }
}