using Godot;
using System;

public partial class ChangeName : VBoxContainer
{
    [Export]
    Label Username;
    [Export]
    LineEdit name;
    [Export]
    Button button;
    public override void _Ready()
    {
        button.Pressed += () =>
        {
            Game.UserName = name.Text;
            NetworkMG.Instance.RpcId(1, nameof(NetworkMG.Instance.ChangeUserName), Game.Authority, name.Text);
        };
    }
    public override void _Process(double delta)
    {
        Username.Text = $"Current Username:{Game.UserName}";
    }

}
