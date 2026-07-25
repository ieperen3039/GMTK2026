using Godot;
using System;
using System.Collections.Generic;

public partial class Thruster : RocketComponent
{
    private List<CpuParticles2D> _particles = [];
    private Sprite2D _flameSprite;
    public override void _Ready()
    {
        base._Ready();

        foreach(Node node in GetNode<Node2D>("ExhaustParticles").GetChildren())
        {
            if (node is CpuParticles2D particle)
            {
                _particles.Add(particle);
                particle.Emitting = false;
                GD.Print("Disable particle");
            }
        }
        _flameSprite = GetNode<Sprite2D>("%ExhaustFlame");
        _flameSprite.Hide();
    }

    public void ActivateThruster()
    {
        // Enable the thrust forces
        foreach (ThrustSource thruster in ThrustSources)
        {
            thruster.SetActivationThrustFactor();
        }

        
        // Enable the visuals
        _flameSprite.Show();
        foreach(CpuParticles2D particle in _particles)
        {
            particle.Emitting = true;
        }
    }

}
