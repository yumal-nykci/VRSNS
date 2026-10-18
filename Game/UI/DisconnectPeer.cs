using Godot;
using System;

public partial class DisconnectPeer : VBoxContainer
{
    [Export]
    Button button;
    public override void _Ready()
    {
        button.Pressed += () =>
        {
            NetworkMG.Instance.DisconnectPeer();
        };
    }
    
}
