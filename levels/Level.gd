class_name Level
extends Node2D

signal next_level_requested
signal reset_requested
signal return_requested

const ALTITUDE_SCORE_ZONE: int = 500

@export var AltitudeGoal: int = 0

var _level_complete_scene: PackedScene
var _duct_tape_scene: PackedScene
var _rubber_band_scene: PackedScene
var _rocket_scene: PackedScene

var _camera: Camera2D
var _altitude_counter: Label
var _rocket_components: Array = []
var _duct_tape_instances_node: Node
var _selectables_node: Node
var _control_component
var _build_phase_bounds: CollisionObject2D
var _num_rocket_components: int = 0

var _default_mouse_tool
var _mouse_tool
var _tape_tool_button: Button
var _rubber_band_tool_button: Button

var _hovered_selectable: Grabbable

var _timer: Timer
var _timer_graphic: CountdownTimer

var _score: Score = Score.new()

var _num_crew_in_level: int = 0
var _is_level_complete: bool = false
var _should_build_rocket: bool = false
var _is_game_started: bool = false


func _ready() -> void:
	_level_complete_scene = load("uid://s62hk0dts0pl")
	_duct_tape_scene = load("uid://dxtpf7xkx1g4k")
	_rubber_band_scene = load("uid://c7lm4m43ofvbg")
	_rocket_scene = load("uid://dmdekhk5ugqao")

	_camera = get_node("Camera2D")
	var rocket_components_node: Node = get_node("RocketComponents")
	_timer = get_node("LevelTimer")
	_timer_graphic = get_node("%CountdownTimer")
	_build_phase_bounds = get_node("BuildPhaseBounds")
	_selectables_node = get_node("OtherSelectables")
	get_node("Finishline").position = Vector2(0, -AltitudeGoal)
	get_node("%GameUi").visible = false
	_altitude_counter = get_node("%AltitudeCounter")
	_altitude_counter.visible = false

	# reset ui offset
	var ui: CanvasLayer = get_node("UI")
	ui.offset = Vector2.ZERO

	var briefing: Briefing = get_node("%Briefing")
	briefing.start_button.pressed.connect(_start_game)
	briefing.main_menu_button.pressed.connect(func(): return_requested.emit())

	_duct_tape_instances_node = Node.new()
	_duct_tape_instances_node.name = "DuctTapeInstances"
	add_child(_duct_tape_instances_node)

	_default_mouse_tool = GrabTool.new(self)
	_mouse_tool = _default_mouse_tool

	for child in rocket_components_node.get_children():
		if child is RocketComponent:
			_num_rocket_components += 1
			child.freeze = true
			_rocket_components.append(child)
		else:
			for sub_child in child.get_children():
				if sub_child is RocketComponent:
					sub_child.freeze = true
					# add each part individually, because we cannot discern these at the end
					_num_rocket_components += 1
					_rocket_components.append(sub_child)

	for child in _selectables_node.get_children():
		if child is RigidBody2D:
			child.freeze = true

	# Setup the timer
	_timer.timeout.connect(_on_countdown_zero)


func _start_game() -> void:
	_is_game_started = true
	get_node("%Briefing").queue_free()
	get_node("%GameUi").visible = true

	# Setup buttons
	_tape_tool_button = get_node("%SetTapeTool")
	_tape_tool_button.toggled.connect(_set_tape_tool)
	_rubber_band_tool_button = get_node("%SetRubberBandTool")
	_rubber_band_tool_button.toggled.connect(_set_rubber_band_tool)
	var reset_button: Button = get_node("%Reset")
	reset_button.pressed.connect(func(): reset_requested.emit())
	_timer.start()

	var rng := RandomNumberGenerator.new()

	# setup grabbable listeners
	for part in _rocket_components:
		part.freeze = false

		Util.toss(part, rng)
		part.input_pickable = true
		part.mouse_entered.connect(func(): _on_hover_selectable(part, true))
		part.mouse_exited.connect(func(): _on_hover_selectable(part, false))

		if part is ControlComponent:
			if _control_component != null:
				push_error("Multiple control components: %s and %s" % [_control_component.name, part.name])
			else:
				_control_component = part

	for child in _selectables_node.get_children():
		if child is RigidBody2D:
			child.freeze = false

		if child is Grabbable:
			child.input_pickable = true
			child.mouse_entered.connect(func(): _on_hover_selectable(child, true))
			child.mouse_exited.connect(func(): _on_hover_selectable(child, false))

		if child is CrewMember:
			_num_crew_in_level += 1
			child.walk_target = _control_component

	if _control_component == null:
		push_error("No control components in scene")


