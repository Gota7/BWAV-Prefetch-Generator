using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWAVPrefetchGen {

    /// <summary>
    /// Program.
    /// </summary>
    class Program {
        
        /// <summary>
        /// Main program.
        /// </summary>
        /// <param name="args">Arguments.</param>
        static void Main(string[] args) {

            //Get args.
            string input = null;
            string output = null;
            if (args.Length < 1) {
                Console.WriteLine("Usage: BWAVPrefetchGen.exe input.bwav (output.bwav)");
                Console.WriteLine("\tc2020 Gota7");
                return;
            }
            input = args[0];
            if (args.Length < 2) {
                output = Path.GetFileNameWithoutExtension(input) + "_Prefetch.bwav";
            } else {
                output = args[1];
            }

            //Remove.
            if (File.Exists(output)) {
                File.Delete(output);
            }

            //Start writing data.
            using (FileStream src = new FileStream(input, FileMode.Open)) {
                using (BinaryReader r = new BinaryReader(src)) {
                    using (FileStream o = new FileStream(output, FileMode.OpenOrCreate)) {
                        using (BinaryWriter w = new BinaryWriter(o)) {

                            //Prefetch flag.
                            w.Write(r.ReadBytes(0xC));
                            w.Write((ushort)1); r.ReadUInt16();
                            ushort numChannels = r.ReadUInt16();
                            w.Write(numChannels);

                            //Contained sample count.
                            uint numSamples = 0;
                            uint[] channelOffs = new uint[numChannels];
                            uint[] retChannelOffs = new uint[numChannels];
                            for (int i = 0; i < numChannels; i++) {
                                w.Write(r.ReadBytes(8));
                                numSamples = r.ReadUInt32();
                                w.Write(numSamples);
                                numSamples = Math.Min(numSamples, 0x3800);
                                w.Write(numSamples); r.ReadUInt32();
                                w.Write(r.ReadBytes(0x20));
                                channelOffs[i] = r.ReadUInt32();
                                w.Write(channelOffs[i]);
                                retChannelOffs[i] = (uint)r.BaseStream.Position;
                                w.Write((uint)0); r.ReadUInt32();
                                w.Write(r.ReadBytes(0x14));
                            }

                            //Align.
                            while (r.BaseStream.Position % 0x20 != 0) {
                                r.ReadByte();
                                w.Write((byte)0);
                            }

                            //Convert samples to bytes.
                            int numBytes = DspAdpcmMath.SampleCountToByteCount((int)numSamples);
                            for (int i = 0; i < numChannels; i++) {
                                long bak = w.BaseStream.Position;
                                w.BaseStream.Position = retChannelOffs[i];
                                w.Write((uint)bak);
                                w.BaseStream.Position = bak;
                                r.BaseStream.Position = channelOffs[i];
                                w.Write(r.ReadBytes(numBytes));
                                while (i != numChannels - 1 && w.BaseStream.Position % 0x20 != 0) {
                                    w.Write((byte)0);
                                }
                            }

                        }
                    }
                }
            }

        }

    }

    /// <summary>
    /// DSP-ADPCM math.
    /// </summary>
    public static class DspAdpcmMath {
        public static readonly int BytesPerFrame = 8;
        public static readonly int SamplesPerFrame = 14;
        public static readonly int NibblesPerFrame = 16;

        public static int NibbleCountToSampleCount(int nibbleCount) {
            int frames = nibbleCount / NibblesPerFrame;
            int extraNibbles = nibbleCount % NibblesPerFrame;
            int extraSamples = extraNibbles < 2 ? 0 : extraNibbles - 2;

            return SamplesPerFrame * frames + extraSamples;
        }

        public static int SampleCountToNibbleCount(int sampleCount) {
            int frames = sampleCount / SamplesPerFrame;
            int extraSamples = sampleCount % SamplesPerFrame;
            int extraNibbles = extraSamples == 0 ? 0 : extraSamples + 2;

            return NibblesPerFrame * frames + extraNibbles;
        }

        public static int NibbleToSample(int nibble) {
            int frames = nibble / NibblesPerFrame;
            int extraNibbles = nibble % NibblesPerFrame;
            int samples = SamplesPerFrame * frames;

            return samples + extraNibbles - 2;
        }

        public static int SampleToNibble(int sample) {
            int frames = sample / SamplesPerFrame;
            int extraSamples = sample % SamplesPerFrame;

            return NibblesPerFrame * frames + extraSamples + 2;
        }

        public static int SampleCountToByteCount(int sampleCount) => SampleCountToNibbleCount(sampleCount).DivideBy2RoundUp();
        public static int ByteCountToSampleCount(int byteCount) => NibbleCountToSampleCount(byteCount * 2);

        public static int DivideBy2RoundUp(this int value) => (value / 2) + (value & 1);

    }

}
