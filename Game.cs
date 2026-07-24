using Godot;
using System;

// level-manager
public partial class Game : Node
{
    public const uint CollisionLayerGrabbable = 0b_0001;
    public const int CentralXCoordinate = 0;
    private PackedScene[] levelScenes;

    private int _currentLevelIdx = 0;
    private Level _currentLevel;

    public override void _Ready()
    {
        levelScenes = [
            ResourceLoader.Load<PackedScene>("res://levels/level-1/scene.tscn"),
            ResourceLoader.Load<PackedScene>("res://levels/level-2/scene.tscn")
        ];
        
        // TODO main menu instead of first level
        NextLevel();
    }

    void NextLevel()
    {
        // TODO add fader
        GD.Print("Moving to level " + _currentLevelIdx);
        if (_currentLevel != null)
        {
            _currentLevel.QueueFree();
            RemoveChild(_currentLevel);
        }

        PackedScene packedScene = levelScenes[_currentLevelIdx++];
        _currentLevel = packedScene.Instantiate<Level>();
        _currentLevel.OnNextLevel += NextLevel;
        AddChild(_currentLevel);
    }
}
