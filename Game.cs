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
    private PackedScene titleScreenScene;
    private PackedScene[] levelScenes;
    private Score[] scores;

    private int _currentLevelIdx = 0;
    private Level currentLevel;
    private TitleScreen titleScreen;

    public override void _Ready()
    {
        titleScreenScene = ResourceLoader.Load<PackedScene>("res://levels/title-screen/scene.tscn");
        levelScenes = [
            ResourceLoader.Load<PackedScene>("res://levels/level-1/scene.tscn"),
            ResourceLoader.Load<PackedScene>("res://levels/level-2/scene.tscn"),
            ResourceLoader.Load<PackedScene>("res://levels/level-3/scene.tscn"),
            ResourceLoader.Load<PackedScene>("res://levels/level-4/scene.tscn"),
        ];
        scores = new Score[levelScenes.Length];

        ShowTitleScreen();
    }

    void ShowTitleScreen()
    {
        CleanupCurrentScene();
        titleScreen = titleScreenScene.Instantiate<TitleScreen>();
        titleScreen.OnLevelSelect += StartLevel;
        AddChild(titleScreen);
    }

    private void StartLevel(int levelIndex)
    {
        _currentLevelIdx = levelIndex;
        CleanupCurrentScene();
        InstantiateLevel(levelIndex);
    }

    // tallies score of current level, starts next level or returns to menu if none
    void NextLevel()
    {
        scores[_currentLevelIdx] = currentLevel.GetScore();

        // TODO add fader
        CleanupCurrentScene();

        _currentLevelIdx++;

        if (_currentLevelIdx == levelScenes.Length)
        {
            ShowTitleScreen();
            return;
        }

        InstantiateLevel(_currentLevelIdx);
    }

    private void InstantiateLevel(int levelIndex)
    {
        GD.Print($"Instantiating level {levelIndex + 1}");
        PackedScene packedScene = levelScenes[levelIndex];
        currentLevel = packedScene.Instantiate<Level>();
        currentLevel.OnNextLevel += NextLevel;
        currentLevel.OnReset += () => StartLevel(levelIndex);
        currentLevel.OnReturn += ShowTitleScreen;
        AddChild(currentLevel);
    }


    private void CleanupCurrentScene()
    {
        if (currentLevel != null)
        {
            currentLevel.QueueFree();
            RemoveChild(currentLevel);
            currentLevel = null;
        }
        else if (titleScreen != null)
        {
            titleScreen.QueueFree();
            RemoveChild(titleScreen);
            titleScreen = null;
        }
    }

}