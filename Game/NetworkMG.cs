using Godot;
using System;
using System.Linq;

public partial class NetworkMG : Node
{
    
    [Signal]
    public delegate void DisconnectedEventHandler();
    [Signal]
    public delegate void HostStartedEventHandler();
    [Export]
    public PlayerMg playerMg;
    [Export]
    public WorldMg WorldMg;
    public static NetworkMG Instance { get; private set; }
    public bool IsConnectedToServer = false;
    public bool IsHost = false;
    public bool IsOnline => IsHost || IsConnectedToServer;
    public override void _Ready()
    {
        Instance = this;

        Multiplayer.ConnectedToServer += OnConnectedToServer;
        Multiplayer.ConnectionFailed += OnConnectionFailed;
        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
        Multiplayer.ServerDisconnected += OnServerDisconnected;
    }
    
    public void OnConnectedToServer()
    {
        GD.Print("Connected to server!");

        RpcId(1, nameof(SendUsernameRpc), Game.UserName);
        RpcId(1, nameof(NetworkMG.Instance.ChangeUserAvatar), Game.UserAvatar);
        Game.Authority = Multiplayer.GetUniqueId();
        IsConnectedToServer = true;
        Game.UnPause();
    }
    public void OnConnectionFailed()
    {
        GD.PrintErr("Connection failed!");
        IsConnectedToServer = false;
    }
    public void OnPeerConnected(long id)
    {
        if (!Multiplayer.IsServer())
            return;
        GD.Print($"Peer connected: {id}");
        playerMg.Spawnpr(id);
        PlayerMg.Instance.EmitSignal(PlayerMg.SignalName.UserListChanged);
        RpcId(id, MethodName.GiveWorld, Game.CurrentWorld);
        foreach (var users in Game.OnlineUsers)
        {
            RpcId(id, nameof(ReceiveUserAvatar), users.Key, Game.UserAvatars[users.Key]);
        }
    }

    public void OnPeerDisconnected(long id)
    {
        if (!Multiplayer.IsServer())
            return;
        IsConnectedToServer = false;
        GD.Print($"Peer disconnected: {id}");

        PlayerMg.Instance.RemoveUser(id);

        var player = playerMg.GetChildren()
            .FirstOrDefault(x => x.GetMultiplayerAuthority() == id);

        player?.QueueFree();

        BroadcastUserList();
    }
    public void OnServerDisconnected()
    {
        GD.Print("Server disconnected!");
        DisconnectPeer();
    }
    public void StartHost(int port)
    {
        GD.Print("Use as a Server");
        var npeer = new ENetMultiplayerPeer();
        var error = npeer.CreateServer(port);
        if (error != Error.Ok)
        {
            throw new Exception($"Failed to Create Server: {error}");
        }
        var world = WorldMg.GetWorld();

        Multiplayer.MultiplayerPeer = npeer;
        GiveWorld(Game.CurrentWorld);

        Game.Authority = 1;

        PlayerMg.Instance.SetUser(1, Game.UserName);
        playerMg.Spawnpr(1);

        // ホスト自身のアバターを登録
        Game.UserAvatars[1] = Game.UserAvatar;

        // ホスト自身にも適用し、接続済みの全ピアにも配信
        Rpc(nameof(ReceiveUserAvatar), 1, Game.UserAvatar);

        EmitSignal(SignalName.HostStarted);
        PlayerMg.Instance.EmitSignal(
            PlayerMg.SignalName.UserListChanged
        );
        Game.UnPause();
        IsHost = true;
    }

    public void StartClient(string address, int port)
    {
        GD.Print("Use as a Client");
        var npeer = new ENetMultiplayerPeer();
        var error = npeer.CreateClient(address, port);
        if (error != Error.Ok)
        {
            throw new Exception($"Failed to Create Client: {error}");
        }
        Multiplayer.MultiplayerPeer = npeer;
        
    }
    public void DisconnectPeer()
    {
        GD.Print("Disconnect peer");
        var peer = Multiplayer.MultiplayerPeer;
        foreach (var id in Game.OnlineUsers)
        {
            PlayerMg.Instance.RemoveUser(id.Key);

        }
        foreach (var player in playerMg.GetChildren())
        {
            player.QueueFree();
        }
        Game.OnlineUsers.Clear();
        Game.Authority = 0;

        if (peer != null)
        {
            peer.Close();
            Multiplayer.MultiplayerPeer = null;
            EmitSignal(SignalName.Disconnected);
        }
        PlayerMg.Instance.EmitSignal(PlayerMg.SignalName.UserListChanged);
        Game.Pause();
        IsHost = false;
        IsConnectedToServer = false;
    }
    private void BroadcastUserList()
    {
        var ids = Game.OnlineUsers.Keys.ToArray();
        var names = Game.OnlineUsers.Values.ToArray();

        Rpc(nameof(ReceiveUserListRpc), ids, names);
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
private void SendUsernameRpc(string username)
{
    if (!Multiplayer.IsServer())
        return;

    long id = Multiplayer.GetRemoteSenderId();

    PlayerMg.Instance.SetUser(id, username);

    GD.Print($"User joined: {id} = {username}");

    BroadcastUserList();
}
    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void ReceiveUserListRpc(long[] ids, string[] names)
    {
        Game.OnlineUsers.Clear();

        for (int i = 0; i < ids.Length; i++)
        {
            Game.OnlineUsers[ids[i]] = names[i];
        }

        PlayerMg.Instance.EmitSignal(PlayerMg.SignalName.UserListChanged);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal=true)]
    public void ChangeUserName(int id ,string username)
    {
        PlayerMg.Instance.SetUser(id, username);

        BroadcastUserList();
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void ChangeUserAvatar(string avatarpath)
    {
        if (!Multiplayer.IsServer())
            return;

        long id = Multiplayer.GetRemoteSenderId();

        if (id == 0)
            return;

        Game.UserAvatars[id] = avatarpath;

        Rpc(nameof(ReceiveUserAvatar), (int)id, avatarpath);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void ReceiveUserAvatar(int id, string avatarpath)
    {
        PlayerMg.Instance
            .GetNode<Player>(id.ToString())
            .ChangeAvatar(avatarpath);
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void GiveWorld(string worldPath)
    {
        foreach (Node n in WorldMg.GetChildren())
        {
            n.QueueFree();
        }
        Game.CurrentWorld = worldPath;
        WorldMg.AddChild(WorldMg.GetWorld());
    }
}
