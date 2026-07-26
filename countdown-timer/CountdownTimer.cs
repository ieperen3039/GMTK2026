using Godot;
using System;

[Tool]
public partial class CountdownTimer : Grabbable
{
    private Sprite2D _tens;
    private Sprite2D _ones;
    private Timer _timer;
    private int _value;

    private AudioStreamPlayer2D audioPlayer;
    // private AudioStreamOggVorbis[] counts;
    private AudioStreamOggVorbis countdownAudio;
    private int countIndex = 0;

    [Export]
    public int Value
    {
        get => _value;
        set { _value = value; UpdateDisplay(); }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();

        audioPlayer = GetNode<AudioStreamPlayer2D>("%AudioStreamPlayer2D");

        _tens = GetNode<Sprite2D>("%Tens");
        _ones = GetNode<Sprite2D>("%Ones");
        UpdateDisplay();
        // bevat de hele countdown
        countdownAudio = ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/one.ogg");

        // counts = [
            // ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/ignition-in-t-minus.ogg"),
            // ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/ten.ogg"),
            // ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/nine.ogg"),
            // ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/eight.ogg"),
            // ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/seven.ogg"),
            // ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/six.ogg"),
            // ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/five.ogg"),
            // ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/four.ogg"),
            // ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/three.ogg"),
            // ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/two.ogg"),
            // ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/one.ogg"),
            // ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/zero.ogg"),
        // ];


        if (Engine.IsEditorHint())
        {
            return; // skip runtime-only setup like timers, signals, etc.
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Engine.IsEditorHint())
            return; // skip runtime-only setup like timers, signals, etc.

        Value = Mathf.CeilToInt(_timer.TimeLeft);

        if (_timer.TimeLeft < 14f && countIndex == 0)
        {
            audioPlayer.Stream = countdownAudio;
            audioPlayer.Play();
            countIndex++;
        }

        // if (countIndex == 0)
        // {
        //     if (_timer.TimeLeft < 14.5f)
        //     {
        //         audioPlayer.Stream = counts[countIndex++];
        //         audioPlayer.Play();
        //     }
        // }
        // else if (countIndex == counts.Length)
        // {
        //     // done counting
        // }
        // else if ((12 - Value) > countIndex)
        // {
        //     GD.Print($"Playing {countIndex} ({Value})");
        //     audioPlayer.Stream = counts[countIndex++];
        //     audioPlayer.Play();
        // }
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
