using Godot;
using System;

public partial class LevelTest : Node2D
{
	// Signals
	[Signal]
	public delegate void LevelPassedEventHandler();

	// Private variables
	private Timer _timer;
	private CountdownTimer _timer_ui;
	private RocketEngine _engine;
	private CanvasLayer _victoryScreen;
	private float AltitudeGoal;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Get references to our child nodes
		_timer = GetNode<Timer>("%LevelTimer");
		_timer_ui = GetNode<CountdownTimer>("%CountdownTimer");
		_engine = GetNode<RocketEngine>("%RocketEngine");
		_victoryScreen = GetNode<CanvasLayer>("%VictoryScreen");

		// Setup the timer
		_timer.WaitTime = 4;
		_timer_ui.Initialize(_timer);
		_timer.Timeout += _engine.StartEngine;

		// Connect more signals
		_engine.AltitudeChanged += TestAltitude;
		LevelPassed += () => {_victoryScreen.Show();};

		_timer.Start();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}


	private void TestAltitude(float altitude)
	{
		if (altitude > AltitudeGoal)
		{
			EmitSignal(SignalName.LevelPassed);
		}
	}
}
