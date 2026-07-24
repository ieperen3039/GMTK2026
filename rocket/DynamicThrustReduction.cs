
using System.Collections.Generic;
using Godot;

public class DynamicThrustReduction
{
    // radians per (meter offset squared)
    public const float XOffsetCorrectionFactor = 0.01f;

    public const float AngleCorrectionSpeed = 10.0f;
    public const float AngleCorrectionDampening = 10.0f;
    public const float TorqueCorrectionStrength = 10.0f;

    static void BalanceThrusters(Rocket rocket)
    {
        List<ThrusterComponent> thrusters = rocket.GetThrusters();

        Dictionary<ThrusterComponent, float> torques = new();
        float totalTorque = 0;

        foreach (ThrusterComponent t in thrusters)
        {
            Vector2 thrustVector = t.GetThrustAt(1.0f);
            Vector2 globalOffset = t.GlobalPosition - rocket.GetCenterOfMass();
            float torque = globalOffset.Cross(thrustVector);
            torques.Add(t, torque);
            totalTorque += torque;
        }

        float offset = Game.CentralXCoordinate - rocket.GlobalPosition.X;
        float desiredRotation = offset * offset * XOffsetCorrectionFactor;
        float currentRotation = Util.RotationRelativeToUp(rocket.Rotation);
        float rotationDifference = desiredRotation - currentRotation;
        float desiredAngularVelocity = rotationDifference * AngleCorrectionSpeed;
        float angularVelocityDifference = desiredAngularVelocity - rocket.AngularVelocity;
        float targetTorque = angularVelocityDifference * AngleCorrectionDampening;
        float torqueDifference = targetTorque - totalTorque;
        float correctionFactor = Mathf.Abs(torqueDifference) * TorqueCorrectionStrength;

        foreach (var (thruster, torque) in torques)
        {
            // if torque is not opposite, go full blast
            if ((torque < 0) == (torqueDifference < 0))
            {
                thruster.SetPowerLevel(1.0f);
            }
            else
            {
                // opposite torque, reduce power depending on torque
                float targetPowerLevel = correctionFactor * torque;
                thruster.SetPowerLevel(Mathf.Clamp(targetPowerLevel, 0, 1));
            }
        }
    }
}