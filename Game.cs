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
    private PackedScene titleScene;
    private PackedScene[] levelScenes;
    private Score[] scores;

    private int _currentLevelIdx = 0;
    private Level currentLevel;
    private Node currentScene;

    public override void _Ready()
    {
        titleScene = ResourceLoader.Load<PackedScene>("res://levels/title-screen/scene.tscn");
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
        currentLevel = null;
        currentScene = titleScene.Instantiate();
        AddChild(currentScene);
    }

    void NextLevel()
    {
        // TODO add fader
        if (currentLevel != null)
        {
            scores[_currentLevelIdx] = currentLevel.GetScore();
            currentLevel.QueueFree();
            RemoveChild(currentLevel);
            _currentLevelIdx++;
        }
        else
        {
            RemoveChild(currentScene);
        }

        if (_currentLevelIdx == levelScenes.Length)
        {
            _currentLevelIdx = 0;
            ShowTitleScreen();
            return;
        }

        GD.Print("Moving to level " + _currentLevelIdx);
        PackedScene packedScene = levelScenes[_currentLevelIdx];
        currentLevel = packedScene.Instantiate<Level>();
        currentLevel.OnNextLevel += NextLevel;
        AddChild(currentLevel);
        currentScene = currentLevel;
    }
}