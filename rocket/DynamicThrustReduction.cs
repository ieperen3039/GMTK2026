
using System.Collections.Generic;
using Godot;
using static System.TupleExtensions;

public class DynamicThrustReduction
{
    // radians per (meter offset squared)
    public const float XOffsetCorrectionFactor = 0.01f;

    public const float AngleCorrectionSpeed = 10.0f;
    public const float AngleCorrectionDampening = 10.0f;
    public const float TorqueCorrectionStrength = 1.0f;
    public const float PlayerControlRotation = 0.5f;
    public const float MinimumControlTorque = 0.1f;

    public static void BalanceThrusters(Rocket rocket, float rotationTarget)
    {
        IReadOnlyList<ThrustSource> thrusters = rocket.GetThrusters();

        Dictionary<ThrustSource, float> torques = new();
        float totalTorque = 0;

        foreach (ThrustSource t in thrusters)
        {
            var (thrust, position) = t.GetThrustAt(rocket.Transform, 1.0f);
            float torque = position.Cross(thrust);
            torques.Add(t, torque);
            totalTorque += torque;
        }

        float offset = Game.CentralXCoordinate - rocket.GlobalPosition.X;
        float desiredRotation = offset * offset * XOffsetCorrectionFactor + rotationTarget * PlayerControlRotation;
        float currentRotation = Util.RotationRelativeToUp(rocket.Rotation);
        float rotationDifference = desiredRotation - currentRotation;
        float desiredAngularVelocity = rotationDifference * AngleCorrectionSpeed;
        float angularVelocityDifference = desiredAngularVelocity - rocket.AngularVelocity;
        float targetTorque = angularVelocityDifference * AngleCorrectionDampening;
        float torqueDifference = targetTorque - totalTorque;
        float correctionFactor = Mathf.Abs(torqueDifference) * TorqueCorrectionStrength;

        GD.Print($"targetTorque = {targetTorque}, correctionFactor = {correctionFactor}");

        foreach (var (thruster, torque) in torques)
        {
            // if torque is not opposite, go full blast
            if ((torque < 0) == (torqueDifference < 0) || Mathf.Abs(torque) < MinimumControlTorque)
            {
                thruster.SetThrustFactor(1.0f);
                GD.Print($"targetPowerLevel = 1.0f (fixed)");
            }
            else
            {
                // opposite torque, reduce power depending on torque
                float targetPowerLevel = correctionFactor * Mathf.Abs(torque);
                thruster.SetThrustFactor(Mathf.Clamp(targetPowerLevel, 0, 1));
                GD.Print($"targetPowerLevel = {targetPowerLevel}");
            }
        }
    }
}