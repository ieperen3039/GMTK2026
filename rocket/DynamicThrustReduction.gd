class_name DynamicThrustReduction

# radians per pixel offset
const X_OFFSET_CORRECTION_FACTOR: float = 0.001
const X_MOMENTUM_CORRECTION_FACTOR: float = 0.01

const ANGLE_CORRECTION_SPEED: float = 0.5
const ANGLE_CORRECTION_DAMPENING: float = 20.0
const TORQUE_CORRECTION_STRENGTH: float = 0.9
const PLAYER_CONTROL_ROTATION: float = 1.0
const MINIMUM_CONTROL_TORQUE: float = 10.0


static func balance_thrusters(rocket: RigidBody2D, thrusters: Array, player_steer: float) -> void:
	if thrusters.size() == 0:
		return

	var torques: Dictionary = {}
	var effectiveness_entries: Array = []  # [{"thruster": ThrustSource, "effectiveness": float}]
	var total_pos_torque: float = 0.0
	var total_neg_torque: float = 0.0  # abs value

	for thruster in thrusters:
		var global_thrust_vector: Vector2 = thruster.get_thrust_at(1.0)
		var global_offset: Vector2 = thruster.global_position - rocket.to_global(rocket.center_of_mass)
		var torque: float = global_offset.cross(global_thrust_vector)
		torques[thruster] = torque

		var upward_thrust: float = global_thrust_vector.cross(Vector2.UP)
		var torque_effectiveness: float = 0.0 if upward_thrust == 0 else abs(torque / upward_thrust)
		effectiveness_entries.append({"thruster": thruster, "effectiveness": torque_effectiveness})

		if torque < 0:
			total_neg_torque -= torque
		else:
			total_pos_torque += torque

	# note: rocket.inertia is 0 if automatically computed
	if rocket.inertia == 0:
		rocket.inertia = 1.0 / PhysicsServer2D.body_get_direct_state(rocket.get_rid()).inverse_inertia

	var offset: float = Game.CENTRAL_X_COORDINATE - rocket.global_position.x
	var offset_correction: float = offset * X_OFFSET_CORRECTION_FACTOR
	var momentum: float = -rocket.linear_velocity.rotated(rocket.rotation).x
	var momentum_correction: float = momentum * X_MOMENTUM_CORRECTION_FACTOR

	var desired_rotation: float = clampf(offset_correction + momentum_correction, -0.25, 0.25) + player_steer * PLAYER_CONTROL_ROTATION
	var current_rotation: float = Util.rotation_relative_to_up(rocket.rotation)
	var rotation_difference: float = clampf(desired_rotation - current_rotation, -PI, PI)
	var desired_angular_velocity: float = rotation_difference * ANGLE_CORRECTION_SPEED
	var angular_velocity_difference: float = desired_angular_velocity - rocket.angular_velocity
	var target_torque: float = angular_velocity_difference * rocket.inertia * ANGLE_CORRECTION_DAMPENING
	var current_torque: float = total_pos_torque - total_neg_torque

	var desired_torque_change: float = (target_torque - current_torque) * TORQUE_CORRECTION_STRENGTH
	var total_torque_in_direction_of_desired: float = total_pos_torque if (current_torque > target_torque) else total_neg_torque

	# print("rocket.inertia = %.2f, rocket.angular_velocity = %s" % [rocket.inertia, rocket.angular_velocity])
	# print("desired_rotation = %.2f, offset_correction = %.2f, momentum_correction = %.2f" % [desired_rotation, offset_correction, momentum_correction])
	# print("rotation_difference = %.3f, desired_angular_velocity = %.6f, angular_velocity_difference = %.6f" % [rotation_difference, desired_angular_velocity, angular_velocity_difference])
	# print("current_torque = %s; target_torque = %s; desired_torque_change = %s" % [current_torque, target_torque, desired_torque_change])

	var accumulated_torque: float = 0.0
	var max_accumulated_torque: float = total_torque_in_direction_of_desired - absf(desired_torque_change)

	# LEAST torqueing thruster first
	effectiveness_entries.sort_custom(func(a, b): return a["effectiveness"] < b["effectiveness"])

	if effectiveness_entries.size() == 1:
		effectiveness_entries[0]["thruster"].thrust_factor = 1.0
		# print("Thruster targetPowerLevel = MAX (it is the only thruster)")
	else:
		for entry in effectiveness_entries:
			var thruster = entry["thruster"]
			var torque: float = torques[thruster]

			# if torque helps move total to target, go full blast
			if (torque > 0) == (target_torque > current_torque) \
				or is_inf(target_torque) \
				or absf(torque) < MINIMUM_CONTROL_TORQUE:
				thruster.thrust_factor = 1.0
				# print("Thruster targetPowerLevel = MAX (torque = %s)" % torque)
			else:
				# opposite torque, reduce power if we run out of budget
				var torque_budget_left: float = max_accumulated_torque - accumulated_torque
				var target_power_level: float = clampf(torque_budget_left / absf(torque), 0, 1)
				thruster.thrust_factor = target_power_level
				accumulated_torque += absf(torque) * target_power_level
				# print("Thruster targetPowerLevel = %s (torque = %s, effective = %s)" % [target_power_level, torque, entry["effectiveness"]])
