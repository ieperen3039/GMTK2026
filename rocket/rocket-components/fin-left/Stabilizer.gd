class_name Stabilizer
extends RocketComponent

const DRAG_FACTOR: float = 0.01


func _physics_process(delta: float) -> void:
	apply_central_force(-linear_velocity * DRAG_FACTOR)

	super._physics_process(delta)
