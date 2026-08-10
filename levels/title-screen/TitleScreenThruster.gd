class_name TitleScreenThruster
extends RigidBody2D

@export var ThrustPower: float = 0.0
@export var TimeUntilBlastoff: float = 10.0
@export var ThrustTime: float = 5.0
var is_blasting_off: bool = false

var _particles: Array = []


func _ready() -> void:
	var particles_node: Node2D = get_node("ExhaustParticles")
	for node in particles_node.get_children():
		if node is CpuParticles2D:
			_particles.append(node)
			node.emitting = false
			node.visible = true


func _physics_process(delta: float) -> void:
	TimeUntilBlastoff -= delta

	if TimeUntilBlastoff < -ThrustTime:
		if is_blasting_off:
			is_blasting_off = false
			for particle in _particles:
				particle.emitting = false
	elif TimeUntilBlastoff < 0:
		if not is_blasting_off:
			is_blasting_off = true

			print("particle.Emitting = true")
			for particle in _particles:
				particle.emitting = true

	if is_blasting_off:
		var undulation: float = sin(TimeUntilBlastoff * 10) * 0.2

		var force: Vector2 = global_transform.basis_xform(Vector2.UP * ThrustPower).rotated(undulation)
		apply_central_force(force)

		for particle in _particles:
			particle.rotation = undulation
