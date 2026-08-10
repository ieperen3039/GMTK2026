class_name Rocket
extends RigidBody2D

signal altitude_changed(altitude: float)

const MAX_ROCKET_COMPONENTS: int = 100
const PLAYER_CONTROL_TORQUE: float = 10000.0
# in pixels/s
const MAX_VELOCITY_DELTA: float = 500.0
# in pixels
const MAX_DISTANCE: float = 75.0
const MAX_DISTANCE_CONTROL: float = 500.0
const MAX_DISTANCE_CONTROL_SQUARED: float = MAX_DISTANCE_CONTROL * MAX_DISTANCE_CONTROL
const MAX_DISTANCE_SQUARED: float = MAX_DISTANCE * MAX_DISTANCE
const MAX_VELOCITY_DELTA_SQUARED: float = MAX_VELOCITY_DELTA * MAX_VELOCITY_DELTA

var _rocket_indicator_color: Color = Color.WHITE

var control_component

var _thrusters: Array = []
var _components: Array = []
var _is_empty: bool = true


func _ready() -> void:
	# this rigid body will be a ephemeral representation of the rocket
	freeze = true
	contact_monitor = false
	center_of_mass_mode = RigidBody2D.CENTER_OF_MASS_MODE_CUSTOM
	_recompute_mass_distribution()


func _physics_process(_delta: float) -> void:
	# copy basic physics properties from control component
	global_transform = control_component.global_transform
	linear_velocity = control_component.linear_velocity
	angular_velocity = control_component.angular_velocity

	_remove_fallen_components()
	_recompute_mass_distribution()

	var right_steer: float = Input.get_axis("move_left", "move_right")
	control_component.apply_torque(PLAYER_CONTROL_TORQUE * right_steer)

	DynamicThrustReduction.balance_thrusters(self, _thrusters, right_steer)

	# negative Y is up
	altitude_changed.emit(-control_component.global_position.y)


func _remove_fallen_components() -> void:
	var average_velocity: Vector2 = Vector2.ZERO
	var component_distances_sq: Dictionary = {}
	for component in _components:
		average_velocity += component.linear_velocity

		var smallest_distance_sq: float = INF
		for component2 in _components:
			if component == component2:
				continue
			var dist: float = component.global_position.distance_squared_to(component2.global_position)
			if dist < smallest_distance_sq:
				smallest_distance_sq = dist

		component_distances_sq[component] = smallest_distance_sq
	average_velocity /= _components.size()

	var to_remove: Array = []
	for component in _components:
		# avoid ditching the control component
		if component == control_component:
			continue

		var distance_sq_to_closest: float = component_distances_sq[component]
		var distance_sq_to_control: float = component.global_position.distance_squared_to(global_position)
		var velocity_delta_sq: float = component.linear_velocity.distance_squared_to(average_velocity)
		if distance_sq_to_control > MAX_DISTANCE_CONTROL_SQUARED \
			or distance_sq_to_closest > MAX_DISTANCE_SQUARED \
			or velocity_delta_sq > MAX_VELOCITY_DELTA_SQUARED:
			print("Dropping %s from Rocket (closest = %s control = %s dv = %s)" % [
				component.name, sqrt(distance_sq_to_closest), sqrt(distance_sq_to_control), sqrt(velocity_delta_sq)
			])
			to_remove.append(component)
			component.modulate = Color.GRAY
			for thruster in component.thrust_sources:
				thruster.set_activation_thrust_factor()
				_thrusters.erase(thruster)

	for component in to_remove:
		_components.erase(component)


func _recompute_mass_distribution() -> void:
	# assume orientation of control_component
	global_transform = control_component.global_transform

	var new_center_of_mass: Vector2 = Vector2.ZERO
	var new_mass: float = 0.0
	for component in _components:
		var local_center_of_mass: Vector2 = to_local(component.to_global(component.center_of_mass))
		new_center_of_mass += local_center_of_mass * component.mass
		new_mass += component.mass
	new_center_of_mass /= new_mass

	center_of_mass = new_center_of_mass
	mass = new_mass

	var new_inertia: float = 0.0
	for component in _components:
		var local_center_of_mass: Vector2 = to_local(component.to_global(component.center_of_mass))
		var dist_sq: float = local_center_of_mass.distance_squared_to(center_of_mass)
		new_inertia += component.inertia + component.mass * dist_sq

	inertia = new_inertia


func add_component(component: RocketComponent) -> void:
	print("Add %s to Rocket" % component.name)
	_components.append(component)
	component.modulate = _rocket_indicator_color

	_thrusters.append_array(component.thrust_sources)

	if component is ControlComponent:
		if control_component != null:
			push_error("Double control component")
		else:
			control_component = component


func add_all_nearby_recursively(core: ControlComponent) -> void:
	var nodes_to_check: Array = [core]
	var nodes_seen: Array = [core]
	add_component(core)
	core.part_of_rocket = true

	var iterations_until_break: int = MAX_ROCKET_COMPONENTS
	while nodes_to_check.size() > 0 and iterations_until_break > 0:
		iterations_until_break -= 1
		var node_to_check: RocketComponent = nodes_to_check[0]
		nodes_to_check.remove_at(0)

		# find all components connected to node_to_check.
		# add all of them to a new Rocket
		for near in node_to_check.get_nearby_bodies():
			if not (near is RocketComponent):
				continue
			var component: RocketComponent = near
			if nodes_seen.has(component):
				continue

			component.part_of_rocket = true
			add_component(component)
			nodes_to_check.append(component)
			nodes_seen.append(component)


func get_thrusters() -> Array:
	return _thrusters
