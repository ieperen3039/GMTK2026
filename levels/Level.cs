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

    private IMouseTool mouseTool;

    public override void _Ready()
    {
        levelCompleteScene = ResourceLoader.Load<PackedScene>("uid://s62hk0dts0pl");
        ductTapeScene = ResourceLoader.Load<PackedScene>("uid://dxtpf7xkx1g4k");
        camera = GetNode<Camera2D>("Camera2D");
        rocketComponentsNode = GetNode<Node>("RocketComponents");
        ductTapeInstancesNode = GetNode<Node>("DuctTapeInstances");
        Area2D backgroundNode = GetNode<Area2D>("Background");
        backgroundNode.InputEvent += OnBackgroundClicked;

        foreach (Node child in rocketComponentsNode.GetChildren())
        {
            if (child is RocketComponent part)
            {
                part.OnClick += OnRocketPartClicked;
            }
        }

        Button tapeToolButton = GetNode("CanvasLayer").GetNode<Button>("SetTapeTool");
        tapeToolButton.Pressed += SetTapeTool;

        mouseTool = new GrabTool(this);
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (DuctTape tape in tapes)
        {
            tape.Update(delta);
        }
    }

    private void SetTapeTool()
    {
        mouseTool.OnCancel();
        mouseTool = new TapeTool(this);
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

    // handle global right-click and release
    public void OnBackgroundClicked(Node viewport, InputEvent @inputEvent, long shapeIdx)
    {
        if (inputEvent is InputEventMouseButton mouseEvent)
        {
            GD.Print("OnBackgroundClicked");
            if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.IsPressed())
            {
                mouseTool.OnCancel();
                mouseTool = new GrabTool(this);
            }
            if (mouseEvent.IsReleased())
            {
                mouseTool.OnRelease();
            }
        }
    }

    private void OnRocketPartClicked(RocketComponent component, InputEventMouseButton mouseEvent)
    {
        if (mouseEvent.ButtonIndex == MouseButton.Right) return;

        mouseTool.OnRocketPartEvent(component, mouseEvent);
    }


    private void OnLevelComplete()
    {
        LevelComplete levelCompleteScreen = levelCompleteScene.Instantiate<LevelComplete>();
        // chain level complete signal to this level complete signal
        levelCompleteScreen.OnNextLevel += () => EmitSignal(SignalName.OnNextLevel);
        AddChild(levelCompleteScreen);
    }

    private class TapeTool : IMouseTool
    {
        public Level parent;
        public DuctTape tape;

        public TapeTool(Level parent)
        {
            GD.Print("TapeTool");
            this.parent = parent;
            tape = NewTape();
        }

        private DuctTape NewTape()
        {
            DuctTape tape = parent.ductTapeScene.Instantiate<DuctTape>();
            tape.ComponentQuery = parent.GetRocketComponentAt;
            parent.ductTapeInstancesNode.AddChild(tape);
            parent.tapes.Add(tape);
            return tape;
        }

        public void OnRocketPartEvent(RocketComponent component, InputEventMouseButton mouseEvent)
        {
            GD.Print("TapeTool::OnRocketPartEvent");
            Vector2 relativeClick = component.ToLocal(mouseEvent.GlobalPosition);
            tape.Attach(component, relativeClick);
            // both for pressed and released

            if (tape.Status == DuctTape.StatusValue.FullConnected)
            {
                GD.Print("Tape Connected");
                // start new tape
                tape = NewTape();
            }
        }

        public void OnRelease()
        {
            GD.Print("TapeTool::OnRelease");
            OnCancel();
            tape = NewTape();
        }

        public void OnCancel()
        {
            parent.tapes.Remove(tape);
            tape.QueueFree();
        }

        public void OnClick(Vector2 mousePosition) { }

    }

    private class GrabTool : IMouseTool
    {
        private Level level;
        private RocketComponent grabbed;

        public GrabTool(Level level)
        {
            GD.Print("GrabTool");
            this.level = level;
            this.grabbed = null;
        }

        public void OnRocketPartEvent(RocketComponent component, InputEventMouseButton mouseEvent)
        {
            GD.Print("GrabTool::OnRocketPartEvent");
            if (mouseEvent.IsPressed())
            {
                grabbed = component;
                Vector2 relativeClick = component.ToLocal(mouseEvent.GlobalPosition);
                component.OnGrab(relativeClick);
            }
            else
            {
                // OnRelease();
                grabbed?.OnRelease();
                grabbed = null;
            }
        }

        public void OnClick(Vector2 mousePosition) { }

        public void OnCancel() => OnRelease();

        public void OnRelease()
        {
            GD.Print("GrabTool::OnRelease");
            grabbed?.OnRelease();
            grabbed = null;
        }
    }
}
