using Godot;
using System;

public partial class ClientJoin : VBoxContainer
{

    [Export]
    LineEdit address;
    [Export]
    LineEdit port;
    [Export]
    Button button;
    public override void _Ready()
    {
        button.Pressed += () =>
        {
            NetworkMG.Instance.StartClient(address.Text,port.Text.ToInt());
        };
    }

}
