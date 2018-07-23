using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CoreRender.Audio
{
    public class WaveFile
    {
        public byte[] Data { get; }
        public BufferAudioFormat Format { get; }
        public int SizeInBytes { get; }
        public int Frequency { get; }

        public WaveFile(Stream s)
        {
            using (BinaryReader br = new BinaryReader(s))
            {
                var riffChunkDescriptor = new RIFFChunkDescriptor()
                {
                    ChunkID = new byte[4],
                    Format = new byte[4]
                };

                br.Read(riffChunkDescriptor.ChunkID, 0, 4);
                riffChunkDescriptor.ChunkSize = br.ReadInt32();
                br.Read(riffChunkDescriptor.Format, 0, 4);

                var id = Encoding.ASCII.GetString(riffChunkDescriptor.ChunkID, 0, 4);

                if (id != "RIFF")
                {
                    throw new InvalidOperationException("Not a valid wave file ID: " + id);
                }

                string format = Encoding.ASCII.GetString(riffChunkDescriptor.Format, 0, 4);
                if (format != "WAVE")
                {
                    throw new InvalidOperationException("Not a valid wave file format : " + format);
                }

                var fmtSubChunk = new FmtSubChunk()
                {
                    Subchunk1ID = new byte[4]
                };

                br.Read(fmtSubChunk.Subchunk1ID, 0, 4);
                fmtSubChunk.Subchunk1Size = br.ReadInt32();
                fmtSubChunk.AudioFormat = br.ReadInt16();
                fmtSubChunk.NumChannels = br.ReadInt16();
                fmtSubChunk.SampleRate = br.ReadInt32();
                fmtSubChunk.ByteRate = br.ReadInt32();
                fmtSubChunk.BlockAlign = br.ReadInt16();
                fmtSubChunk.BitsPerSample = br.ReadInt16();

                string fmtChunkID = Encoding.ASCII.GetString(fmtSubChunk.Subchunk1ID, 0, 4);

                if (fmtChunkID != "fmt ")
                {
                    throw new InvalidOperationException("Not a supported fmt sub-chunk ID: " + fmtChunkID);
                }

                Format = MapFormat(fmtSubChunk.NumChannels, fmtSubChunk.BitsPerSample);
                Frequency = fmtSubChunk.SampleRate;

                // SubChunk2ID
                br.ReadInt32();
                int subchunk2Size = br.ReadInt32();
                Data = br.ReadBytes(subchunk2Size);
                SizeInBytes = subchunk2Size;
            }
        }

        private BufferAudioFormat MapFormat(short numChannels, short bitsPerSample)
        {
            if (numChannels == 1 || numChannels == 2)
            {
                if (bitsPerSample == 8)
                {
                    return numChannels == 1 ? BufferAudioFormat.Mono8 : BufferAudioFormat.Stereo8;
                }
                else if (bitsPerSample == 16)
                {
                    return numChannels == 1 ? BufferAudioFormat.Mono16 : BufferAudioFormat.Stereo16;
                }
                else
                {
                    throw new InvalidOperationException("Unsupported bit depth in wave file: " + bitsPerSample);
                }
            }
            else
            {
                throw new InvalidOperationException("Unsupported number of channels in wave file: " + numChannels);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RIFFChunkDescriptor
        {
            public byte[] ChunkID;
            public int ChunkSize;
            public byte[] Format;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FmtSubChunk
        {
            public byte[] Subchunk1ID;
            public int Subchunk1Size;
            public short AudioFormat;
            public short NumChannels;
            public int SampleRate;
            public int ByteRate;
            public short BlockAlign;
            public short BitsPerSample;
        }
    }
}
