using Godot;
using System;
using System.Collections.Generic;

public partial class Thruster : RocketComponent
{
    struct OriginalValues
    {
        public double Lifetime;
    }

    private Dictionary<CpuParticles2D, OriginalValues> _particles = [];
    private ThrusterSFX _sfx;
    public override void _Ready()
    {
        base._Ready();
        _sfx = GetNode<ThrusterSFX>("ThrusterSFX");
        _sfx.Playing = false;

        foreach (Node node in GetNode<Node2D>("ExhaustParticles").GetChildren())
        {
            if (node is CpuParticles2D particle)
            {
                _particles.Add(particle, new() { Lifetime = particle.Lifetime });
                particle.Emitting = false;
            }
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        float averageThrustFactor = 0;
        int numThrusters = 0;
        foreach (ThrustSource thruster in ThrustSources)
        {
            if (thruster.IsPassive) continue;
            averageThrustFactor += thruster.ThrustFactor;
            numThrusters++;
        }
        averageThrustFactor /= numThrusters;

        foreach (var (particle, original) in _particles)
        {
            if (averageThrustFactor > 0.1)
            {
                particle.Emitting = true;
                // squared thrust factor to make the effect more noticable
                particle.Lifetime = original.Lifetime * averageThrustFactor * averageThrustFactor;
            }
            else
            {
                particle.Emitting = false;
            }
        }
    }


    public void ActivateThruster()
    {
        // Enable the thrust forces
        foreach (ThrustSource thruster in ThrustSources)
        {
            thruster.SetActivationThrustFactor();
        }

        // Enable the sound
        _sfx.StartEngine();

        // Enable the visuals
        foreach (var (particle, _) in _particles)
        {
            particle.Emitting = true;
        }
    }
}
