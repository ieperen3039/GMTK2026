class_name CrewCompartment
extends ControlComponent

var _full_sprite: Sprite2D
var _empty_sprite: Sprite2D
var _is_filled: bool = false
var num_crew_inside: int = 0


func _ready() -> void:
	super._ready()

	_full_sprite = get_node("Full")
	_empty_sprite = get_node("Empty")
	var collection_hitbox: Area2D = get_node("CrewCollectionHitbox")
	collection_hitbox.body_entered.connect(_on_body_enter)

	set_filled(false)


func _on_body_enter(body: Node2D) -> void:
	if body is CrewMember:
		# eat
		set_filled(true)
		num_crew_inside += 1
		mass += body.mass
		body.on_release()
		body.visible = false
		body.process_mode = Node.PROCESS_MODE_DISABLED
		body.reparent(self, false)
		body.position = Vector2.ZERO


func set_filled(filled: bool) -> void:
	_is_filled = filled
	_full_sprite.visible = _is_filled
	_empty_sprite.visible = not _is_filled
