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
	private bool _isActive = false;

	[Export] public float NoiseCutoffHz = 800f; // higher = brighter/hissier roar
	[Export] public float RumbleFreqHz = 45f;   // low rumble base frequency
	[Export] public float Volume = 0.5f;        // 0..1

	public override void _Ready()
	{
		var gen = new AudioStreamGenerator();
		gen.MixRate = 44100f;
		gen.BufferLength = 0.2f; // seconds of buffered audio
		Stream = gen;
		_sampleHz = gen.MixRate;

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

			// Low frequency sine -> gives the deep rumble underneath
			_phase += RumbleFreqHz / _sampleHz;
			if (_phase > 1f) _phase -= 1f;
			float rumble = Mathf.Sin(_phase * Mathf.Tau);

			float sample = (_lpState * 0.7f + rumble * 0.3f) * Volume;
			_playback.PushFrame(new Vector2(sample, sample)); // stereo, same L/R
		}
	}
}
