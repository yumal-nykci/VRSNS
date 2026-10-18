using System;
using Godot;

public partial class ViewportUi3D : Node3D
{
    private bool _isMouseInside = false;
    private Vector2 _lastEventPos2D = Vector2.Zero;
    private double _lastEventTime = -1.0;

    private SubViewport _viewport;
    private MeshInstance3D _quad;
    private Area3D _area;

    public override void _Ready()
    {
        _viewport = GetNode<SubViewport>("SubViewport");
        _quad = GetNode<MeshInstance3D>("Quad");
        _area = GetNode<Area3D>("Quad/Area3D");

        _area.MouseEntered += MouseEnteredArea;
        _area.MouseExited += MouseExitedArea;
        _area.InputEvent += MouseInputEvent;

        var material = _quad.GetSurfaceOverrideMaterial(0) as BaseMaterial3D;

        if (material != null &&
            material.BillboardMode == BaseMaterial3D.BillboardModeEnum.Disabled)
        {
            SetProcess(false);
        }
    }



    public override void _Process(double delta)
    {
        RotateAreaToBillboard();
    }

    private void MouseEnteredArea()
    {
        _isMouseInside = true;

        _viewport.Notification((int)Viewport.NotificationVpMouseEnter);
    }

    private void MouseExitedArea()
    {
        _viewport.Notification((int)Viewport.NotificationVpMouseExit);

        _isMouseInside = false;
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        // マウス・タッチ系はPhysics Picking側で処理する
        if (inputEvent is InputEventMouseButton ||
            inputEvent is InputEventMouseMotion ||
            inputEvent is InputEventScreenDrag ||
            inputEvent is InputEventScreenTouch)
        {
            return;
        }

        _viewport.PushInput(inputEvent);
    }

    private void MouseInputEvent(
        Node camera,
        InputEvent inputEvent,
        Vector3 eventPosition,
        Vector3 normal,
        long shapeIdx)
    {
        if (_quad.Mesh is not QuadMesh quadMesh)
            return;

        Vector2 quadMeshSize = quadMesh.Size;

        // ワールド座標 → Quadのローカル座標
        Vector3 eventPos3D =
            _quad.GlobalTransform.AffineInverse() * eventPosition;

        Vector2 eventPos2D = Vector2.Zero;

        if (_isMouseInside)
        {
            eventPos2D = new Vector2(
                eventPos3D.X,
                -eventPos3D.Y
            );

            // -0.5 ～ 0.5
            eventPos2D.X /= quadMeshSize.X;
            eventPos2D.Y /= quadMeshSize.Y;

            // 0 ～ 1
            eventPos2D += new Vector2(0.5f, 0.5f);

            // SubViewport座標へ
            eventPos2D.X *= _viewport.Size.X;
            eventPos2D.Y *= _viewport.Size.Y;
        }
        else
        {
            eventPos2D = _lastEventPos2D;
        }

        // 入力イベントの座標を書き換える
        if (inputEvent is InputEventMouse mouseEvent)
        {
            mouseEvent.Position = eventPos2D;
            mouseEvent.GlobalPosition = eventPos2D;
        }
        else if (inputEvent is InputEventScreenDrag screenDrag)
        {
            screenDrag.Position = eventPos2D;
        }
        else if (inputEvent is InputEventScreenTouch screenTouch)
        {
            screenTouch.Position = eventPos2D;
        }

        double now = Time.GetTicksMsec() / 1000.0;

        // 相対移動量を計算
        if (inputEvent is InputEventMouseMotion mouseMotion)
        {
            mouseMotion.Relative = eventPos2D - _lastEventPos2D;

            if (_lastEventTime >= 0)
            {
                mouseMotion.Velocity =
                    mouseMotion.Relative /
                    (float)(now - _lastEventTime);
            }
        }
        else if (inputEvent is InputEventScreenDrag drag)
        {
            drag.Relative = eventPos2D - _lastEventPos2D;

            if (_lastEventTime >= 0)
            {
                drag.Velocity =
                    drag.Relative /
                    (float)(now - _lastEventTime);
            }
        }

        _lastEventPos2D = eventPos2D;
        _lastEventTime = now;

        // SubViewportへ入力を渡す
        _viewport.PushInput(inputEvent);
    }

    private void RotateAreaToBillboard()
    {
        var material =
            _quad.GetSurfaceOverrideMaterial(0) as BaseMaterial3D;

        if (material == null)
            return;

        var billboardMode = material.BillboardMode;

        if (billboardMode == BaseMaterial3D.BillboardModeEnum.Disabled)
            return;

        Camera3D camera = GetViewport().GetCamera3D();

        if (camera == null)
            return;

        Vector3 look =
            camera.ToGlobal(new Vector3(0, 0, -100))
            - camera.GlobalPosition;

        look = _area.Position + look;

        // Y-Billboard
        if (billboardMode ==
            BaseMaterial3D.BillboardModeEnum.FixedY)
        {
            look = new Vector3(
                look.X,
                0,
                look.Z
            );
        }

        _area.LookAt(look, Vector3.Up);

        // カメラの傾きを補正
        _area.RotateObjectLocal(
            Vector3.Back,
            camera.Rotation.Z
        );
    }
}