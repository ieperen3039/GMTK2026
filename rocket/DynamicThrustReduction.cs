
using System.Collections.Generic;
using Godot;
using static System.TupleExtensions;

public class DynamicThrustReduction
{
    // radians per pixel offset
    public const float XOffsetCorrectionFactor = 0.002f;

    public const float AngleCorrectionSpeed = 10.0f;
    public const float AngleCorrectionDampening = 10.0f;
    public const float TorqueCorrectionStrength = 1.0f;
    public const float PlayerControlRotation = 0.5f;
    public const float MinimumControlTorque = 10f;

    public static void BalanceThrusters(Rocket rocket, float rotationTarget)
    {
        IReadOnlyList<ThrustSource> thrusters = rocket.GetThrusters();

        Dictionary<ThrustSource, float> torques = new();
        PriorityQueue<ThrustSource, float> mostEffectiveTorqueingThrusters = new();
        float totalPosTorque = 0;
        float totalNegTorque = 0; // abs value

        foreach (ThrustSource thruster in thrusters)
        {
            Vector2 globalThrustVector = thruster.GetThrustAt(1.0f);
            Vector2 globalOffset = thruster.GlobalPosition - rocket.ToGlobal(rocket.CenterOfMass);
            float torque = globalOffset.Cross(globalThrustVector);
            torques.Add(thruster, torque);

            float upwardsThrust = globalThrustVector.Cross(Vector2.Up);
            float torqueEffectiveness = torque / upwardsThrust;
            mostEffectiveTorqueingThrusters.Enqueue(thruster, torqueEffectiveness);

            if (torque < 0) totalNegTorque -= torque;
            else totalPosTorque += torque;
        }

        // note: rocket.Inertia is set to 0
        float rocketIntertia = 1.0f / PhysicsServer2D.BodyGetDirectState(rocket.GetRid()).InverseInertia;

        float offset = Game.CentralXCoordinate - rocket.GlobalPosition.X;
        float desiredRotation = offset * XOffsetCorrectionFactor + rotationTarget * PlayerControlRotation;
        float currentRotation = Util.RotationRelativeToUp(rocket.Rotation);
        float rotationDifference = Mathf.Clamp(desiredRotation - currentRotation, -Mathf.Pi, Mathf.Pi);
        float desiredAngularVelocity = rotationDifference * AngleCorrectionSpeed;
        float angularVelocityDifference = desiredAngularVelocity - rocket.AngularVelocity;
        float targetTorque = angularVelocityDifference * rocketIntertia * AngleCorrectionDampening;
        float currentTorque = totalPosTorque - totalNegTorque;

        float desiredTorqueChange = (targetTorque - currentTorque) * TorqueCorrectionStrength;
        float totalTorqueInDirectionOfDesired = (currentTorque > targetTorque) ? totalPosTorque : totalNegTorque;

        GD.Print($"offset = {offset}, desiredRotation = {desiredRotation}, currentRotation = {currentRotation}");
        GD.Print($"currentTorque = {currentTorque}, targetTorque = {targetTorque}, desiredTorqueChange = {desiredTorqueChange}");

        float accumulatedTorque = 0;
        float maxAccumulatedTorque = totalTorqueInDirectionOfDesired - Mathf.Abs(desiredTorqueChange);

        while (mostEffectiveTorqueingThrusters.Count > 0)
        {
            // LEAST effective thruster first
            ThrustSource thruster = mostEffectiveTorqueingThrusters.Dequeue();
            float torque = torques[thruster];

            // if torque helps move total to target, go full blast
            if ((torque > 0) == (targetTorque > currentTorque)
                || float.IsInfinity(targetTorque)
                || Mathf.Abs(torque) < MinimumControlTorque)
            {
                thruster.SetThrustFactor(1.0f);
                GD.Print($"Set targetPowerLevel = MAX (torque = {torque})");
            }
            else
            {
                // opposite torque, reduce power if we run out of budget
                float torqueBudgetLeft = maxAccumulatedTorque - accumulatedTorque;
                float targetPowerLevel = Mathf.Clamp(torqueBudgetLeft / Mathf.Abs(torque), 0, 1);
                thruster.SetThrustFactor(targetPowerLevel);
                GD.Print($"Set targetPowerLevel = {targetPowerLevel} (torque = {torque})");
                accumulatedTorque += Mathf.Abs(torque) * targetPowerLevel;
            }
        }
    }
}