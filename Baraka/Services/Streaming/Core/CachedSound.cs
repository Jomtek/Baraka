﻿using Baraka.Services.Streaming.Core;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Baraka.Services.Streaming
{
    // https://markheath.net/post/fire-and-forget-audio-playback-with
    public class CachedSound
    {
        public float[] AudioData { get; private set; }
        public WaveFormat WaveFormat { get; private set; }
        public CachedSound(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var audioFileReader = new AudioStreamReader(ms))
            {
                var resampler = new WdlResamplingSampleProvider(audioFileReader, 44100);
                WaveFormat = resampler.WaveFormat;

                var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
                var readBuffer = new float[WaveFormat.SampleRate * WaveFormat.Channels];
                int samplesRead;
                while ((samplesRead = resampler.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    wholeFile.AddRange(readBuffer.Take(samplesRead));
                }
                AudioData = wholeFile.ToArray();
            }
        }
    }
}
