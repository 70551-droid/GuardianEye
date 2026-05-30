using System;
using System.Runtime.InteropServices;

namespace GuardianEye.Client
{
    /// <summary>
    /// Detects whether any audio is currently being output on the system
    /// using the Windows Core Audio API (IAudioMeterInformation).
    /// If audio is playing (e.g., a video, music), returns true.
    /// </summary>
    public static class AudioDetector
    {
        // COM interfaces for Windows Core Audio API
        [ComImport]
        [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        private class MMDeviceEnumerator { }

        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceEnumerator
        {
            int EnumAudioEndpoints(int dataFlow, int dwStateMask, out IntPtr ppDevices);
            int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice ppEndpoint);
        }

        [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDevice
        {
            int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
        }

        [Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioMeterInformation
        {
            int GetPeakValue(out float pfPeak);
        }

        // EDataFlow.eRender = 0 (speakers/output)
        // ERole.eMultimedia = 1
        private const int eRender = 0;
        private const int eMultimedia = 1;
        private const int CLSCTX_ALL = 23;

        private static readonly Guid IID_IAudioMeterInformation = 
            new Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064");

        /// <summary>
        /// Returns true if audio is currently being output on the default playback device.
        /// </summary>
        public static bool IsAudioPlaying()
        {
            try
            {
                var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
                int hr = enumerator.GetDefaultAudioEndpoint(eRender, eMultimedia, out IMMDevice device);
                if (hr != 0 || device == null) return false;

                Guid iid = IID_IAudioMeterInformation;
                hr = device.Activate(ref iid, CLSCTX_ALL, IntPtr.Zero, out object meterObj);
                if (hr != 0 || meterObj == null) return false;

                var meter = (IAudioMeterInformation)meterObj;
                hr = meter.GetPeakValue(out float peak);
                if (hr != 0) return false;

                // If peak value is above a tiny threshold, audio is playing
                return peak > 0.001f;
            }
            catch
            {
                // If anything goes wrong with COM, assume no audio (fail safe: allow idle lock)
                return false;
            }
        }
    }
}
