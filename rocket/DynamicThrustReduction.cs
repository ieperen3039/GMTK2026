
using System.Collections.Generic;
using Godot;
using static System.TupleExtensions;

public class DynamicThrustReduction
{
    // radians per pixel offset
    public const float XOffsetCorrectionFactor = 0.001f;
    public const float XMomentumCorrectionFactor = 0.01f;

    public const float AngleCorrectionSpeed = 0.5f;
    public const float AngleCorrectionDampening = 20.0f;
    public const float TorqueCorrectionStrength = 0.9f;
    public const float PlayerControlRotation = 1f;
    public const float MinimumControlTorque = 10f;

    public static void BalanceThrusters(RigidBody2D rocket, IReadOnlyList<ThrustSource> thrusters, float playerSteer)
    {
        if (thrusters.Count == 0) return;

        Dictionary<ThrustSource, float> torques = new();
        PriorityQueue<ThrustSource, float> leastEffectiveTorqueingThrusters = new();
        float totalPosTorque = 0;
        float totalNegTorque = 0; // abs value

        foreach (ThrustSource thruster in thrusters)
        {
            Vector2 globalThrustVector = thruster.GetThrustAt(1.0f);
            Vector2 globalOffset = thruster.GlobalPosition - rocket.ToGlobal(rocket.CenterOfMass);
            float torque = globalOffset.Cross(globalThrustVector);
            torques.Add(thruster, torque);

            float downwardThrust = globalThrustVector.Cross(Vector2.Down);
            float torqueEffectiveness = (downwardThrust == 0) ? 0 : (torque / downwardThrust);
            leastEffectiveTorqueingThrusters.Enqueue(thruster, torqueEffectiveness);

            if (torque < 0) totalNegTorque -= torque;
            else totalPosTorque += torque;
        }

        // note: rocket.Inertia is 0 if automatically computed
        if (rocket.Inertia == 0)
        {
            rocket.Inertia = 1.0f / PhysicsServer2D.BodyGetDirectState(rocket.GetRid()).InverseInertia;
        }
        
        float offset = Game.CentralXCoordinate - rocket.GlobalPosition.X;
        float offsetCorrection = offset * XOffsetCorrectionFactor;
        float momentum = -rocket.LinearVelocity.Rotated(rocket.Rotation).X;
        float momentumCorrection = momentum * XMomentumCorrectionFactor;

        float desiredRotation = Mathf.Clamp(offsetCorrection + momentumCorrection, -0.25f, 0.25f) + playerSteer * PlayerControlRotation;
        float currentRotation = Util.RotationRelativeToUp(rocket.Rotation);
        float rotationDifference = Mathf.Clamp(desiredRotation - currentRotation, -Mathf.Pi, Mathf.Pi);
        float desiredAngularVelocity = rotationDifference * AngleCorrectionSpeed;
        float angularVelocityDifference = desiredAngularVelocity - rocket.AngularVelocity;
        float targetTorque = angularVelocityDifference * rocket.Inertia * AngleCorrectionDampening;
        float currentTorque = totalPosTorque - totalNegTorque;

        float desiredTorqueChange = (targetTorque - currentTorque) * TorqueCorrectionStrength;
        float totalTorqueInDirectionOfDesired = (currentTorque > targetTorque) ? totalPosTorque : totalNegTorque;

        // GD.Print($"rocket.Inertia = {rocket.Inertia,6:F2}, rocket.AngularVelocity = {rocket.AngularVelocity}");
        // GD.Print($"desiredRotation = {desiredRotation,6:F2}, offsetCorrection = {offsetCorrection,6:F2}, momentumCorrection = {momentumCorrection,6:F2}");
        // GD.Print($"rotationDifference = {rotationDifference,6:F3}, desiredAngularVelocity = {desiredAngularVelocity,6:F6}, angularVelocityDifference = {angularVelocityDifference,6:F6}");
        // GD.Print($"currentTorque = {currentTorque,10}; targetTorque = {targetTorque,10}; desiredTorqueChange = {desiredTorqueChange,10}");

        float accumulatedTorque = 0;
        float maxAccumulatedTorque = totalTorqueInDirectionOfDesired - Mathf.Abs(desiredTorqueChange);

        if (leastEffectiveTorqueingThrusters.Count == 1)
        {
            leastEffectiveTorqueingThrusters.Dequeue().ThrustFactor = 1.0f;
            // GD.Print($"Thruster targetPowerLevel = MAX (it is the only thruster)");
        }
        else
        {
            while (leastEffectiveTorqueingThrusters.Count > 0)
            {
                // MOST effective thruster first
                ThrustSource thruster = leastEffectiveTorqueingThrusters.Dequeue();
                float torque = torques[thruster];

                // if torque helps move total to target, go full blast
                if ((torque > 0) == (targetTorque > currentTorque)
                    || float.IsInfinity(targetTorque)
                    || Mathf.Abs(torque) < MinimumControlTorque)
                {
                    thruster.ThrustFactor = 1.0f;
                    // GD.Print($"Thruster targetPowerLevel = MAX (torque = {torque})");
                }
                else
                {
                    // opposite torque, reduce power if we run out of budget
                    float torqueBudgetLeft = maxAccumulatedTorque - accumulatedTorque;
                    float targetPowerLevel = Mathf.Clamp(torqueBudgetLeft / Mathf.Abs(torque), 0, 1);
                    thruster.ThrustFactor = targetPowerLevel;
                    accumulatedTorque += Mathf.Abs(torque) * targetPowerLevel;
                    // GD.Print($"Thruster targetPowerLevel = {targetPowerLevel} (torque = {torque}, torqueBudgetLeft = {maxAccumulatedTorque - accumulatedTorque})");
                }
            }
        }
    }
}