using Godot;
using System.Threading.Tasks;
public partial class AvatarMg : Node
{
    [Signal]
    public delegate void AvatarChangedEventHandler(long id);

    public static AvatarMg Instance;

    private Node AvatarLoader;

    public override void _EnterTree()
    {
        Instance = this;

        var script = GD.Load<GDScript>(
    "res://Game/Assets/Scripts/AvatarLoad.gd"
);

        GD.Print($"Script: {script}");

        AvatarLoader = (Node)script.New();

        GD.Print($"Loader: {AvatarLoader}");
        GD.Print($"Loader script: {AvatarLoader.GetScript()}");
        GD.Print($"Has load_model: {AvatarLoader.HasMethod("load_model")}");

        AddChild(AvatarLoader);
    }

    public async Task<Node3D> LoadAvatar(string path)
    {
        var tcs = new TaskCompletionSource<Node3D>();
        Variant result = new();
        await Task.Run(() =>
            result = AvatarLoader.Call("load_model", path)
           
        );
        if (result.VariantType == Variant.Type.Nil)
        {
            GD.PrintErr($"Failed to load avatar: {path}");
            return null;
        }
        tcs.TrySetResult(result.As<Node3D>());
        return await tcs.Task;
    }
}