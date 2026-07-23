using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

// contains whats in a level
public partial class Level : Node2D
{
    [Signal]
    public delegate void OnNextLevelEventHandler();

    private PackedScene levelCompleteScene;
    private PackedScene ductTapeScene;

    private List<DuctTape> tapes = new();
    private Camera2D camera;
    private Node rocketComponentsNode;
    private Node ductTapeInstancesNode;

    private DuctTape currentMouseTool = null;

    public override void _Ready()
    {
        levelCompleteScene = ResourceLoader.Load<PackedScene>("uid://s62hk0dts0pl");
        ductTapeScene = ResourceLoader.Load<PackedScene>("uid://dxtpf7xkx1g4k");
        camera = GetNode<Camera2D>("Camera2D");
        rocketComponentsNode = GetNode<Node>("RocketComponents");
        ductTapeInstancesNode = GetNode<Node>("DuctTapeInstances");

        foreach (Node child in rocketComponentsNode.GetChildren())
        {
            if (child is RocketComponent part)
            {
                part.OnClick += OnRocketPartClicked;
            }
        }

        Button tapeToolButton = GetNode("CanvasLayer").GetNode<Button>("SetTapeTool");
        tapeToolButton.Toggled += SetTapeTool;
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (DuctTape tape in tapes)
        {
            tape.Update(delta);
        }
    }

    private void SetTapeTool(bool active)
    {
        if (active)
        {
            DuctTape tape = ductTapeScene.Instantiate<DuctTape>();
            tape.ComponentQuery = GetRocketComponentAt;
            ductTapeInstancesNode.AddChild(tape);
            tapes.Add(tape);
            currentMouseTool = tape;
        }
        else
        {
            tapes.Remove(currentMouseTool);
            currentMouseTool.QueueFree();
            currentMouseTool = null;
        }
    }

    private RocketComponent GetRocketComponentAt(Vector2 position)
    {
        PhysicsDirectSpaceState2D spaceState = GetWorld2D().DirectSpaceState;
        PhysicsPointQueryParameters2D query = new()
        {
            Position = position,
            CollideWithBodies = true
        };
        Array<Dictionary> results = spaceState.IntersectPoint(query);

        foreach (Dictionary result in results)
        {
            Variant collider = result["collider"];
            if (collider.GetType() == typeof(RocketComponent))
            {
                return collider.As<RocketComponent>();
            }
        }

        return null;
    }

    // attach camera to largest component tree, activate all engines
    private void OnCountdownZero()
    {
        foreach (Node child in rocketComponentsNode.GetChildren())
        {
            if (child is ThrusterComponent thruster)
            {
                // activate thruster
            }
        }
    }

    private void OnRocketPartClicked(RocketComponent component, MouseButton button, Vector2 where)
    {
        if (currentMouseTool == null)
        {
            if (button == MouseButton.Left)
            {
                component.OnGrab(where);
            }
            else if (button == MouseButton.Right)
            {
                component.OnRelease();
            }
        }
        else
        {
            if (button == MouseButton.Left)
            {
                currentMouseTool.Attach(component, where);
                if (currentMouseTool.Status == DuctTape.StatusValue.FullConnected)
                {
                    // start new tape
                    currentMouseTool = null;
                    SetTapeTool(true);
                }
            }
            else if (button == MouseButton.Right)
            {
                SetTapeTool(false);
            }
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
