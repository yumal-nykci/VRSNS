using Godot;
using System;

public partial class ChangeAvatar : VBoxContainer
{
    [Export]
    LineEdit avatarpath;
    [Export]
    Button button;
    public override void _Ready()
    {
        button.Pressed += () =>
        {
            GD.Print("CHANGE AVATAR!");
            Game.UserAvatar=avatarpath.Text;
            if(NetworkMG.Instance.IsOnline)
            NetworkMG.Instance.RpcId(1, nameof(NetworkMG.Instance.ChangeUserAvatar),Game.UserAvatar);
        };
    }

}
