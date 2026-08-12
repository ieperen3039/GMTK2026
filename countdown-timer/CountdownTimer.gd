class_name CountdownTimer
extends RocketComponent

@export var Quiet: bool = false

var _tens: Sprite2D
var _ones: Sprite2D

var _audio_player: AudioStreamPlayer2D
var _counts: Array = []
var _count_index: int = 0

@export var Value: int = 0:
	set(new_value):
		if Value != new_value:
			Value = new_value
			_update_display()


func set_value(v: float) -> void:
	Value = ceili(v)


func _ready() -> void:
	super._ready()

	_audio_player = get_node("%AudioStreamPlayer2D")

	_tens = get_node("%Tens")
	_ones = get_node("%Ones")

	_counts = [
		load("res://countdown-timer/audio/zero.ogg"),
		load("res://countdown-timer/audio/one.ogg"),
		load("res://countdown-timer/audio/two.ogg"),
		load("res://countdown-timer/audio/three.ogg"),
		load("res://countdown-timer/audio/four.ogg"),
		load("res://countdown-timer/audio/five.ogg"),
		load("res://countdown-timer/audio/six.ogg"),
		load("res://countdown-timer/audio/seven.ogg"),
		load("res://countdown-timer/audio/eight.ogg"),
		load("res://countdown-timer/audio/nine.ogg"),
		load("res://countdown-timer/audio/ten.ogg"),
	]


func _update_display() -> void:
	if _tens == null or _ones == null:
		return
	var clamped: int = clampi(Value, 0, 99)
	_tens.frame = floori(clamped / 10.0)
	_ones.frame = clamped % 10

	if not Quiet:
		if Value >= 0 and Value < _counts.size():
			print("Playing %d (%d)" % [_count_index, Value])
			_audio_player.stream = _counts[Value]
			_audio_player.play()
