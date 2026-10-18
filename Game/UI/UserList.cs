using Godot;
using System;
using System.Linq;

public partial class UserList : VBoxContainer
{
    [Export]
    Label TotalUser;

    [Export]
    VBoxContainer UserC;
    public override void _Ready()
    {
        PlayerMg.Instance.UserListChanged += UpdateUserList;
    }
    public void UpdateUserList()
    {
        foreach (var u in UserC.GetChildren()) u.QueueFree();
        foreach (var user in Game.OnlineUsers)
        {
            UserC.AddChild(new Label { Text = user.Value });
        }
        TotalUser.Text = $"Total Users:{Game.OnlineUsers.Count().ToString()}";
    }
}
