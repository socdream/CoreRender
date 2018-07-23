using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreRender.Audio.OpenAL
{
    public class OpenALEngine : AudioEngine, IDisposable
    {
        private readonly AudioContext _context;

        public override AudioResourceFactory ResourceFactory { get; }

        public OpenALEngine()
        {
            _context = new AudioContext();
            _context.MakeCurrent();
            ResourceFactory = new OpenALResourceFactory();
        }

        public override void SetListenerPosition(float[] position)
        {
            AL.Listener(ALListener3f.Position, position[0], position[1], position[2]);
        }

        public override void SetListenerOrientation(float[] forward, float[] up)
        {
            OpenTK.Vector3 f = new OpenTK.Vector3(forward[0], forward[1], forward[2]);
            OpenTK.Vector3 u = new OpenTK.Vector3(up[0], up[1], up[2]);
            AL.Listener(ALListenerfv.Orientation, ref f, ref u);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
