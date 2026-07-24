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
    private RocketComponent hoveredComponent;

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
                part.MouseEntered += () => OnRocketComponentMouse(part, true);
                part.MouseExited += () => OnRocketComponentMouse(part, false);
            }
        }

        Button tapeToolButton = GetNode("CanvasLayer").GetNode<Button>("SetTapeTool");
        tapeToolButton.Pressed += SetTapeTool;

        mouseTool = new GrabTool(this);
    }

    private void OnRocketComponentMouse(RocketComponent part, bool setActive)
    {
        if (!setActive)
        {
            if (hoveredComponent == part)
            {
                hoveredComponent = null;
            }
        }
        else
        {
            hoveredComponent = part;
        }
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

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Right)
            {
                mouseTool.OnCancel();
                mouseTool = new GrabTool(this);
            }
            else if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                // use GetGlobalMousePosition instead of mouseEvent.Position; 
                // mouseEvent.Position is relative to viewport
                if (mouseEvent.IsPressed())
                {
                    mouseTool.OnClick(GetGlobalMousePosition());
                }
                else if (mouseEvent.IsReleased())
                {
                    mouseTool.OnRelease(GetGlobalMousePosition());
                }
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
            parent.ductTapeInstancesNode.AddChild(tape);
            parent.tapes.Add(tape);
            return tape;
        }


        public void OnClick(Vector2 mousePosition)
        {
            RocketComponent component = parent.hoveredComponent;
            if (component != null)
            {
                Vector2 relativeClick = component.ToLocal(mousePosition);
                tape.Attach(component, relativeClick);

                if (tape.Status != DuctTape.StatusValue.HalfConnected)
                {
                    throw new Exception("Unexpected state " + tape.Status);
                }
            }
        }

        public void OnRelease(Vector2 mousePosition)
        {
            RocketComponent component = parent.hoveredComponent;
            if (component != null)
            {
                Vector2 relativeClick = component.ToLocal(mousePosition);
                tape.Attach(component, relativeClick);

                if (tape.Status == DuctTape.StatusValue.FullConnected)
                {
                    tape = NewTape();
                }
            }
            else
            {
                OnCancel();
                tape = NewTape();
            }
        }

        public void OnCancel()
        {
            parent.tapes.Remove(tape);
            parent.ductTapeInstancesNode.RemoveChild(tape);
            tape.QueueFree();
        }
    }

    private class GrabTool : IMouseTool
    {
        private Level parent;
        private RocketComponent grabbed;

        public GrabTool(Level parent)
        {
            GD.Print("GrabTool");
            this.parent = parent;
            this.grabbed = null;
        }

        public void OnClick(Vector2 mousePosition)
        {
            RocketComponent component = parent.hoveredComponent;
            if (component != null)
            {
                grabbed = component;
                Vector2 relativeClick = component.ToLocal(mousePosition);
                component.OnGrab(relativeClick);
            }
        }

        public void OnRelease(Vector2 mousePosition) => OnCancel();

        public void OnCancel()
        {
            grabbed?.OnRelease();
            grabbed = null;
        }

    }
}
