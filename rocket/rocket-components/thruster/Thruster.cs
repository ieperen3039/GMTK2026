using Godot;
using System;
using System.Collections.Generic;

public partial class Thruster : RocketComponent
{
    [Export]
    private Vector2 InitialParticleVelocity = new(0, 500);

    private List<CpuParticles2D> _particles = [];
    public override void _Ready()
    {
        base._Ready();

        foreach (Node node in GetNode<Node2D>("ExhaustParticles").GetChildren())
        {
            if (node is CpuParticles2D particle)
            {
                _particles.Add(particle);
                particle.Emitting = false;
            }
        }

        // TODO remove
        ActivateThruster();
    }

    public void ActivateThruster()
    {
        // Enable the thrust forces
        foreach (ThrustSource thruster in ThrustSources)
        {
            thruster.SetActivationThrustFactor();
        }

        // Enable the visuals
        foreach (CpuParticles2D particle in _particles)
        {
            particle.Emitting = true;
        }
    }
}
