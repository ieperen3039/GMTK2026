using Godot;
using System;

public partial class BowlingPin : RocketComponent
{
    public const float PitchDeviation = 0.5f;

    private float basePitchScale;
    private Random rng = new();

    public override void _Ready()
    {
        base._Ready();
        AudioStreamPlayer2D sfxPlayer = GetNodeOrNull<AudioStreamPlayer2D>("ClangSfx");
        if (sfxPlayer != null)
        {
            basePitchScale = sfxPlayer.PitchScale - PitchDeviation / 2;
        }
    }


    protected override void PlayCollisionSound(AudioStreamPlayer2D player, float volume)
    {
        player.VolumeLinear = volume;
        player.PitchScale = basePitchScale + PitchDeviation * rng.NextSingle();
        player.Play();
    }
}
