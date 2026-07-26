using System;
using Godot;

// Attach this script to an AudioStreamPlayer (or AudioStreamPlayer2D/3D) node.
public partial class ThrusterSFX : AudioStreamPlayer2D
{
	private AudioStreamGeneratorPlayback _playback;
	private float _sampleHz;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();

	// References so I can tune the audio
	private int _sfxBusIdx;
	[Export] private AudioEffectDistortion _distortionEffect;
	[Export] private AudioEffectReverb _reverbEffect;

	// Filter state for the "roar" (low-passed white noise)
	private float _lpState = 0f;
	private float _phase = 0f;

	// Paul Kellett pink noise filter state (7 taps)
	private float _pb0, _pb1, _pb2, _pb3, _pb4, _pb5, _pb6;
 
	[Export] public float NoiseCutoffHz = 800f; // higher = brighter/hissier roar (white noise layer)
	[Export] public float RumbleFreqHz = 45f;   // low rumble base frequency
	[Export] public float Volume = 0.5f;        // 0..1
	[Export] public float PinkMix = 0.4f;       // 0..1, how much pink noise blends into the roar
	private bool _isActive = false;
 

	public override void _Ready()
	{
		var gen = new AudioStreamGenerator();
		gen.MixRate = 44100f;
		gen.BufferLength = 0.2f; // seconds of buffered audio
		Stream = gen;
		_sampleHz = gen.MixRate;

		PitchScale = _rng.RandfRange(0.90f, 1.1f);

		// 1. Get the index of your specific audio bus (case-sensitive)
		_sfxBusIdx = AudioServer.GetBusIndex("RocketEngine");

		// 2. Fetch the effects by their slot index and cast them
		// Slot 0 = First effect in the inspector list, Slot 1 = Second effect, etc.
		_distortionEffect = AudioServer.GetBusEffect(_sfxBusIdx, 0) as AudioEffectDistortion;
		_reverbEffect = AudioServer.GetBusEffect(_sfxBusIdx, 1) as AudioEffectReverb;


		Play();
		_playback = (AudioStreamGeneratorPlayback)GetStreamPlayback();
		_rng.Randomize();

		FillBuffer(); // prime the buffer before first _Process call
	}

	public override void _Process(double delta)
	{
		FillBuffer();
	}

	// Call this when the rocket engine fires.
	public void StartEngine()
	{
		if (_isActive) return;
 
		// Reset filter/oscillator state so restarts don't pop or click.
		_lpState = 0f;
		_phase = 0f;
		_pb0 = _pb1 = _pb2 = _pb3 = _pb4 = _pb5 = _pb6 = 0f;
 
		Play();
		// IMPORTANT: must re-fetch the playback object every time playback (re)starts —
		// the old AudioStreamGeneratorPlayback becomes invalid once Stop() is called.
		_playback = (AudioStreamGeneratorPlayback)GetStreamPlayback();
		_isActive = true;
 
		FillBuffer(); // prime the buffer immediately so there's no gap before the next _Process
	}
 
	// Call this when the rocket engine shuts off.
	public void StopEngine()
	{
		if (!_isActive) return;
 
		_isActive = false;
		Stop();
		_playback = null;
	}

	private void FillBuffer()
	{
		int framesAvailable = _playback.GetFramesAvailable();
		float lpAlpha = Mathf.Clamp(NoiseCutoffHz / (_sampleHz * 0.5f), 0f, 1f);
 
		for (int i = 0; i < framesAvailable; i++)
		{
			// White noise, low-passed -> gives the "roar/whoosh" texture
			float noise = (float)(_rng.Randf() * 2.0 - 1.0);
			_lpState += lpAlpha * (noise - _lpState);
 
			// Pink noise -> fills in the "body" between the rumble and the hiss
			float pink = NextPink();
 
			// Low frequency sine -> gives the deep rumble underneath
			_phase += RumbleFreqHz / _sampleHz;
			if (_phase > 1f) _phase -= 1f;
			float rumble = Mathf.Sin(_phase * Mathf.Tau);
 
			float roar = Mathf.Lerp(_lpState, pink, PinkMix);
			float sample = (roar * 0.7f + rumble * 0.3f) * Volume;
			_playback.PushFrame(new Vector2(sample, sample)); // stereo, same L/R
		}
	}
 
	// Paul Kellett's refined pink noise approximation (~ -3dB/octave), cheap and good quality.
	private float NextPink()
	{
		float white = (float)(_rng.Randf() * 2.0 - 1.0);
 
		_pb0 = 0.99886f * _pb0 + white * 0.0555179f;
		_pb1 = 0.99332f * _pb1 + white * 0.0750759f;
		_pb2 = 0.96900f * _pb2 + white * 0.1538520f;
		_pb3 = 0.86650f * _pb3 + white * 0.3104856f;
		_pb4 = 0.55000f * _pb4 + white * 0.5329522f;
		_pb5 = -0.7616f * _pb5 - white * 0.0168980f;
 
		float pink = _pb0 + _pb1 + _pb2 + _pb3 + _pb4 + _pb5 + _pb6 + white * 0.5362f;
		_pb6 = white * 0.115926f;
 
		return pink * 0.11f; // scale down, the sum above runs hotter than +-1
	}
}
