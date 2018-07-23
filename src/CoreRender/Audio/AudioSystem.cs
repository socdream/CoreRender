using CoreRender.Audio.Null;
using CoreRender.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRender.Audio
{
    public class AudioSystem : IDisposable
    {
        private const uint InitialFreeSources = 2;

        private readonly AudioEngine _engine;
        private readonly Dictionary<WaveFile, AudioBuffer> _buffers = new Dictionary<WaveFile, AudioBuffer>();

        private readonly List<AudioSource> _activeSoundSources = new List<AudioSource>();
        private readonly List<AudioSource> _freeSoundSources;

        public AudioEngine Engine => _engine;

        public AudioSystem()
        {
            _engine = CreateDefaultAudioEngine();

            _freeSoundSources = new List<AudioSource>();
            for (uint i = 0; i < InitialFreeSources; i++)
            {
                _freeSoundSources.Add(CreateNewFreeAudioSource());
            }
        }

        private AudioSource GetFreeSource()
        {
            AudioSource source;
            if (_freeSoundSources.Count == 0)
            {
                source = CreateNewFreeAudioSource();
            }
            else
            {
                source = _freeSoundSources[_freeSoundSources.Count - 1];
                _freeSoundSources.RemoveAt(_freeSoundSources.Count - 1);
            }

            return source;
        }

        private AudioSource CreateNewFreeAudioSource()
        {
            AudioSource source = _engine.ResourceFactory.CreateAudioSource();
            source.Position = new float[3];
            source.PositionKind = AudioPositionKind.ListenerRelative;
            return source;
        }

        private AudioEngine CreateDefaultAudioEngine(bool useNull = false)
        {
            if (useNull)
            {
                return new NullAudioEngine();
            }
            //else if (options == AudioEngineOptions.UseOpenAL || !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            else
            {
                try
                {
                    return new OpenALEngine();
                }
                catch (DllNotFoundException) { }
            }
            //else
            //{
            //    return new XAudio2Engine();
            //}

            return new NullAudioEngine();
        }

        protected void UpdateCore(float deltaSeconds)
        {
            for (int i = 0; i < _activeSoundSources.Count; i++)
            {
                AudioSource source = _activeSoundSources[i];
                if (!source.IsPlaying || source.PlaybackPosition >= 1f)
                {
                    _activeSoundSources.Remove(source);
                    _freeSoundSources.Add(source);
                    i--;
                }
            }
        }

        public AudioBuffer GetAudioBuffer(WaveFile wave)
        {
            if (!_buffers.TryGetValue(wave, out AudioBuffer buffer))
            {
                buffer = _engine.ResourceFactory.CreateAudioBuffer();
                buffer.BufferData(wave.Data, wave.Format, wave.SizeInBytes, wave.Frequency);
                _buffers.Add(wave, buffer);
            }

            return buffer;
        }

        private void SetListenerPosition(float[] position, float[] forward, float[] up)
        {
            _engine.SetListenerPosition(position);
            _engine.SetListenerOrientation(forward, up);
        }

        public void PlaySound(WaveFile wave)
        {
            PlaySound(wave, 1.0f, 1.0f);
        }

        public void PlaySound(AudioBuffer buffer)
        {
            PlaySound(buffer, 1.0f, 1.0f, new float[3], AudioPositionKind.ListenerRelative);
        }

        public void PlaySound(WaveFile wave, float volume)
        {
            PlaySound(wave, volume, 1f);
        }

        public void PlaySound(WaveFile wave, float volume, float pitch)
        {
            AudioBuffer buffer = GetAudioBuffer(wave);
            PlaySound(buffer, volume, pitch, new float[3], AudioPositionKind.ListenerRelative);
        }

        public void PlaySound(WaveFile wave, float volume, float pitch, float[] position, AudioPositionKind positionKind)
        {
            AudioBuffer buffer = GetAudioBuffer(wave);
            PlaySound(buffer, volume, pitch, position, positionKind);
        }

        public void PlaySound(AudioBuffer buffer, float volume, float pitch, float[] position, AudioPositionKind positionKind)
        {
            AudioSource source = GetFreeSource();
            source.Gain = volume;
            source.Pitch = pitch;
            source.Play(buffer);
            source.Position = position;
            source.PositionKind = positionKind;
            _activeSoundSources.Add(source);
        }

        public void Dispose()
        {
            foreach (var source in _activeSoundSources)
            {
                source.Dispose();
            }
            foreach (var source in _freeSoundSources)
            {
                source.Dispose();
            }
            foreach (var kvp in _buffers)
            {
                kvp.Value.Dispose();
            }

            if (_engine is IDisposable)
            {
                ((IDisposable)_engine).Dispose();
            }
        }
    }
}
