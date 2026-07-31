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

    public const float FadeOutDuration = 0.25f;
    public const float FadeInDuration = 0.25f;

    private CanvasItem fader;
    private Node activeScene;

    private PackedScene titleScreenScene;
    private PackedScene[] levelScenes;
    private Score[] scores;

    private int _currentLevelIdx = 0;
    private Level currentLevel;

    public override void _Ready()
    {
        fader = GetNode<CanvasItem>("%Fader");
        fader.Modulate = new Color(1, 1, 1, 0);

        titleScreenScene = ResourceLoader.Load<PackedScene>("res://levels/title-screen/scene.tscn");
        levelScenes = [
            ResourceLoader.Load<PackedScene>("res://levels/level-1/scene.tscn"),
            ResourceLoader.Load<PackedScene>("res://levels/level-2/scene.tscn"),
            ResourceLoader.Load<PackedScene>("res://levels/level-3/scene.tscn"),
            ResourceLoader.Load<PackedScene>("res://levels/level-4/scene.tscn"),
        ];
        scores = new Score[levelScenes.Length];

        TitleScreen titleScreen = titleScreenScene.Instantiate<TitleScreen>();
        titleScreen.OnLevelSelect += StartLevel;
        AddChild(titleScreen);
        activeScene = titleScreen;
    }

    void TransitionTo(Node nextScene)
    {
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(fader, "modulate", new Color(1, 1, 1, 1), FadeOutDuration);
        tween.TweenCallback(Callable.From(() =>
        {
            activeScene.QueueFree();
            RemoveChild(activeScene);
            AddChild(nextScene);
            activeScene = nextScene;
        }));
        tween.TweenProperty(fader, "modulate", new Color(1, 1, 1, 0), FadeOutDuration);

    }

    void ShowTitleScreen()
    {
        currentLevel = null;
        TitleScreen titleScreen = titleScreenScene.Instantiate<TitleScreen>();
        titleScreen.OnLevelSelect += StartLevel;
        TransitionTo(titleScreen);
    }

    private void StartLevel(int levelIndex)
    {
        _currentLevelIdx = levelIndex;
        InstantiateLevel(levelIndex);
    }

    // tallies score of current level, starts next level or returns to menu if none
    void NextLevel()
    {
        scores[_currentLevelIdx] = currentLevel.GetScore();
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
        TransitionTo(currentLevel);
    }
}