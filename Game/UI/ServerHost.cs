using Godot;
using System;

public partial class ServerHost : VBoxContainer
{
    [Export]
    LineEdit port;
    [Export]
    LineEdit world;
    [Export]
    Button button;
    public override void _Ready()
    {
        button.Pressed += () =>
        {
            Game.CurrentWorld = world.Text;
            NetworkMG.Instance.StartHost(port.Text.ToInt());
        };
    }
}
