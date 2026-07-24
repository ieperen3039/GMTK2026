using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

// contains whats in a level
public partial class Level : Node2D
{
    [Signal]
    public delegate void OnNextLevelEventHandler();

    private PackedScene levelCompleteScene;
    private PackedScene ductTapeScene;
    private PackedScene rocketScene;

    private List<DuctTape> tapes = new();
    private Camera2D camera;
    private Node rocketComponentsNode;
    private Node ductTapeInstancesNode;
    private ControlComponent controlComponent;

    private IMouseTool mouseTool;
    private RocketComponent hoveredComponent;

    public override void _Ready()
    {
        levelCompleteScene = ResourceLoader.Load<PackedScene>("uid://s62hk0dts0pl");
        ductTapeScene = ResourceLoader.Load<PackedScene>("uid://dxtpf7xkx1g4k");
        rocketScene = ResourceLoader.Load<PackedScene>("uid://dmdekhk5ugqao");
        camera = GetNode<Camera2D>("Camera2D");
        rocketComponentsNode = GetNode<Node>("RocketComponents");
        ductTapeInstancesNode = GetNode<Node>("DuctTapeInstances");
        mouseTool = new GrabTool(this);

        foreach (Node child in rocketComponentsNode.GetChildren())
        {
            if (child is RocketComponent part)
            {
                part.MouseEntered += () => OnRocketComponentMouse(part, true);
                part.MouseExited += () => OnRocketComponentMouse(part, false);

                if (part is ControlComponent control)
                {
                    if (controlComponent != null)
                    {
                        throw new Exception($"Multiple control components: {controlComponent.Name} and {control.Name}");
                    }

                    controlComponent = control;
                }
            }
        }

        if (controlComponent == null)
        {
            throw new Exception($"No control components in scene");
        }

        Button tapeToolButton = GetNode("CanvasLayer").GetNode<Button>("SetTapeTool");
        tapeToolButton.Pressed += SetTapeTool;
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

        // iteratively search for nodes connected to any of the nodes in nodesSeen
        // OPITMIZATION(#19) we can check against _all_ compomenents in nodesSeen in the inner if-statement
        Rocket rocket = rocketScene.Instantiate<Rocket>();
        rocket.GlobalPosition = controlComponent.GlobalPosition;
        rocket.GlobalRotation = controlComponent.GlobalRotation;
        HashSet<Node> nodesToCheck = [controlComponent];
        HashSet<Node> nodesSeen = [controlComponent];
        int i = 0;
        while (nodesToCheck.Count > 0)
        {
            i++;
            if (i > 100) break;
            GD.Print($"Nodes to check: {nodesToCheck:?}");

            Node nodeToCheck = nodesToCheck.First();
            nodesToCheck.Remove(nodeToCheck);

            // find all components connected to nodeToCheck.
            // add all of them to a new Rocket
            foreach (DuctTape connection in tapes)
            {
                RocketComponent a = connection.ComponentA;
                RocketComponent b = connection.ComponentB;
                if (a == nodeToCheck || b == nodeToCheck)
                {
                    // check that these components are not already queued for handling
                    if (!nodesSeen.Contains(a))
                    {
                        rocket.AddComponent(a);
                        nodesToCheck.Add(a);
                        nodesSeen.Add(a);
                    }
                    if (!nodesSeen.Contains(b))
                    {
                        rocket.AddComponent(b);
                        nodesToCheck.Add(b);
                        nodesSeen.Add(b);
                    }
                }
            }
        }

        // these have been emptied in rocket.AddComponent;
        foreach (Node node in nodesSeen)
        {
            node.QueueFree();
            RemoveChild(node);
        }

        camera.Reparent(rocket);
        GetTree().CreateTween()
            .TweenProperty(camera, "position", Vector2.Zero, 1f)
            .SetEase(Tween.EaseType.Out);

        AddChild(rocket);
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
