class_name RandomComponentGenerator
extends Node2D

const TIME_BETWEEN_ADD: float = 1.0
var _time_until_new_add: float = 0.0
var _components_added: int = 0

var _spawn_position: Vector2
var _component_scenes: Array = []  # each entry: {"scene": PackedScene, "weight": int}
var _total_weight: int = 0
var _rng := RandomNumberGenerator.new()
var _timer: CountdownTimer


func _ready() -> void:
	_spawn_position = get_node("SpawnPosition").position
	_component_scenes = [
		{"scene": load("uid://3ypjldxcxkvw"), "weight": 10}, # tank
		{"scene": load("uid://d3sd7kyiugv60"), "weight": 5}, # cone
		{"scene": load("uid://c4x5k3q1n002b"), "weight": 4}, # thruster
		{"scene": load("uid://b5v30djg1rxq8"), "weight": 5}, # mini thruster
		{"scene": load("uid://73ile0xgnbys"), "weight": 2}, # traffic cone
		{"scene": load("uid://s7v7dbr5g7n4"), "weight": 1}, # bowling ball
	]

	for entry in _component_scenes:
		_total_weight += entry["weight"]

	_timer = get_node("%CountdownTimer")
	_timer.Quiet = true


func _process(delta: float) -> void:
	_time_until_new_add -= delta
    # at around 100 components, we reach critical mass
	if _time_until_new_add < 0 && _components_added < 100:
		_time_until_new_add += TIME_BETWEEN_ADD
		_components_added += 1
		_timer.Value = _components_added
		# add component

		var selection: int = _rng.randi() % _total_weight
		for entry in _component_scenes:
			selection -= entry["weight"]
			if selection < 0:
				var component: RigidBody2D = entry["scene"].instantiate()
				component.global_position = _spawn_position
				Util.toss(component, _rng, 100, 20)
				add_child(component)
				break
