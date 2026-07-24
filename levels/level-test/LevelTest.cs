using Godot;
using System;

public partial class LevelTest : Node2D
{
	private Timer _timer;
	private CountdownTimer _timer_ui;
	private RocketEngine _engine;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_timer = GetNode<Timer>("%LevelTimer");
		_timer_ui = GetNode<CountdownTimer>("%CountdownTimer");
		_engine = GetNode<RocketEngine>("%RocketEngine");

		_timer.WaitTime = 4;
		_timer_ui.Initialize(_timer);

		_timer.Timeout += _engine.StartEngine;

		_timer.Start();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
