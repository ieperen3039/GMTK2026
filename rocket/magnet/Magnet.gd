class_name Magnet
extends Area2D

@export var PullStrength: float = 250.0

var connected_to: Magnet = null


func _ready() -> void:
	monitoring = true
	area_entered.connect(_on_shape_enter)
	area_exited.connect(_on_shape_exit)
	collision_layer = Game.COLLISION_LAYER_MAGNET
	collision_mask = Game.COLLISION_LAYER_MAGNET


func _on_shape_enter(area: Area2D) -> void:
	if area is Magnet:
		connected_to = area


func _on_shape_exit(area: Area2D) -> void:
	if area is Magnet and connected_to != null:
		connected_to = null


func get_force() -> Vector2:
	if connected_to == null:
		return Vector2.ZERO

	return global_position.direction_to(connected_to.global_position) * PullStrength
