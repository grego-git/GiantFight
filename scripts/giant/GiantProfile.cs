using System.Collections.Generic;

public class GiantProfile
{
    public Dictionary<string, string[]> ShakeAnimations { get; set; }
    public Dictionary<string, string[]> AttackAnimations { get; set; }
    public Dictionary<string, string[]> StunBones { get; set; }
    public string SlamAnimation { get; set; }
    public string FloorAnimation { get; set; }
    public string MidAnimation { get; set; }
    public string TopAnimation { get; set; }
    public string IdleAnimation { get; set; }
    public string ExternalAttackAnimation { get; set; }
}