using System;
using System.IO;
using NAudio.FileFormats.Wav;
using NAudio.Wave;
using NVorbis;
using UnityEngine;

namespace ModLoader.LoaderUtil
{
    public static class ResourceDataLoader
    {
        public static AudioClip GetAudioClipFromWav(Stream raw_data, string clip_name)
        {
            var waveFileReader = new WaveFileReader(raw_data);
            var waveFormat = waveFileReader.WaveFormat;
            var clip = AudioClip.Create(clip_name, (int) waveFileReader.SampleCount,
                waveFormat.Channels, waveFormat.SampleRate, false);
            clip.name = clip_name;
            clip.SetData(waveFileReader.ReadAllSamples(), 0);
            return clip;
        }

        public static AudioClip GetAudioClipFromOgg(Stream raw_data, string clip_name)
        {
            var vorbisReader = new VorbisReader(raw_data, false);
            var clip = AudioClip.Create(clip_name, (int) vorbisReader.TotalSamples,
                vorbisReader.Channels, vorbisReader.SampleRate, false);
            clip.name = clip_name;
            clip.SetData(vorbisReader.ReadAllSamples(), 0);
            return clip;
        }

        public static AudioClip GetAudioClipFromMp3(Stream raw_data, string clip_name)
        {
            var mp3FileReader = new Mp3FileReader(raw_data);
            using var memoryStream = new MemoryStream();
            WaveFileWriter.WriteWavFileToStream(memoryStream, mp3FileReader);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return GetAudioClipFromWav(memoryStream, clip_name);
        }

        public static float[] ReadAllSamples(this WaveFileReader waveFileReader)
        {
            var buf = new float[waveFileReader.SampleCount * waveFileReader.WaveFormat.Channels];
            var index = 0;
            while (waveFileReader.SafeReadNextSampleFrame(out var _data) && _data != null)
            {
                foreach (var simple in _data)
                {
                    buf[index] = simple;
                    index += 1;
                }
            }

            return buf;
        }

        public static float[] ReadAllSamples(this VorbisReader vorbisReader)
        {
            var buf = new float[vorbisReader.TotalSamples * vorbisReader.Channels];
            vorbisReader.ReadSamples(buf.AsSpan());
            return buf;
        }

        private static bool SafeReadNextSampleFrame(this WaveFileReader waveFileReader, out float[] data)
        {
            try
            {
                data = waveFileReader.ReadNextSampleFrame();
                return true;
            }
            catch (Exception)
            {
                data = Array.Empty<float>();
                return false;
            }
        }
    }
}