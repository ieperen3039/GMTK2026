class_name ThrusterSFX
extends AudioStreamPlayer2D

var _playback: AudioStreamGeneratorPlayback
var _sample_hz: float = 0.0
var _rng := RandomNumberGenerator.new()

# References so I can tune the audio
var _sfx_bus_idx: int = 0
@export var _distortion_effect: AudioEffectDistortion
@export var _reverb_effect: AudioEffectReverb

# Filter state for the "roar" (low-passed white noise)
var _lp_state: float = 0.0
var _phase: float = 0.0

# Paul Kellett pink noise filter state (7 taps)
var _pb0: float = 0.0
var _pb1: float = 0.0
var _pb2: float = 0.0
var _pb3: float = 0.0
var _pb4: float = 0.0
var _pb5: float = 0.0
var _pb6: float = 0.0

@export var NoiseCutoffHz: float = 800.0  # higher = brighter/hissier roar (white noise layer)
@export var RumbleFreqHz: float = 45.0    # low rumble base frequency
@export var Volume: float = 0.5           # 0..1
@export var PinkMix: float = 0.4          # 0..1, how much pink noise blends into the roar
var _is_active: bool = false


func _ready() -> void:
	var gen := AudioStreamGenerator.new()
	gen.mix_rate = 44100.0
	gen.buffer_length = 0.2  # seconds of buffered audio
	stream = gen
	_sample_hz = gen.mix_rate

	pitch_scale = _rng.randf_range(0.90, 1.1)

	# 1. Get the index of your specific audio bus (case-sensitive)
	_sfx_bus_idx = AudioServer.get_bus_index("RocketEngine")

	# 2. Fetch the effects by their slot index
	# Slot 0 = First effect in the inspector list, Slot 1 = Second effect, etc.
	_distortion_effect = AudioServer.get_bus_effect(_sfx_bus_idx, 0)
	_reverb_effect = AudioServer.get_bus_effect(_sfx_bus_idx, 1)

	play()
	_playback = get_stream_playback()
	_rng.randomize()

	_fill_buffer()  # prime the buffer before first _process call


func _process(_delta: float) -> void:
	_fill_buffer()


# Call this when the rocket engine fires.
func start_engine() -> void:
	if _is_active:
		return

	# Reset filter/oscillator state so restarts don't pop or click.
	_lp_state = 0.0
	_phase = 0.0
	_pb0 = 0.0
	_pb1 = 0.0
	_pb2 = 0.0
	_pb3 = 0.0
	_pb4 = 0.0
	_pb5 = 0.0
	_pb6 = 0.0

	play()
	# IMPORTANT: must re-fetch the playback object every time playback (re)starts —
	# the old AudioStreamGeneratorPlayback becomes invalid once stop() is called.
	_playback = get_stream_playback()
	_is_active = true

	_fill_buffer()  # prime the buffer immediately so there's no gap before the next _process


# Call this when the rocket engine shuts off.
func stop_engine() -> void:
	if not _is_active:
		return

	_is_active = false
	stop()
	_playback = null


func _fill_buffer() -> void:
	var frames_available: int = _playback.get_frames_available()
	var lp_alpha: float = clampf(NoiseCutoffHz / (_sample_hz * 0.5), 0.0, 1.0)

	for i in range(frames_available):
		# White noise, low-passed -> gives the "roar/whoosh" texture
		var noise: float = _rng.randf() * 2.0 - 1.0
		_lp_state += lp_alpha * (noise - _lp_state)

		# Pink noise -> fills in the "body" between the rumble and the hiss
		var pink: float = _next_pink()

		# Low frequency sine -> gives the deep rumble underneath
		_phase += RumbleFreqHz / _sample_hz
		if _phase > 1.0:
			_phase -= 1.0
		var rumble: float = sin(_phase * TAU)

		var roar: float = lerpf(_lp_state, pink, PinkMix)
		var sample: float = (roar * 0.7 + rumble * 0.3) * Volume
		_playback.push_frame(Vector2(sample, sample))  # stereo, same L/R


# Paul Kellett's refined pink noise approximation (~ -3dB/octave), cheap and good quality.
func _next_pink() -> float:
	var white: float = _rng.randf() * 2.0 - 1.0

	_pb0 = 0.99886 * _pb0 + white * 0.0555179
	_pb1 = 0.99332 * _pb1 + white * 0.0750759
	_pb2 = 0.96900 * _pb2 + white * 0.1538520
	_pb3 = 0.86650 * _pb3 + white * 0.3104856
	_pb4 = 0.55000 * _pb4 + white * 0.5329522
	_pb5 = -0.7616 * _pb5 - white * 0.0168980

	var pink: float = _pb0 + _pb1 + _pb2 + _pb3 + _pb4 + _pb5 + _pb6 + white * 0.5362
	_pb6 = white * 0.115926

	return pink * 0.11  # scale down, the sum above runs hotter than +-1
