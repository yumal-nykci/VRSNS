using Godot;
using System.Collections.Generic;

public partial class Game : CanvasLayer
{
    [Export]
    Control PauseMenu;
    
    public static bool isVr =
        XRServer.PrimaryInterface != null &&
        XRServer.PrimaryInterface.IsInitialized();

    public static bool Pausing = true;
    public static bool FocusGrupinUI = true;
    public static string UserName = "User";
    public static string UserAvatar="";
    public static int Authority =0;
    public static Dictionary<long, string> OnlineUsers = new();
    public static Dictionary<long, string> UserAvatars = new();
    public static string CurrentWorld="";
    private static Game game;
    public override void _Ready()
    {
        game = this;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("open_close_menu"))
        {
            if (Pausing) UnPause();
            else Pause();
        }
    }
    public static void Pause()
    {
        Pausing = true;
        FocusGrupinUI = Pausing;
        game.PauseMenu.Visible = Pausing;
    }
    public static void UnPause()
    {
        Pausing = false;
        FocusGrupinUI = Pausing;
        game.PauseMenu.Visible = Pausing;
    }
    
}