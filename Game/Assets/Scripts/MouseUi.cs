using Godot;
using System;
using System.Drawing;

public partial class MouseUi : CanvasLayer
{
    TextureRect mouse;
    TextureRect screenround;
    public override void _Ready()
    {
        if (Game.isVr) return;
        mouse = GetNode<TextureRect>("Mouse");
        screenround = GetNode<TextureRect>("ScreenRound");
    }
    public override void _Process(double delta)
    {
        if (Game.isVr) return;
        if (mouse == null || mouse.Texture == null)
            return;
        Vector2 mousePos = GetViewport().GetMousePosition();
        Vector2 winSize = GetViewport().GetVisibleRect().Size;
        if (Input.IsActionPressed("capture_release"))
        {
            mouse.Position = mousePos- mouse.Size / 2;
        }
        else
        {
            mouse.Position = winSize / 2- mouse.Size / 2;
        }
    }

}
