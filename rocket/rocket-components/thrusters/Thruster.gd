class_name Thruster
extends RocketComponent

var _particles: Dictionary = {}  # CpuParticles2D -> original lifetime (float)
var _sfx: ThrusterSFX


func _ready() -> void:
	super._ready()
	_sfx = get_node("ThrusterSFX")
	_sfx.playing = false

	var particles_node: Node2D = get_node("ExhaustParticles")
	particles_node.visible = true
	for node in particles_node.get_children():
		if node is CPUParticles2D:
			_particles[node] = node.lifetime
			node.emitting = false


func _process(delta: float) -> void:
	super._process(delta)

	var average_thrust_factor: float = 0.0
	var num_thrusters: int = 0
	for thruster in thrust_sources:
		average_thrust_factor += thruster.thrust_factor
		num_thrusters += 1
	average_thrust_factor /= num_thrusters

	for particle in _particles:
		var original_lifetime: float = _particles[particle]
		if average_thrust_factor > 0.1:
			particle.emitting = true
			# squared thrust factor to make the effect more noticable
			particle.lifetime = original_lifetime * average_thrust_factor * average_thrust_factor
		else:
			particle.emitting = false


func activate_thruster() -> void:
	# Enable the thrust forces
	for thruster in thrust_sources:
		thruster.set_activation_thrust_factor()

	# Enable the sound
	_sfx.start_engine()

	# Enable the visuals
	for particle in _particles:
		particle.emitting = true
