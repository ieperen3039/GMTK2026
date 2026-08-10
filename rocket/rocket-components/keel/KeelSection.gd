class_name KeelSection
extends RocketComponent

const STIFFNESS: int = 75
const SPRING_LENGTH: int = 4

@export var ConnectedTo: KeelSection


func _ready() -> void:
	var joint_position: Vector2 = get_node("Joint").position

	if ConnectedTo != null:
		var pin := PinJoint2D.new()
		pin.position = joint_position
		pin.node_a = get_path()
		pin.node_b = ConnectedTo.get_path()
		add_child(pin)

		var spring_a := DampedSpringJoint2D.new()
		spring_a.position = joint_position + Vector2(64, 0)
		spring_a.node_a = get_path()
		spring_a.node_b = ConnectedTo.get_path()
		spring_a.length = 2 * SPRING_LENGTH
		spring_a.rest_length = SPRING_LENGTH
		spring_a.stiffness = STIFFNESS
		add_child(spring_a)

		var spring_b := DampedSpringJoint2D.new()
		spring_b.position = joint_position - Vector2(64, 0)
		spring_b.node_a = get_path()
		spring_b.node_b = ConnectedTo.get_path()
		spring_b.length = 2 * SPRING_LENGTH
		spring_b.rest_length = SPRING_LENGTH
		spring_b.stiffness = STIFFNESS
		add_child(spring_b)


func _process(_delta: float) -> void:
	pass
