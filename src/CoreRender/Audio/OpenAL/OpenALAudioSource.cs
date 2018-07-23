using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRender.Audio.OpenAL
{
    public class OpenALAudioSource : AudioSource
    {
        public OpenALAudioSource()
        {
            ID = AL.GenSource();
            if (ID == 0)
            {
                throw new InvalidOperationException("Too many OpenALAudioSources.");
            }
        }

        public int ID { get; }

        public override float Gain
        {
            get
            {
                AL.GetSource(ID, ALSourcef.Gain, out float gain);
                return gain;
            }
            set
            {
                AL.Source(ID, ALSourcef.Gain, value);
            }
        }

        public override float Pitch
        {
            get
            {
                AL.GetSource(ID, ALSourcef.Pitch, out float pitch);
                return pitch;
            }
            set
            {
                if (value < 0.5 || value > 2.0f)
                {
                    throw new ArgumentOutOfRangeException("Pitch must be between 0.5 and 2.0.");
                }

                AL.Source(ID, ALSourcef.Pitch, value);
            }
        }

        public override bool Looping
        {
            get
            {
                AL.GetSource(ID, ALSourceb.Looping, out bool looping);
                return looping;
            }
            set
            {
                AL.Source(ID, ALSourceb.Looping, value);
            }
        }

        public override float[] Position
        {
            get
            {
                AL.GetSource(ID, ALSource3f.Position, out OpenTK.Vector3 openTKVec);
                return new float[] { openTKVec.X, openTKVec.Y, openTKVec.Z };
            }
            set
            {
                var openTKVec = new OpenTK.Vector3(value[0], value[1], value[2]);
                AL.Source(ID, ALSource3f.Position, ref openTKVec);
            }
        }

        public override float[] Direction
        {
            get
            {
                AL.GetSource(ID, ALSource3f.Direction, out OpenTK.Vector3 openTKVec);
                return new float[] { openTKVec.X, openTKVec.Y, openTKVec.Z };
            }
            set
            {
                var openTKVec = new OpenTK.Vector3(value[0], value[1], value[2]);
                AL.Source(ID, ALSource3f.Direction, ref openTKVec);
            }
        }

        public override AudioPositionKind PositionKind
        {
            get
            {
                AL.GetSource(ID, ALSourceb.SourceRelative, out bool sourceRelative);
                return sourceRelative ? AudioPositionKind.ListenerRelative : AudioPositionKind.AbsoluteWorld;
            }
            set
            {
                AL.Source(ID, ALSourceb.SourceRelative, value == AudioPositionKind.ListenerRelative ? true : false);
            }
        }

        /// <summary>
        /// Gets or sets the playback position, as a value between 0.0f (beginning of clip), and 1.0f (end of clip).
        /// </summary>
        public override float PlaybackPosition
        {
            get
            {
                AL.GetSource(ID, ALGetSourcei.ByteOffset, out int playbackBytes);
                AL.GetSource(ID, ALGetSourcei.Buffer, out int bufferID);
                AL.GetBuffer(bufferID, ALGetBufferi.Size, out int totalBufferBytes);
                return (float)playbackBytes / totalBufferBytes;
            }
            set
            {
                AL.GetSource(ID, ALGetSourcei.Buffer, out int bufferID);
                AL.GetBuffer(bufferID, ALGetBufferi.Size, out int totalBufferBytes);
                int newByteOffset = (int)(totalBufferBytes * value);
                AL.Source(ID, ALSourcei.ByteOffset, newByteOffset);
            }
        }

        public override bool IsPlaying
        {
            get
            {
                return AL.GetSourceState(ID) == ALSourceState.Playing;
            }
        }

        public override void Play(AudioBuffer buffer)
        {
            OpenALAudioBuffer alBuffer = (OpenALAudioBuffer)buffer;
            AL.BindBufferToSource(ID, alBuffer.ID);
            AL.SourcePlay(ID);
        }

        public override void Stop()
        {
            AL.SourceStop(ID);
        }

        public override void Dispose()
        {
            AL.DeleteSource(ID);
        }
    }
}
