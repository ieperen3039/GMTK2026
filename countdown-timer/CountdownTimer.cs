using Godot;
using System;

public partial class CountdownTimer : RocketComponent
{
    [Export]
    public bool Quiet;
    private Sprite2D _tens;
    private Sprite2D _ones;
    private int shownValue;

    private AudioStreamPlayer2D audioPlayer;
    private AudioStreamOggVorbis[] counts;
    private int countIndex = 0;


    [Export]
    public int Value
    {
        get => shownValue;
        set
        {
            if (shownValue != value)
            {
                shownValue = value;
                UpdateDisplay();
            }
        }
    }

    public void SetValue(float floatValue) => Value = Mathf.CeilToInt(floatValue);
    public void SetValue(double doubleValue) => Value = Mathf.CeilToInt(doubleValue);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();

        audioPlayer = GetNode<AudioStreamPlayer2D>("%AudioStreamPlayer2D");

        _tens = GetNode<Sprite2D>("%Tens");
        _ones = GetNode<Sprite2D>("%Ones");

        counts = [
            ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/zero.ogg"),
            ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/one.ogg"),
            ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/two.ogg"),
            ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/three.ogg"),
            ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/four.ogg"),
            ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/five.ogg"),
            ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/six.ogg"),
            ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/seven.ogg"),
            ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/eight.ogg"),
            ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/nine.ogg"),
            ResourceLoader.Load<AudioStreamOggVorbis>("res://countdown-timer/audio/ten.ogg"),
        ];
    }

    private void UpdateDisplay()
    {
        if (_tens == null || _ones == null) return;
        int clamped = Mathf.Clamp(shownValue, 0, 99);
        _tens.Frame = clamped / 10;
        _ones.Frame = clamped % 10;

        if (!Quiet)
        {
            if (shownValue >= 0 && shownValue < counts.Length)
            {
                GD.Print($"Playing {countIndex} ({Value})");
                audioPlayer.Stream = counts[shownValue];
                audioPlayer.Play();
            }
        }
    }
}
