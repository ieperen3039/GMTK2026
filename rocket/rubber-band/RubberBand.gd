class_name RubberBand
extends Node2D

enum StatusValue {
	EMPTY,
	HALF_CONNECTED,
	FULL_CONNECTED,
}

var _connector_scene: PackedScene
var _component_a: RigidBody2D = null
var _component_b: RigidBody2D = null
var _graphic: Line2D
const FORCE_FACTOR: float = 25.0
const MAX_FORCE: float = 10_000.0


func _ready() -> void:
	_connector_scene = load("uid://cwcg3yk8l3hi8")
	_graphic = get_node("Graphics")
	_graphic.top_level = false


var status: StatusValue:
	get:
		if _component_a == null:
			return StatusValue.EMPTY
		if _component_b == null:
			return StatusValue.HALF_CONNECTED
		return StatusValue.FULL_CONNECTED


func place(where: Vector2) -> void:
	match status:
		StatusValue.EMPTY:
			_component_a = _connector_scene.instantiate()
			_component_a.global_position = where
			add_child(_component_a)
			_graphic.add_point(where)
			_graphic.add_point(where)
			_component_a.freeze = true
			return

		StatusValue.HALF_CONNECTED:
			_component_b = _connector_scene.instantiate()
			_component_b.global_position = where
			add_child(_component_b)

			_component_a.freeze = false
			return

		StatusValue.FULL_CONNECTED:
			push_error("Already attached to two components")


func _physics_process(delta: float) -> void:
	if status == StatusValue.FULL_CONNECTED:
		_update_connected(delta)
	elif status == StatusValue.HALF_CONNECTED:
		_update_half_connected()


func _update_half_connected() -> void:
	var mouse_position: Vector2 = get_global_mouse_position()

	# update graphical part
	_graphic.set_point_position(0, to_local(_component_a.global_position))
	_graphic.set_point_position(1, to_local(mouse_position))


func _update_connected(_delta: float) -> void:
	_graphic.set_point_position(0, to_local(_component_a.global_position))
	_graphic.set_point_position(1, to_local(_component_b.global_position))

	# relative to A
	var gap_a_to_b: Vector2 = _component_b.global_position - _component_a.global_position
	var force: Vector2 = (gap_a_to_b * FORCE_FACTOR).limit_length(MAX_FORCE)

	_component_a.apply_central_force(force)
	_component_b.apply_central_force(-force)
