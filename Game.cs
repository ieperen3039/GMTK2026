using Godot;
using System;
using System.Linq;

// level-manager
public partial class Game : Node
{
    public const uint CollisionLayerPrimary = 0b_0001;
    public const uint CollisionLayerGrabbable = 0b_0001;
    public const uint CollisionLayerMagnet = 0b_0001;
    public const int CentralXCoordinate = 0;
    private PackedScene[] levelScenes;
    private Score[] scores;

    private int _currentLevelIdx = 0;
    private Level _currentLevel;

    public override void _Ready()
    {
        levelScenes = [
            ResourceLoader.Load<PackedScene>("res://levels/level-1/scene.tscn"),
            ResourceLoader.Load<PackedScene>("res://levels/level-2/scene.tscn"),
            ResourceLoader.Load<PackedScene>("res://levels/level-3/scene.tscn")
        ];
        scores = new Score[levelScenes.Length];

        ShowTitleScreen();
    }

    void ShowTitleScreen()
    {
        //TODO replace this with title screen
        NextLevel();
    }

    void NextLevel()
    {
        // TODO add fader
        if (_currentLevel != null)
        {
            scores[_currentLevelIdx] = _currentLevel.GetScore();
            _currentLevel.QueueFree();
            RemoveChild(_currentLevel);
        }
        else
        {
            _currentLevelIdx++;
        }

        if (_currentLevelIdx == levelScenes.Length)
        {
            _currentLevelIdx = 0;
            ShowTitleScreen();
            return;
        }

        GD.Print("Moving to level " + _currentLevelIdx);
        PackedScene packedScene = levelScenes[_currentLevelIdx];
        _currentLevel = packedScene.Instantiate<Level>();
        _currentLevel.OnNextLevel += NextLevel;
        AddChild(_currentLevel);
    }
}