func _on_hover_selectable(part: Grabbable, set_active: bool) -> void:
	if not set_active:
		if _hovered_selectable == part:
			_hovered_selectable = null
	else:
		print("Hovering %s" % part.name)
		_hovered_selectable = part


func _physics_process(_delta: float) -> void:
	if not _is_game_started:
		_timer_graphic.set_value(_timer.wait_time)
	else:
		_timer_graphic.set_value(_timer.time_left)

	if Input.is_action_just_pressed("toggle_tape") and not _tape_tool_button.disabled:
		# set active if not already active
		_tape_tool_button.set_pressed(not _tape_tool_button.is_pressed())

	if _should_build_rocket:
		_should_build_rocket = false
		for part in _rocket_components:
			part.modulate = Color.GRAY

		var rocket: Rocket = _rocket_scene.instantiate()
		rocket.altitude_changed.connect(_check_victory)
		rocket.add_all_nearby_recursively(_control_component)
		add_child(rocket)

		_altitude_counter.text = ""
		_altitude_counter.visible = true
		rocket.altitude_changed.connect(func(alt: float): _altitude_counter.text = str(int(alt)) + " m")

		_camera.reparent(rocket.control_component)
		get_tree().create_tween() \
			.tween_property(_camera, "position", Vector2.ZERO, 1.0) \
			.set_ease(Tween.EASE_OUT)


func _set_tape_tool(set_active: bool) -> void:
	_set_mouse_tool(TapeTool.new(self) if set_active else _default_mouse_tool)


func _set_rubber_band_tool(set_active: bool) -> void:
	_set_mouse_tool(RubberBandTool.new(self) if set_active else _default_mouse_tool)


# attach camera to largest component tree, activate all engines
func _on_countdown_zero() -> void:
	_build_phase_bounds.process_mode = Node.PROCESS_MODE_DISABLED

	# all thrusters to 100%
	for part in _rocket_components:
		if part is Thruster:
			part.activate_thruster()

	# building the rocket must happen on the physics thread
	_should_build_rocket = true


func _check_victory(altitude: float) -> void:
	if altitude > AltitudeGoal and not _is_level_complete:
		# TODO show warning that not all crew are present
		if _control_component is CrewCompartment and _control_component.num_crew_inside < _num_crew_in_level:
			return

		print("Level Complete!")
		_is_level_complete = true
		_on_level_complete()


func _on_level_complete() -> void:
	# first count the score
	_score = Score.new()
	_score.total_components = _num_rocket_components

	var minimum_altitude_to_count: float = AltitudeGoal - ALTITUDE_SCORE_ZONE
	for part in _rocket_components:
		if -part.global_position.y < minimum_altitude_to_count:
			continue
		_score.num_lifted_components += 1

	for node in _selectables_node.get_children():
		if not (node is RigidBody2D):
			continue
		if -node.global_position.y < minimum_altitude_to_count:
			continue
		_score.num_extras += 1

	var level_complete_screen: LevelComplete = _level_complete_scene.instantiate()
	# chain level complete signal to this level complete signal
	level_complete_screen.next_level_requested.connect(func(): next_level_requested.emit())

	level_complete_screen.score = _score
	_camera.reparent(level_complete_screen)
	add_child(level_complete_screen)

	_altitude_counter.visible = false


