@tool
class_name ProtoDuctTape
extends Line2D

var _length: float = 0.0


func _ready() -> void:
	top_level = false
	add_point(Vector2.ZERO)
	add_point(Vector2.ZERO)
	while points.size() > 2:
		remove_point(2)


func global_anchor_a() -> Vector2:
	return to_global(points[0])


func global_anchor_b() -> Vector2:
	return to_global(points[1])


func realize() -> DuctTape:
	var duct_tape_scene: PackedScene = load("uid://dxtpf7xkx1g4k")
	var tape: DuctTape = duct_tape_scene.instantiate()
	tape._ready()

	# A
	var query := PhysicsPointQueryParameters2D.new()
	query.position = global_anchor_a()

	var hits: Array = get_world_2d().direct_space_state.intersect_point(query, 1)
	if hits.size() > 0:
		var collider = hits[0]["collider"]
		if collider is RocketComponent:
			tape.attach(collider, collider.to_local(global_anchor_a()))

	# B
	query = PhysicsPointQueryParameters2D.new()
	query.position = global_anchor_b()

	hits = get_world_2d().direct_space_state.intersect_point(query, 1)
	if hits.size() > 0:
		var collider = hits[0]["collider"]
		if collider is RocketComponent:
			tape.attach(collider, collider.to_local(global_anchor_b()))

	return tape
