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
    [Signal]
    public delegate void OnResetEventHandler();
    [Signal]
    public delegate void OnReturnEventHandler();

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
    private Node selectablesNode;
    private ControlComponent controlComponent;
    private CollisionObject2D buildPhaseBounds;
    private int numRocketComponents;

    private IMouseTool defaultMouseTool;
    private IMouseTool mouseTool;
    private Button tapeToolButton;

    private Grabbable hoveredSelectable;

    private Timer timer;
    private CountdownTimer timerGraphic;

    private Score score = new();

    private int numCrewInLevel = 0;
    private bool isLevelComplete = false;
    private bool shouldBuildRocket = false;
    private bool isGameStarted = false;

    public override void _Ready()
    {
        levelCompleteScene = ResourceLoader.Load<PackedScene>("uid://s62hk0dts0pl");
        ductTapeScene = ResourceLoader.Load<PackedScene>("uid://dxtpf7xkx1g4k");
        rocketScene = ResourceLoader.Load<PackedScene>("uid://dmdekhk5ugqao");

        camera = GetNode<Camera2D>("Camera2D");
        rocketComponentsNode = GetNode<Node>("RocketComponents");
        timer = GetNode<Timer>("LevelTimer");
        timerGraphic = GetNode<CountdownTimer>("%CountdownTimer");
        buildPhaseBounds = GetNode<CollisionObject2D>("BuildPhaseBounds");
        selectablesNode = GetNode<Node>("OtherSelectables");
        GetNode<Node2D>("Finishline").Position = new(0, -AltitudeGoal);

        // reset ui offset
        CanvasLayer ui = GetNode<CanvasLayer>("UI");
        ui.Offset = Vector2.Zero;

        Briefing briefing = GetNode<Briefing>("%Briefing");
        briefing.StartButton.Pressed += StartGame;
        briefing.MainMenuButton.Pressed += () => EmitSignal(SignalName.OnReturn);

        ductTapeInstancesNode = new Node { Name = "DuctTapeInstances" };
        AddChild(ductTapeInstancesNode);

        defaultMouseTool = new GrabTool(this);
        mouseTool = defaultMouseTool;

        foreach (Node child in rocketComponentsNode.GetChildren())
        {
            if (child is RocketComponent part)
            {
                numRocketComponents++;
                part.Freeze = true;
            }
        }

        foreach (Node child in selectablesNode.GetChildren())
        {
            if (child is RigidBody2D part)
            {
                part.Freeze = true;
            }
        }

        // Setup the timer
        timer.Timeout += OnCountdownZero;
    }

    private void StartGame()
    {
        isGameStarted = true;
        GetNode<Briefing>("%Briefing").QueueFree();
        GetNode<Control>("%GameUi").Visible = true;

        // Setup buttons
        tapeToolButton = GetNode<Button>("%SetTapeTool");
        tapeToolButton.Toggled += SetTapeTool;
        Button resetButton = GetNode<Button>("%Reset");
        resetButton.Pressed += () => EmitSignal(SignalName.OnReset);
        timer.Start();

        Random rng = new();

        // setup grabbable listeners
        foreach (Node child in rocketComponentsNode.GetChildren())
        {
            if (child is RocketComponent part)
            {
                part.Freeze = false;
                part.LinearVelocity = Vector2.Zero;
                part.AngularVelocity = 0f;

                Util.Toss(part, rng);
                part.InputPickable = true;
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
            }
        }

        foreach (Node child in selectablesNode.GetChildren())
        {
            if (child is RigidBody2D body)
            {
                body.Freeze = false;
                body.LinearVelocity = Vector2.Zero;
                body.AngularVelocity = 0f;
            }

            if (child is Grabbable part)
            {
                part.InputPickable = true;
                part.MouseEntered += () => OnHoverSelectable(part, true);
                part.MouseExited += () => OnHoverSelectable(part, false);
            }

            if (child is CrewMember)
            {
                numCrewInLevel++;
            }
        }

        if (controlComponent == null)
        {
            GD.PrintErr($"No control components in scene");
        }
    }

    private void OnHoverSelectable(Grabbable part, bool setActive)
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
        if (!isGameStarted)
        {
            timerGraphic.SetValue(timer.WaitTime);
        }
        else
        {
            timerGraphic.SetValue(timer.TimeLeft);
        }

        foreach (DuctTape tape in tapes)
        {
            tape.Update(delta);
        }

        if (Input.IsActionJustPressed("toggle_tape"))
        {
            // set active if not already active
            tapeToolButton.SetPressed(tapeToolButton.IsPressed());
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

    private void SetTapeTool(bool setActive) => SetMouseTool(setActive ? new TapeTool(this) : defaultMouseTool);

    // attach camera to largest component tree, activate all engines
    private void OnCountdownZero()
    {
        buildPhaseBounds.ProcessMode = ProcessModeEnum.Disabled;

        // all thrusters to 100%
        foreach (Node node in rocketComponentsNode.GetChildren())
        {
            if (node is Thruster component)
            {
                component.ActivateThruster();
            }
        }

        // building the rocket must happen on the physics thread
        shouldBuildRocket = true;
    }

    private void CheckVictory(float altitude)
    {
        if (altitude > AltitudeGoal && !isLevelComplete)
        {
            // TODO show warning that not all crew are present
            if (controlComponent is CrewCompartment cc && cc.NumCrewInside < numCrewInLevel)
            {
                return;
            }

            GD.Print("Level Complete!");
            isLevelComplete = true;
            OnLevelComplete();
        }
    }

    private void OnLevelComplete()
    {
        // first count the score
        score = new() { TotalComponents = numRocketComponents, };

        float minimumAltitudeToCount = AltitudeGoal - AltitudeScoreZone;
        foreach (Node node in rocketComponentsNode.GetChildren())
        {
            if (node is not RigidBody2D component) continue;
            if (-component.GlobalPosition.Y < minimumAltitudeToCount) continue;

            if (component is RocketComponent)
            {
                score.NumLiftedComponents++;
            }
        }

        foreach (Node node in selectablesNode.GetChildren())
        {
            if (node is not RigidBody2D component) continue;
            if (-component.GlobalPosition.Y < minimumAltitudeToCount) continue;
            score.NumExtras++;
        }

        LevelComplete levelCompleteScreen = levelCompleteScene.Instantiate<LevelComplete>();
        // chain level complete signal to this level complete signal
        levelCompleteScreen.OnNextLevel += () => EmitSignal(SignalName.OnNextLevel);

        levelCompleteScreen.Score = score;
        camera.Reparent(levelCompleteScreen);
        AddChild(levelCompleteScreen);
    }

    private void SetMouseTool(IMouseTool newMouseTool)
    {
        mouseTool.OnCancel();
        mouseTool = newMouseTool;
        tapeToolButton.SetPressedNoSignal(newMouseTool is TapeTool);
        GD.Print($"mouseTool = {mouseTool.GetType().Name}");
    }


    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Right)
            {
                SetMouseTool(defaultMouseTool);
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
            Grabbable selectable = parent.hoveredSelectable;
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
            Grabbable selectable = parent.hoveredSelectable;
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
            tape.Snap();
            parent.tapes.Remove(tape);
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
            this.parent = parent;
            this.grabbed = null;
        }

        public void OnClick(Vector2 mousePosition)
        {
            RigidBody2D thing = parent.hoveredSelectable;
            if (thing is Grabbable grabbable)
            {
                // prevent grabbing rocket components
                if (thing is RocketComponent component && component.PartOfRocket) return;

                grabbed = grabbable;
                Vector2 relativeClick = grabbable.ToLocal(mousePosition);
                grabbable.OnGrab(relativeClick);
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