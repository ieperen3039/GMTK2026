class_name CrewMember
extends Grabbable

const ANIMATION_NAME_WALK: String = "Walk"
const ANIMATION_NAME_FALL: String = "Fall"
const ANIMATION_NAME_STAND: String = "Stand"
const WALK_FORCE_FACTOR: float = 50.0
const TARGET_WALK_SPEED: float = 5.0
const FALL_VELOCITY: float = 10.0

var walk_target: Node2D
var _walk_force: float = 0.0
var _walk_left: bool = true

var _animation: AnimatedSprite2D


func _ready() -> void:
	walk_target = self

	super._ready()
	_animation = get_node("Animation")
	_animation.play(ANIMATION_NAME_STAND)
	_walk_force = physics_material_override.friction * WALK_FORCE_FACTOR

	lock_rotation = true


func _process(delta: float) -> void:
	super._process(delta)
	var is_at_target: bool = absf(global_position.x - walk_target.global_position.x) < 0.1

	match _animation.animation:
		ANIMATION_NAME_STAND:
			if sleeping == true and not is_at_target:
				_animation.play(ANIMATION_NAME_WALK)
			if linear_velocity.y > 1.0:
				_animation.play(ANIMATION_NAME_FALL)
		ANIMATION_NAME_WALK:
			if is_at_target:
				_animation.play(ANIMATION_NAME_STAND)
			elif linear_velocity.length() > FALL_VELOCITY:
				_animation.play(ANIMATION_NAME_FALL)
		ANIMATION_NAME_FALL:
			if sleeping == true:
				_animation.play(ANIMATION_NAME_WALK)
			if linear_velocity.y <= 0:
				_animation.play(ANIMATION_NAME_STAND)
		_:
			print("Unhandled animation '%s'" % _animation.animation)
			_animation.play(ANIMATION_NAME_WALK)


func _physics_process(delta: float) -> void:
	super._physics_process(delta)

	_walk_left = global_position.x < walk_target.global_position.x

	if _walk_left:
		_animation.scale = Vector2(-1, 1)
	else:
		_animation.scale = Vector2(1, 1)

	if not is_dragging and _animation.animation == ANIMATION_NAME_WALK:
		sleeping = false
		var fraction_of_target_speed: float = absf(linear_velocity.x / TARGET_WALK_SPEED)
		var total_walk_force: float = _walk_force * (1.1 - clampf(fraction_of_target_speed, 0, 1))

		if _walk_left:
			# also pull up a little for the sake of figting friction
			apply_central_force(Vector2(total_walk_force, -mass * 400))
		else:
			# also pull up a little for the sake of figting friction
			apply_central_force(Vector2(-total_walk_force, -mass * 400))
