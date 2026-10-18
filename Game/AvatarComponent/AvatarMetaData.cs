using Godot;
using System;

public partial class AvatarMetaData : Node
{
    [Export]
    public AvatarEye Eye;

    [Export] public StringName HeadBone { get; set; }
    [Export] public StringName LeftHandBone { get; set; }
    [Export] public StringName RightHandBone { get; set; }
}
