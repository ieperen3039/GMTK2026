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
        
        Level firstLevel = levelScenes[0].Instantiate<Level>();
        AddChild(firstLevel);
    }

    void NextLevel()
    {
        // TODO add fader

        RemoveChild(currentLevel);
        PackedScene packedScene = levelScenes[currentLevelIdx++];
        currentLevel = packedScene.Instantiate<Level>();
        currentLevel.OnNextLevel += NextLevel;
        AddChild(currentLevel);
    }
}
