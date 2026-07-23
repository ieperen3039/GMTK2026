using Godot;
using System;

// level-manager
public partial class Game : Node
{
    private PackedScene[] levelScenes;

    private int currentLevelIdx = 0;
    private Level currentLevel;

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
        GD.Print("Moving to level " + currentLevel);
        if (currentLevel != null)
        {
            RemoveChild(currentLevel);
        }

        PackedScene packedScene = levelScenes[currentLevelIdx++];
        currentLevel = packedScene.Instantiate<Level>();
        currentLevel.OnNextLevel += NextLevel;
        AddChild(currentLevel);
    }
}
