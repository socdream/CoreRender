using CoreRender.Audio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace CoreRender.Tests
{
    [TestClass]
    public class AudioTests
    {
        [TestMethod]
        public void TestAudioPlayback()
        {
            using(var audioSystem = new AudioSystem())
            {
                
                //var wave = new WaveFile(new FileStream(@"C:\Temp\VictoryChord.wav", FileMode.Open));
                var wave = new WaveFile(new FileStream(@"C:\Temp\01 - Deséame Suerte.wav", FileMode.Open));
                audioSystem.PlaySound(wave);
                
                Thread.Sleep(10000);
            }
        }
    }
}
