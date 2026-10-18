using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class Menu : Control
{
    [Export]
    VBoxContainer connectui;

    [Export]
    VBoxContainer disconnectui;
    public override void _Ready()
    {
        Multiplayer.ConnectedToServer += OnConnected;

        Multiplayer.ServerDisconnected += OnDisconnected;



    }
    public void OnConnected()
    {
        connectui.Visible = false;
        disconnectui.Visible = true;
    }
    public void OnDisconnected()
    {
        connectui.Visible = true;
        disconnectui.Visible = false;
    }
}
