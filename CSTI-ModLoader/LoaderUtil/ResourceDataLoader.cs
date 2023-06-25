using System;
using System.Text;
using UnityEngine;

namespace ModLoader.LoaderUtil
{
    public static class ResourceDataLoader
    {
        public static AudioClip GetAudioClipFromWav(byte[] raw_data, string clip_name)
        {
            AudioClip clip = null;
            //var raw_data = System.IO.File.ReadAllBytes(file);
            var raw_string = Encoding.ASCII.GetString(raw_data);
            //var clip_name = Path.GetFileNameWithoutExtension(file);

            if (raw_string.Substring(0, 4) == "RIFF")
            {
                if (raw_string.Substring(8, 4) == "WAVE")
                {
                    int index = 4; //ChunkId
                    var ChunkSize = BitConverter.ToUInt32(raw_data, 4);
                    index += 4;
                    index += 4; // WAVE

                    UInt16 NumChannels = 1;
                    UInt32 SampleRate = 0;
                    UInt16 BitsPerSample = 0;
                    UInt16 BolckAlign = 1;

                    while (index < raw_data.Length)
                    {
                        var SubchunkID = raw_string.Substring(index, 4);
                        var SubchunkSize = BitConverter.ToUInt32(raw_data, index + 4);

                        index += 8;
                        if (SubchunkID == "fmt ")
                        {
                            var AudioFormat = BitConverter.ToUInt16(raw_data, index);
                            NumChannels = BitConverter.ToUInt16(raw_data, index + 2);
                            SampleRate = BitConverter.ToUInt32(raw_data, index + 4);
                            var ByteRate = BitConverter.ToUInt32(raw_data, index + 8);
                            BolckAlign = BitConverter.ToUInt16(raw_data, index + 12);
                            BitsPerSample = BitConverter.ToUInt16(raw_data, index + 14);
                            index += (int) SubchunkSize;
                        }
                        else if (SubchunkID == "data")
                        {
                            var data_len = (raw_data.Length - index);
                            var data = new float[data_len / BolckAlign * NumChannels];

                            //Debug.LogFormat("{0} {1} {2} {3} {4} {5}", NumChannels, BolckAlign, BitsPerSample, data_len, data.Length, SubchunkSize);

                            for (int i = 0; i < data.Length; i += NumChannels)
                            {
                                for (int j = 0; j < NumChannels; j++)
                                {
                                    if (BitsPerSample == 8)
                                        data[i + j] =
                                            BitConverter.ToChar(raw_data, index + BolckAlign * (i / NumChannels) + j) /
                                            ((float) Char.MaxValue);
                                    else if (BitsPerSample == 16)
                                        data[i + j] =
                                            (BitConverter.ToInt16(raw_data,
                                                index + BolckAlign * (i / NumChannels) + 2 * j)) /
                                            ((float) Int16.MaxValue);
                                    else if (BitsPerSample == 32)
                                        data[i + j] =
                                            BitConverter.ToInt32(raw_data,
                                                index + BolckAlign * (i / NumChannels) + 4 * j) /
                                            ((float) Int32.MaxValue);
                                }
                            }

                            clip = AudioClip.Create(clip_name, data.Length / NumChannels, NumChannels, (int) SampleRate,
                                false);
                            clip.SetData(data, 0);

                            index += (int) SubchunkSize;
                            break;
                        }
                        else
                        {
                            index += (int) SubchunkSize;
                        }
                    }
                }
            }

            return clip;
        }
    }
}