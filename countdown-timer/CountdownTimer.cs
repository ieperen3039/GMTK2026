using Godot;
using System;

[Tool]
public partial class CountdownTimer : Node2D
{
    private Sprite2D _tens;
    private Sprite2D _ones;
	private Timer _timer;
    private int _value;

    [Export]
    public int Value
    {
        get => _value;
        set { _value = value; UpdateDisplay(); }
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_tens = GetNode<Sprite2D>("%Tens");
		_ones = GetNode<Sprite2D>("%Ones");
		UpdateDisplay();

		if (Engine.IsEditorHint())
		{
        	return; // skip runtime-only setup like timers, signals, etc.
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint())
			return; // skip runtime-only setup like timers, signals, etc.
		
		Value = (int)_timer.TimeLeft;
	}

	public void Initialize(Timer timer)
	{
		_timer = timer;
	}

    private void UpdateDisplay()
    {
        if (_tens == null || _ones == null) return;
        int clamped = Mathf.Clamp(_value, 0, 99);
        _tens.Frame = clamped / 10;
        _ones.Frame = clamped % 10;
    }
}
