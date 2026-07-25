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

    private const int AltitudeScoreZone = 100;

    [Export]
    private int AltitudeGoal;

    private PackedScene levelCompleteScene;
    private PackedScene ductTapeScene;
    private PackedScene rocketScene;

    private List<DuctTape> tapes = new();
    private Camera2D camera;
    private Node rocketComponentsNode;
    private Node ductTapeInstancesNode;
    private ControlComponent controlComponent;
    private CollisionObject2D buildPhaseBounds;
    private int numRocketComponents;

    private IMouseTool defaultMouseTool;
    private IMouseTool mouseTool;
    private RigidBody2D hoveredSelectable;

    private Timer timer;
    private CountdownTimer timer_ui;

    private Score score = new();

    private bool isLevelComplete = false;
    private bool shouldBuildRocket = false;


    public override void _Ready()
    {
        levelCompleteScene = ResourceLoader.Load<PackedScene>("uid://s62hk0dts0pl");
        ductTapeScene = ResourceLoader.Load<PackedScene>("uid://dxtpf7xkx1g4k");
        rocketScene = ResourceLoader.Load<PackedScene>("uid://dmdekhk5ugqao");

        camera = GetNode<Camera2D>("Camera2D");
        rocketComponentsNode = GetNode<Node>("RocketComponents");
        ductTapeInstancesNode = GetNode<Node>("DuctTapeInstances");
        timer = GetNode<Timer>("LevelTimer");
        timer_ui = GetNode<CountdownTimer>("%CountdownTimer");
        buildPhaseBounds = GetNode<CollisionObject2D>("BuildPhaseBounds");
        Node selectablesNode = GetNode<Node>("OtherSelectables");

        defaultMouseTool = new GrabTool(this);
        mouseTool = defaultMouseTool;

        // setup grabbable listeners
        foreach (Node child in rocketComponentsNode.GetChildren())
        {
            if (child is RigidBody2D part)
            {
                part.MouseEntered += () => OnHoverSelectable(part, true);
                part.MouseExited += () => OnHoverSelectable(part, false);

                if (part is ControlComponent control)
                {
                    if (controlComponent != null)
                    {
                        throw new Exception($"Multiple control components: {controlComponent.Name} and {control.Name}");
                    }

                    controlComponent = control;
                }

                if (part is RocketComponent) numRocketComponents++;
            }
        }

        foreach (Node child in selectablesNode.GetChildren())
        {
            if (child is RigidBody2D part)
            {
                part.InputPickable = true;
                part.MouseEntered += () => OnHoverSelectable(part, true);
                part.MouseExited += () => OnHoverSelectable(part, false);
            }
        }

        if (controlComponent == null)
        {
            throw new Exception($"No control components in scene");
        }

        // Setup tools
        CanvasLayer canvasLayer = GetNode<CanvasLayer>("CanvasLayer");
        canvasLayer.Offset = Vector2.Zero;
        Button tapeToolButton = canvasLayer.GetNode<Button>("SetTapeTool");
        tapeToolButton.Pressed += SetTapeTool;

        // Setup the timer
        timer_ui.Initialize(timer);
        timer.Timeout += OnCountdownZero;
        timer.Start();
    }

    private void OnHoverSelectable(RigidBody2D part, bool setActive)
    {
        if (!setActive)
        {
            if (hoveredSelectable == part)
            {
                hoveredSelectable = null;
            }
        }
        else
        {
            GD.Print($"Hovering {part.Name}");
            hoveredSelectable = part;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (DuctTape tape in tapes)
        {
            tape.Update(delta);
        }

        if (Input.IsActionJustPressed("toggle_tape"))
        {
            if (mouseTool is TapeTool) ResetMouseTool();
            else SetTapeTool();
        }

        if (shouldBuildRocket)
        {
            shouldBuildRocket = false;
            
            Rocket rocket = rocketScene.Instantiate<Rocket>();
            rocket.AltitudeChanged += CheckVictory;
            rocket.AddAllNearbyRecursively(controlComponent);
            AddChild(rocket);

            camera.Reparent(rocket.ControlComponent);
            GetTree().CreateTween()
                .TweenProperty(camera, "position", Vector2.Zero, 1f)
                .SetEase(Tween.EaseType.Out);
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
        // NOTE: overwrite _default_ tool
        defaultMouseTool = new NullTool();
        ResetMouseTool();
        hoveredSelectable = null;

        buildPhaseBounds.ProcessMode = ProcessModeEnum.Disabled;

        // all thrusters to 100%
        foreach (Node node in rocketComponentsNode.GetChildren())
        {
            if (node is RocketComponent component)
            {
                foreach (ThrustSource thruster in component.ThrustSources)
                {
                    thruster.SetActivationThrustFactor();
                }
            }
        }

        // building the rocket must happen on the physics thread
        shouldBuildRocket = true;
    }

    private void CheckVictory(float altitude)
    {
        if (altitude > AltitudeGoal && !isLevelComplete)
        {
            GD.Print("Level Complete!");
            isLevelComplete = true;
            OnLevelComplete();
        }
    }

    private void OnLevelComplete()
    {
        // first count the score
        int numLiftedComponents = 0;
        int numExtras = 0;

        float minimumAltitudeToCount = AltitudeGoal - AltitudeScoreZone;
        foreach (Node node in rocketComponentsNode.GetChildren())
        {
            if (node is not RigidBody2D component) continue;
            if (-component.GlobalPosition.Y < minimumAltitudeToCount) continue;
            
            if (component is RocketComponent)
            {
                numLiftedComponents++;
            }
            else
            {
                numExtras++;
            }
        }
        
        score = new()
        {
            TotalComponents = numRocketComponents,
            NumLiftedComponents = numLiftedComponents,
            NumExtras = numExtras,
        };

        LevelComplete levelCompleteScreen = levelCompleteScene.Instantiate<LevelComplete>();
        // chain level complete signal to this level complete signal
        levelCompleteScreen.OnNextLevel += () => EmitSignal(SignalName.OnNextLevel);

        levelCompleteScreen.Score = score;
        camera.Reparent(levelCompleteScreen);
        AddChild(levelCompleteScreen);
    }

    private void ResetMouseTool()
    {
        mouseTool.OnCancel();
        mouseTool = defaultMouseTool;
    }


    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Right)
            {
                ResetMouseTool();
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

    public Score GetScore() => score;

    // player can apply tape to rocket components


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
            RigidBody2D selectable = parent.hoveredSelectable;
            GD.Print($"OnClick {selectable?.Name}");
            if (selectable != null)
            {
                Vector2 relativeClick = selectable.ToLocal(mousePosition);
                tape.Attach(selectable, relativeClick);

                if (tape.Status == DuctTape.StatusValue.Empty)
                {
                    // avoid edge case
                    OnCancel();
                    tape = NewTape();
                }
            }
        }

        public void OnRelease(Vector2 mousePosition)
        {
            RigidBody2D selectable = parent.hoveredSelectable;
            if (selectable != null)
            {
                Vector2 relativeClick = selectable.ToLocal(mousePosition);
                tape.Attach(selectable, relativeClick);

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

    // player can grab rocket components
    private class GrabTool : IMouseTool
    {
        private Level parent;
        private Grabbable grabbed;

        public GrabTool(Level parent)
        {
            GD.Print("GrabTool");
            this.parent = parent;
            this.grabbed = null;
        }

        public void OnClick(Vector2 mousePosition)
        {
            RigidBody2D thing = parent.hoveredSelectable;
            if (thing is Grabbable component)
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

    // player can't do anything
    private class NullTool : IMouseTool
    {
        public void OnCancel() { }
        public void OnClick(Vector2 mousePosition) { }
        public void OnRelease(Vector2 mousePosition) { }
    }
}