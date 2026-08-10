class_name Util

# mod rotation to (-180, +180) degrees
static func rotation_relative_to_up(angle: float) -> float:
	return fposmod(angle + PI, 2 * PI) - PI


static func toss(target: RigidBody2D, rng: RandomNumberGenerator, max_velocity: float = 50.0, max_rotation: float = 10.0) -> void:
	target.angular_velocity = max_rotation * rng.randf()
	target.linear_velocity = _random_unit_vector(rng) * max_velocity


static func _random_unit_vector(rng: RandomNumberGenerator) -> Vector2:
	return Vector2(1, 0).rotated(2 * PI * rng.randf())
