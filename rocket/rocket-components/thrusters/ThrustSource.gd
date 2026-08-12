@tool
class_name ThrustSource
extends Node2D

@export var ThrustPower: float = 0.0

var thrust_factor: float = 0.0

var enable_thrust: bool:
	get: return thrust_factor > 0.0


func set_activation_thrust_factor() -> void:
	thrust_factor = 1.0


func get_thrust() -> Vector2:
	return get_thrust_at(thrust_factor)


# returns global thrust vector
func get_thrust_at(fraction_of_full: float) -> Vector2:
	return get_local_thrust_at(fraction_of_full).rotated(global_rotation)


func get_local_thrust() -> Vector2:
	return get_local_thrust_at(thrust_factor)


func get_local_thrust_at(fraction_of_full: float) -> Vector2:
	return Vector2.UP * ThrustPower * fraction_of_full


func _draw() -> void:
	if Engine.is_editor_hint():
		draw_line(Vector2.ZERO, get_local_thrust_at(1.0) * -0.1, Color(1, 0, 0, 0.5), 2.0)
		draw_line(Vector2.ZERO, Vector2(5, 5), Color(1, 0, 0, 0.5), 2.0)
		draw_line(Vector2.ZERO, Vector2(-5, 5), Color(1, 0, 0, 0.5), 2.0)
