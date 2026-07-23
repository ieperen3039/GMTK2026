using Godot;
using System;
using System.Collections.Generic;

// contains whats in a level
public partial class Level : Node
{
    [Signal]
    public delegate void OnNextLevelEventHandler();

    private PackedScene levelCompleteScene;
    private List<RocketComponent> parts = new();
    private Camera2D camera;

    public override void _Ready()
    {
        levelCompleteScene = ResourceLoader.Load<PackedScene>("uid://s62hk0dts0pl");

        camera = GetNode<Camera2D>("Camera2D");
        Node rocketParts = GetNode<Node>("RocketComponents");
        foreach (Node child in rocketParts.GetChildren())
        {
            if (child is RocketComponent part)
            {
                parts.Add(part);
            }
        }
    }

    // attach camera to largest component tree, activate all engines
    private void OnCountdownZero()
    {
        foreach (RocketComponent part in parts)
        {
            // ...
        }
    }

    private void OnLevelComplete()
    {
        LevelComplete levelCompleteScreen = levelCompleteScene.Instantiate<LevelComplete>();
        // chain level complete signal to this level complete signal
        levelCompleteScreen.OnNextLevel += () => EmitSignal(SignalName.OnNextLevel);
        AddChild(levelCompleteScreen);
    }

}
