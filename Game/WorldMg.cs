using Godot;
using System;

public partial class WorldMg : Node
{
    public Node3D GetWorld()
    {
        return GD.Load<PackedScene>("user://Worlds/" + Game.CurrentWorld + ".tscn").Instantiate<Node3D>(); 
    }
}
