using Godot;
using System;

public partial class PlayerMg : Node
{
    [Signal]
    public delegate void UserListChangedEventHandler();
    public static PlayerMg Instance;
    [Export]
    PackedScene playerscene;

    [Export]
    MultiplayerSpawner spawner;
    [Export]
    public PackedScene fallback;
    [Export]
    public PackedScene loading;

    public override void _EnterTree()
    {
        spawner.SpawnFunction =
            new Callable(this, MethodName.SpawnPlayer);
        Instance = this;
    }

    public void Spawnpr(long id)
    {
        GD.Print($"A: SpawnPlayer({id})");

        if (!Multiplayer.IsServer())
            return;

        GD.Print("B: before Spawn");
        spawner.Spawn(id);
        GD.Print("C: after Spawn");
    }

    public Node SpawnPlayer(Variant data)
    {
        GD.Print("D: SpawnFunction");

        int id = (int)data;

        GD.Print("E: before Instantiate");
        Player player = playerscene.Instantiate<Player>();
        player.Name = id.ToString();

        GD.Print("F: before Authority");
        player.SetMultiplayerAuthority(id);

        GD.Print("G: return Player");
        return player;
    }
    public void SetUser(long id, string username)
    {
        Game.OnlineUsers[id] = username;
        EmitSignal(SignalName.UserListChanged);
    }
    public void RemoveUser(long id)
    {
        Game.OnlineUsers.Remove(id);
        Game.UserAvatars.Remove(id);

        EmitSignal(SignalName.UserListChanged);
    }
}