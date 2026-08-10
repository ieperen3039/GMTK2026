class_name DuctTape
extends Node2D

enum StatusValue {
	EMPTY,
	HALF_CONNECTED, # ComponentA is held by us
	FULL_CONNECTED,
}

const MOUSE_PULL_FACTOR: float = 0.05
const SNAP_SPEED: float = 1000.0
const SNAP_DAMPENING: float = 1.0
const MAX_FORCE: float = 100_000.0

var component_a: RocketComponent = null
var _anchor_a: Vector2 = Vector2.ZERO
var component_b: RocketComponent = null
var _anchor_b: Vector2 = Vector2.ZERO

var _graphic: Line2D
var _length: float = 0.0


func _ready() -> void:
	_graphic = get_node("Graphics")
	_graphic.top_level = false


var status: StatusValue:
	get:
		if component_a == null:
			return StatusValue.EMPTY
		if component_b == null:
			return StatusValue.HALF_CONNECTED
		return StatusValue.FULL_CONNECTED


func attach(component: RocketComponent, local_attachment_position: Vector2) -> void:
	match status:
		StatusValue.EMPTY:
			print("Tape attach A")
			# set position to make it easier later when converting to rocket
			position = component.position
			component_a = component
			_anchor_a = local_attachment_position
			component_a.on_grab(local_attachment_position, MOUSE_PULL_FACTOR)
			var line_point: Vector2 = to_local(global_anchor_a())
			_graphic.add_point(line_point)
			_graphic.add_point(line_point)
			return

		StatusValue.HALF_CONNECTED:
			component_a.on_release()
			if component == component_a:
				print("Tape detach A")
				component_a = null
				return

			print("Tape attach B")
			component_b = component
			_anchor_b = local_attachment_position

			_length = global_anchor_a().distance_to(global_anchor_b())
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
	var anchor_a: Vector2 = global_anchor_a()
	_graphic.set_point_position(0, to_local(anchor_a))
	_graphic.set_point_position(1, to_local(mouse_position))

	# component A pull is handled by RocketComponent


func _update_connected(_delta: float) -> void:
	var anchor_a: Vector2 = global_anchor_a()
	var anchor_b: Vector2 = global_anchor_b()

	_graphic.set_point_position(0, to_local(anchor_a))
	_graphic.set_point_position(1, to_local(anchor_b))

	# relative to A
	var gap_a_to_b: Vector2 = anchor_b - anchor_a
	var modified_length: float = clampf(gap_a_to_b.length() - _length, 1.0, _length * 2.0)
	var target_movement: Vector2 = gap_a_to_b.normalized() * modified_length
	var target_velocity: Vector2 = target_movement * SNAP_SPEED
	var velocity_difference: Vector2 = target_velocity - (component_a.linear_velocity - component_b.linear_velocity)
	var force: Vector2 = (velocity_difference * SNAP_DAMPENING).limit_length(MAX_FORCE)

	component_a.apply_force(force, anchor_a - component_a.global_position)
	component_b.apply_force(-force, anchor_b - component_b.global_position)

	if gap_a_to_b.length() > _length * 2:
		snap()


func snap() -> void:
	if status == StatusValue.HALF_CONNECTED:
		component_a.on_release()

	component_a = null
	component_b = null
	_graphic.clear_points()


func global_anchor_a() -> Vector2:
	return component_a.to_global(_anchor_a)


func global_anchor_b() -> Vector2:
	return component_b.to_global(_anchor_b)


func reparent_graphics(new_parent: Node2D) -> void:
	# this also takes care of the global-to-local conversions of the current points
	_graphic.reparent(new_parent)
	_graphic = null
