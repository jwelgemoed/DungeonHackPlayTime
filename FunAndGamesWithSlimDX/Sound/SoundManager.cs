using SharpDX;
using SharpDX.DirectSound;
using SharpDX.Multimedia;
using System;

namespace DungeonHack.Sound
{
    public class SoundManager : IDisposable
    {
        private DirectSound _directSound;
        private PrimarySoundBuffer _primarySoundBuffer;
        private SecondarySoundBuffer _secondarySoundBuffer;

        public void Initialize(IntPtr formHandle)
        {
            _directSound = new DirectSound();

            _directSound.SetCooperativeLevel(formHandle, CooperativeLevel.Priority);

            var soundBufferDesc = new SoundBufferDescription()
            {
                Flags = BufferFlags.PrimaryBuffer,
                AlgorithmFor3D = Guid.Empty
            };

            _primarySoundBuffer = new PrimarySoundBuffer(_directSound, soundBufferDesc);

            // Default WaveFormat Stereo 44100 16 bit
            WaveFormat waveFormat = new WaveFormat();

            // Create SecondarySoundBuffer
            var secondaryBufferDesc = new SoundBufferDescription
            {
                BufferBytes = waveFormat.ConvertLatencyToByteSize(60000),
                Format = waveFormat,
                Flags = BufferFlags.GetCurrentPosition2 | BufferFlags.ControlPositionNotify | BufferFlags.GlobalFocus |
                                        BufferFlags.ControlVolume | BufferFlags.StickyFocus,
                AlgorithmFor3D = Guid.Empty
            };
            _secondarySoundBuffer = new SecondarySoundBuffer(_directSound, secondaryBufferDesc);

            // Get Capabilties from secondary sound buffer
            var capabilities = _secondarySoundBuffer.Capabilities;

            // Lock the buffer
            var dataPart1 = _secondarySoundBuffer.Lock(0, 
                capabilities.BufferBytes, 
                LockFlags.EntireBuffer, 
                out DataStream dataPart2);

            // Fill the buffer with some sound
            int numberOfSamples = capabilities.BufferBytes / waveFormat.BlockAlign;
            for (int i = 0; i < numberOfSamples; i++)
            {
                double vibrato = Math.Cos(2 * Math.PI * 10.0 * i / waveFormat.SampleRate);
                short value = (short)(Math.Cos(2 * Math.PI * (220.0 + 4.0 * vibrato) * i / waveFormat.SampleRate) * 16384); // Not too loud
                dataPart1.Write(value);
                dataPart1.Write(value);
            }

            // Unlock the buffer
            _secondarySoundBuffer.Unlock(dataPart1, dataPart2);
        }

        public void LoadAmbientSound()
        {
            
        }

        public void PlayAmbient()
        {
            _primarySoundBuffer.Play(0, PlayFlags.Looping);
            //_secondarySoundBuffer.Play(0, PlayFlags.Looping);

           // SharpDX.DirectSound.DirectSound
        }

        public void Dispose()
        {
            _secondarySoundBuffer?.Dispose();
            _primarySoundBuffer?.Dispose();
        }
    }
}