func _set_mouse_tool(new_mouse_tool) -> void:
	_mouse_tool.on_cancel()
	_mouse_tool = new_mouse_tool
	_tape_tool_button.set_pressed_no_signal(new_mouse_tool is TapeTool)
	_rubber_band_tool_button.set_pressed_no_signal(new_mouse_tool is RubberBandTool)
	print("mouse_tool = %s" % new_mouse_tool.get_script().get_global_name())


func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_RIGHT:
			_set_mouse_tool(_default_mouse_tool)
		elif event.button_index == MOUSE_BUTTON_LEFT:
			# use get_global_mouse_position instead of event.position;
			# event.position is relative to viewport
			if event.is_pressed():
				_mouse_tool.on_click(get_global_mouse_position())
			elif event.is_released():
				_mouse_tool.on_release(get_global_mouse_position())


func get_score() -> Score:
	return _score


# player can apply tape to rocket components
class TapeTool:
	var parent: Level
	var tape: DuctTape

	func _init(p: Level) -> void:
		parent = p
		tape = _new_tape()

	func _new_tape() -> DuctTape:
		var new_tape: DuctTape = parent._duct_tape_scene.instantiate()
		parent._duct_tape_instances_node.add_child(new_tape)
		return new_tape

	func on_click(mouse_position: Vector2) -> void:
		var selectable: Grabbable = parent._hovered_selectable
		if selectable is RocketComponent:
			var relative_click: Vector2 = selectable.to_local(mouse_position)
			tape.attach(selectable, relative_click)

			if tape.status == DuctTape.StatusValue.EMPTY:
				# avoid edge case
				on_cancel()
				tape = _new_tape()

	func on_release(mouse_position: Vector2) -> void:
		var selectable: Grabbable = parent._hovered_selectable
		if selectable is RocketComponent:
			var relative_click: Vector2 = selectable.to_local(mouse_position)
			tape.attach(selectable, relative_click)

			if tape.status == DuctTape.StatusValue.FULL_CONNECTED:
				tape = _new_tape()
			else:
				on_cancel()
				tape = _new_tape()
		else:
			on_cancel()
			tape = _new_tape()

	func on_cancel() -> void:
		tape.snap()
		tape.queue_free()


# player can grab rocket components
class GrabTool:
	var parent: Level
	var grabbed: Grabbable

	func _init(p: Level) -> void:
		parent = p
		grabbed = null

	func on_click(mouse_position: Vector2) -> void:
		var thing: Grabbable = parent._hovered_selectable
		if thing is Grabbable:
			# prevent grabbing rocket components
			if thing is RocketComponent:
				var component: RocketComponent = thing
				if component.part_of_rocket:
					return

			grabbed = thing
			var relative_click: Vector2 = thing.to_local(mouse_position)
			thing.on_grab(relative_click)

	func on_release(_mouse_position: Vector2) -> void:
		on_cancel()

	func on_cancel() -> void:
		if grabbed != null:
			grabbed.on_release()
		grabbed = null


class RubberBandTool:
	var parent: Level
	var band: RubberBand

	func _init(p: Level) -> void:
		parent = p
		band = _new_band()

	func _new_band() -> RubberBand:
		var new_band: RubberBand = parent._rubber_band_scene.instantiate()
		parent._duct_tape_instances_node.add_child(new_band)
		return new_band

	func on_click(mouse_position: Vector2) -> void:
		band.place(mouse_position)

		if band.status == RubberBand.StatusValue.EMPTY:
			# avoid edge case
			on_cancel()
			band = _new_band()

	func on_release(mouse_position: Vector2) -> void:
		band.place(mouse_position)

		if band.status == RubberBand.StatusValue.FULL_CONNECTED:
			band = _new_band()
		else:
			on_cancel()
			band = _new_band()

	func on_cancel() -> void:
		band.queue_free()


# player can't do anything
class NullTool:
	func on_cancel() -> void:
		pass

	func on_click(_mouse_position: Vector2) -> void:
		pass

	func on_release(_mouse_position: Vector2) -> void:
		pass
