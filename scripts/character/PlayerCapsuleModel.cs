using Godot;

public partial class PlayerCapsuleModel : Node3D
{
    private MeshInstance3D meshInstance;
    private StandardMaterial3D material;

    public override void _Ready()
    {
        base._Ready();
        meshInstance = (MeshInstance3D)GetNode("MeshInstance3D");
        material = (StandardMaterial3D)meshInstance.GetSurfaceOverrideMaterial(0);
    }

    public void Update(CharacterData characterData)
    {
        if (!characterData.Debug)
        {
            Visible = false;
            return;
        }
        
        Visible = true;

        if (characterData.HealthMeter.Value <= 0.0f)
            material.AlbedoColor = Colors.Magenta;

        material.AlbedoColor = new Color(material.AlbedoColor.R, material.AlbedoColor.G, material.AlbedoColor.B, 0.5f);
    }
}
