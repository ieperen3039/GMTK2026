using Godot;
using System;

// level-manager
public partial class Game : Node
{
    [Export]
    private PackedScene[] levels;


    public override void _Ready()
    {
        PackedScene firstLevelScene = ResourceLoader.Load<PackedScene>("res://...");
        Level firstLevel = firstLevelScene.Instantiate<Level>();
        AddChild(firstLevel);
    }
}
