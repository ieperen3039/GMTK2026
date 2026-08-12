class_name Grabbable
extends RigidBody2D

const SNAP_SPEED: float = 20.0
const SNAP_DAMPENING: float = 30.0
const MAX_SPEED_WHEN_DRAGGING: float = 1000.0

const CLANG_VELOCITY_FACTOR: float = 0.2
const CLANG_VOLUME_FACTOR: float = 1.0 / 1000.0
const CLANG_VELOCITY_DELTA: float = 5.0

var is_dragging: bool = false
var _local_grab_offset: Vector2 = Vector2.ZERO
var _original_material: PhysicsMaterial
var _pull_factor: float = 1.0

var _clang_volume_drop_off: float = 0.0
var _last_measured_velocity_for_clang: Vector2 = Vector2.ZERO
var _sfx_player: AudioStreamPlayer2D
var _adjusted_previous_sfx_volume: float = 0.0


func _ready() -> void:
	input_pickable = true
	collision_layer |= Game.COLLISION_LAYER_GRABBABLE
	max_contacts_reported = 1
	contact_monitor = true
	_original_material = physics_material_override

	_sfx_player = get_node_or_null("ClangSfx")
	if _sfx_player != null:
		_clang_volume_drop_off = _sfx_player.pitch_scale / _sfx_player.stream.get_length()


func _physics_process(_delta: float) -> void:
	if is_dragging and not freeze:
		var target_position: Vector2 = get_global_mouse_position()
		var direction: Vector2 = target_position - to_global(_local_grab_offset)
		var target_velocity: Vector2 = direction * SNAP_SPEED
		var velocity_difference: Vector2 = target_velocity - linear_velocity
		var global_offset: Vector2 = global_transform.basis_xform(_local_grab_offset)
		# for small masses, reduce the force to avoid slingshotting
		apply_force(velocity_difference * SNAP_DAMPENING * _pull_factor * minf(0.5, mass), global_offset)
		linear_velocity = linear_velocity.limit_length(MAX_SPEED_WHEN_DRAGGING)


func _process(delta: float) -> void:
	if _sfx_player != null:
		_update_sound(delta)


func _update_sound(delta: float) -> void:
	_adjusted_previous_sfx_volume -= delta * _clang_volume_drop_off
	var curr_speed: float = linear_velocity.length()
	var prev_speed: float = _last_measured_velocity_for_clang.length()

	var did_stop: bool = curr_speed < prev_speed * CLANG_VELOCITY_FACTOR
	var is_significant: bool = prev_speed > CLANG_VELOCITY_DELTA
	if did_stop and is_significant:
		var volume: float = (prev_speed - curr_speed - CLANG_VELOCITY_DELTA) * CLANG_VOLUME_FACTOR
		if volume > _adjusted_previous_sfx_volume:
			_adjusted_previous_sfx_volume = clampf(volume, 0, 1)
			play_collision_sound(_sfx_player, _adjusted_previous_sfx_volume)

	_last_measured_velocity_for_clang = linear_velocity


func play_collision_sound(player: AudioStreamPlayer2D, volume: float) -> void:
	player.volume_linear = volume
	player.play()


func on_release() -> void:
	is_dragging = false
	physics_material_override = _original_material


func on_grab(local_grab_offset: Vector2, pull_factor: float = 1.0) -> void:
	_local_grab_offset = local_grab_offset
	_pull_factor = pull_factor
	is_dragging = true

	var mat := PhysicsMaterial.new()
	mat.friction = 0.1
	mat.bounce = 0.1
	physics_material_override = mat
