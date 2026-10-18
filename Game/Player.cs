using System;
using System.Collections.Generic;
using Godot;

public partial class Player : CharacterBody3D
{
    [Export] Node3D AvatarRoot;
    [Export] float speed = 4f;
    [Export] float gravity = 12f;
    [Export] float jumpPower = 6f;
    [Export] float mouseSensitivity = 0.1f;
    [Export] float joypadSensitivity = 3.0f;
    Node3D cameraPivot;
    Node3D Avatar;
    Camera3D camera;

    float yaw = 0f;   // Yrot
    float pitch = 0f; // Xrot
    public bool thirdperson;
    public AvatarLoadState AvatarState { get; private set; }


    public override void _Ready()
    {
        Avatar = GetNode<Node3D>("Avatar");

        if (IsMultiplayerAuthority())
        {
            cameraPivot = GetNode<Node3D>("CameraPivot");

            camera = new Camera3D();
            camera.Name = "Camera";
            camera.Position = new Vector3(0, 0, 0);
            camera.Current = true;

            cameraPivot.AddChild(camera);
        }
        UpdateNamePlate();
        PlayerMg.Instance.UserListChanged += UpdateNamePlate;
    }

    private void UpdateNamePlate()
    {
        if (!GodotObject.IsInstanceValid(this))
            return;

        if (!IsInsideTree())
            return;
        if (Multiplayer.MultiplayerPeer == null)
            return;
        long id = GetMultiplayerAuthority();

        if (IsMultiplayerAuthority())
        {
            GetNodeOrNull<Label3D>("NamePlate")?.QueueFree();
            return;
        }

        var name = Game.OnlineUsers.GetValueOrDefault(id);

        if (name == null)
            return;

        var plate = GetNodeOrNull<Label3D>("NamePlate");

        if (plate == null)
        {
            plate = new Label3D
            {
                Name = "NamePlate",
                Position = new Vector3(0, 1.8f, 0),
                Billboard = BaseMaterial3D.BillboardModeEnum.Enabled
            };

            AddChild(plate);
        }

        plate.Text = name;
    }


    public override void _Input(InputEvent @event)
    {
        if (Multiplayer.MultiplayerPeer == null)
            return;
        if (!IsMultiplayerAuthority())
        {
            return;
        }
        if (@event is InputEventMouseMotion motion)
        {
            if (Game.Pausing || Input.IsActionPressed("capture_release")) return;

            yaw -= motion.Relative.X * mouseSensitivity;
            pitch -= motion.Relative.Y * mouseSensitivity;

            pitch = Mathf.Clamp(pitch, -70f, 89f);

            RotationDegrees = new Vector3(0, yaw, 0);
            cameraPivot.RotationDegrees = new Vector3(pitch, 0, 0);
        }
        
        if(@event is InputEventAction action)
        {
            if(action.Action== "toggle3rdparson")
            {
                thirdperson = !thirdperson;
                
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
 
        if (Multiplayer.MultiplayerPeer == null)
            return;
        if (!IsMultiplayerAuthority())
            return;
        if (!Game.Pausing)
        {
            float lookX = Input.GetAxis("look_left", "look_right");
            float lookY = Input.GetAxis("look_up", "look_down");
            yaw -= lookX * joypadSensitivity * (float)delta * 60f;
            pitch -= lookY * joypadSensitivity * (float)delta * 60f;

            pitch = Mathf.Clamp(pitch, -70f, 89f);

            RotationDegrees = new Vector3(0, yaw, 0);
            cameraPivot.RotationDegrees = new Vector3(pitch, 0, 0);

        }
        if (Input.IsActionPressed("sprint"))
        {
            if (Input.IsActionPressed("sneak")) speed=2f;
            else speed = 5f;
        }
        else
        {
            if (Input.IsActionPressed("sneak")) speed = 1f;
            else speed = 3f;
        }
        if (Game.Pausing == true)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
        else
        {
            if (Input.IsActionPressed("capture_release")) Input.MouseMode = Input.MouseModeEnum.ConfinedHidden;
            else Input.MouseMode = Input.MouseModeEnum.Captured;
        }
        

        Vector2 inputDir = Input.GetVector(
            "move_right",
            "move_left",
            "move_back",
            "move_forward"
        );
        Vector3 forward = -cameraPivot.GlobalTransform.Basis.Z;
        Vector3 right = cameraPivot.GlobalTransform.Basis.X;

        forward.Y = 0;
        right.Y = 0;
        forward = forward.Normalized();
        right = right.Normalized();

        Vector3 direction = (right * -inputDir.X + forward * inputDir.Y);

        if (direction.Length() > 0)
            direction = direction.Normalized() * speed;

        Vector3 velocity = Velocity;

        if (!Game.FocusGrupinUI)
        {
            velocity.X = direction.X;
            velocity.Z = direction.Z;
        }

        if (!IsOnFloor())
            velocity.Y -= gravity * (float)delta;
        else if (Input.IsActionJustPressed("jump"))
            velocity.Y = jumpPower;

        Velocity = velocity;

        MoveAndSlide();
    }

    private int _avatarLoadVersion = 0;

    internal async void ChangeAvatar(string avatarpath)
    {
        int version = ++_avatarLoadVersion;

        AvatarState = AvatarLoadState.Loading;

        ClearAvatar();
        AvatarRoot.AddChild(
            PlayerMg.Instance.loading.Instantiate<Node3D>()
        );

        try
        {
            string path = $"user://Avatars/{avatarpath}.vrm";
            var model = await AvatarMg.Instance.LoadAvatar(path);

            if (!GodotObject.IsInstanceValid(this) ||
                !GodotObject.IsInstanceValid(AvatarRoot))
            {
                model?.QueueFree();
                return;
            }

            // 新しいロード要求が来ていたら古い結果を破棄
            if (version != _avatarLoadVersion)
            {
                model?.QueueFree();
                return;
            }

            ClearAvatar();

            if (model == null)
            {
                AvatarRoot.AddChild(
                    PlayerMg.Instance.fallback.Instantiate<Node3D>()
                );

                AvatarState = AvatarLoadState.Failed;
                GD.PrintErr($"Avatar loading returned null: {path}");
                return;
            }

            AvatarRoot.AddChild(model);
            AvatarState = AvatarLoadState.Loaded;
        }
        catch (Exception e)
        {
            if (!GodotObject.IsInstanceValid(this) ||
                !GodotObject.IsInstanceValid(AvatarRoot))
            {
                return;
            }

            if (version != _avatarLoadVersion)
                return;

            ClearAvatar();

            AvatarRoot.AddChild(
                PlayerMg.Instance.fallback.Instantiate<Node3D>()
            );

            AvatarState = AvatarLoadState.Failed;
            GD.PrintErr($"Avatar loading failed: {e}");
        }
    }

    public void ClearAvatar()
    {
        foreach (Node avc in AvatarRoot.GetChildren())
        {
            avc.QueueFree();
        }
    }
}
public enum AvatarLoadState
{
    None,
    Loading,
    Loaded,
    Failed
}