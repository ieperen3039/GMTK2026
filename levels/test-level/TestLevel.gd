class_name TestLevel
extends Node2D

var _rocket: Rocket
var _center_of_mass: Node2D
var _rocket_components: Array = []
var _duct_tape_instances_node: Node


func _ready() -> void:
	var rocket_components_node: Node = get_node("RocketComponents")
	_duct_tape_instances_node = get_node("DuctTapeInstances")
	var rocket_scene: PackedScene = load("uid://dmdekhk5ugqao")
	var duct_tape_scene: PackedScene = load("uid://dxtpf7xkx1g4k")

	_rocket = rocket_scene.instantiate()

	for child in rocket_components_node.get_children():
		if child is RocketComponent:
			_rocket_components.append(child)
			_rocket.add_component(child)
			child.linear_damp = 100

	# tape everything
	for part in _rocket_components:
		for part2 in _rocket_components:
			if part == part2:
				continue
			if part.global_position.distance_to(part2.global_position) > 46:
				continue

			var tape: DuctTape = duct_tape_scene.instantiate()
			_duct_tape_instances_node.add_child(tape)

			tape.attach(part, Vector2.ZERO)
			tape.attach(part2, Vector2.ZERO)

	get_node("Camera2D").reparent(_rocket.control_component, false)
	_center_of_mass = get_node("ComIndicator")
	_center_of_mass.position = _rocket.to_global(_rocket.center_of_mass)
	_center_of_mass.visible = true

	add_child(_rocket)


func _physics_process(_delta: float) -> void:
	for child in _duct_tape_instances_node.get_children():
		if child is ProtoDuctTape:
			var tape: DuctTape = child.realize()
			_duct_tape_instances_node.add_child(tape)
			child.queue_free()

	for part in _rocket_components:
		part.linear_damp = 0

	_center_of_mass.position = _rocket.to_global(_rocket.center_of_mass)
