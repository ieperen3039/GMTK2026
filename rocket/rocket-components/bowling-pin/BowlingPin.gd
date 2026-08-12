class_name BowlingPin
extends RocketComponent

const PITCH_DEVIATION: float = 0.5

var _base_pitch_scale: float = 0.0
var _rng := RandomNumberGenerator.new()


func _ready() -> void:
	super._ready()
	var sfx_player: AudioStreamPlayer2D = get_node_or_null("ClangSfx")
	if sfx_player != null:
		_base_pitch_scale = sfx_player.pitch_scale - PITCH_DEVIATION / 2.0


func play_collision_sound(player: AudioStreamPlayer2D, volume: float) -> void:
	player.volume_linear = volume
	player.pitch_scale = _base_pitch_scale + PITCH_DEVIATION * _rng.randf()
	player.play()
