using Godot;

public partial class Player : CharacterBody3D
{
    [Export] float speed = 5f;
    [Export] float gravity = 12f;
    [Export] float jumpPower = 6f;
    [Export] float mouseSensitivity = 0.3f;
    public bool pausing = false;
    Node3D cameraPivot;
    Camera3D camera;

    float yaw = 0f;   // Yrot
    float pitch = 0f; // Xrot

    public override void _Ready()
    {
        cameraPivot = GetNode<Node3D>("CameraPivot");
        camera = cameraPivot.GetNode<Camera3D>("Camera");

        
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion)
        {
            yaw -= motion.Relative.X * mouseSensitivity;
            pitch += motion.Relative.Y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -89f, 89f);

            cameraPivot.RotationDegrees = new Vector3(pitch, yaw, 0);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionPressed("caputure_release")||pausing==true)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
        else
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
        Vector2 inputDir = Input.GetVector(
            "move_left",
            "move_right",
            "move_forward",
            "move_back"
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

        velocity.X = direction.X;
        velocity.Z = direction.Z;

        if (!IsOnFloor())
            velocity.Y -= gravity * (float)delta;
        else if (Input.IsActionJustPressed("jump"))
            velocity.Y = jumpPower;

        Velocity = velocity;

        MoveAndSlide();
    }

}
