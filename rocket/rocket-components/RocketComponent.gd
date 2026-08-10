class_name RocketComponent
extends Grabbable

const ROCKET_CHECK_MARGIN: float = 5.0
const ANGLE_PULL: float = 5.0

var thrust_sources: Array = []
var magnets: Array = []
var _collision_boxes: Array = []  # each entry: {"shape": Shape2D, "owner": Node2D}

var part_of_rocket: bool = false


func _ready() -> void:
	super._ready()

	collision_layer = Game.COLLISION_LAYER_PRIMARY | Game.COLLISION_LAYER_GRABBABLE
	angular_damp = 1.0
	mouse_entered.connect(_on_mouse_entered)
	mouse_exited.connect(_on_mouse_exited)

	for child in get_children():
		if child is ThrustSource:
			thrust_sources.append(child)
		elif child is Magnet:
			magnets.append(child)
		elif child is CollisionShape2D:
			_collision_boxes.append({"shape": child.shape, "owner": child})
		elif child is CollisionPolygon2D:
			for piece in Geometry2D.decompose_polygon_in_convex(child.polygon):
				var convex := ConvexPolygonShape2D.new()
				convex.points = piece
				_collision_boxes.append({"shape": convex, "owner": child})


func _physics_process(delta: float) -> void:
	super._physics_process(delta)

	if freeze:
		return

	if not is_dragging:
		# note that thruster should be off at the start of the game
		for thruster in thrust_sources:
			if not thruster.enable_thrust:
				continue

			var global_thrust_vector: Vector2 = thruster.get_thrust()
			var global_offset: Vector2 = thruster.global_position - global_position
			apply_force(global_thrust_vector, global_offset)

	for magnet in magnets:
		var global_thrust_vector: Vector2 = magnet.get_force()
		var global_offset: Vector2 = magnet.global_position - global_position
		apply_force(global_thrust_vector, global_offset)


func get_nearby_bodies() -> Array:
	var results: Array = []

	for box in _collision_boxes:
		var query := PhysicsShapeQueryParameters2D.new()
		query.shape = box["shape"]
		query.transform = box["owner"].global_transform
		query.margin = ROCKET_CHECK_MARGIN
		query.collide_with_bodies = true
		query.collide_with_areas = false
		query.exclude = [get_rid()]

		var hits: Array = get_world_2d().direct_space_state.intersect_shape(query, 8)
		for hit in hits:
			var collider = hit["collider"]
			if collider is RigidBody2D:
				results.append(collider)

	return results


func _on_mouse_entered() -> void:
	pass


func _on_mouse_exited() -> void:
	pass
