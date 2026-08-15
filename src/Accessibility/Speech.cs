using System;
using System.Runtime.InteropServices;
using System.Threading;
using CandyCrushAccessible.Audio;

namespace CandyCrushAccessible.Accessibility
{
    public static class Speech
    {
        private const int NVDA_CANCEL_SETTLE_MS = 40;
        private const int NON_INTERRUPT_DROP_MS = 1000;

        private static bool _nvdaAvailable;
        private static bool _checked;
        private static readonly object _lock = new object();
        private static DateTime _lastNvdaSpeak = DateTime.MinValue;
        private static object _sapiVoice;
        private static bool _sapiReady;

        [DllImport("nvdaControllerClient64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int nvdaController_testIfRunning();

        [DllImport("nvdaControllerClient64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int nvdaController_speakText([MarshalAs(UnmanagedType.LPWStr)] string text);

        [DllImport("nvdaControllerClient64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int nvdaController_cancelSpeech();

        public static void Initialize()
        {
            try
            {
                _nvdaAvailable = nvdaController_testIfRunning() == 0;
            }
            catch
            {
                _nvdaAvailable = false;
            }
            _checked = true;
            if (!_nvdaAvailable)
            {
                InitSapi();
            }
        }

        private static void InitSapi()
        {
            try
            {
                Type t = Type.GetTypeFromProgID("SAPI.SpVoice");
                if (t == null) return;
                _sapiVoice = Activator.CreateInstance(t);
                _sapiReady = true;
            }
            catch
            {
                _sapiVoice = null;
                _sapiReady = false;
            }
        }

        public static bool NvdaRunning
        {
            get
            {
                if (!_checked) Initialize();
                return _nvdaAvailable;
            }
        }

        public static void Speak(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            lock (_lock)
            {
                if (_nvdaAvailable)
                {
                    DateTime now = DateTime.UtcNow;
                    double elapsed = (now - _lastNvdaSpeak).TotalMilliseconds;
                    if (elapsed < NVDA_CANCEL_SETTLE_MS)
                    {
                        nvdaController_cancelSpeech();
                        Thread.Sleep(15);
                    }
                    nvdaController_speakText(text);
                    _lastNvdaSpeak = now;
                    SoundEngine.DuckMusic(0.3f, 1200);
                }
                else if (_sapiReady)
                {
                    SpeakSapi(text);
                }
            }
        }

        public static void SpeakInterrupt(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            lock (_lock)
            {
                if (_nvdaAvailable)
                {
                    nvdaController_cancelSpeech();
                    Thread.Sleep(15);
                    nvdaController_speakText(text);
                    _lastNvdaSpeak = DateTime.UtcNow;
                    SoundEngine.DuckMusic(0.3f, 1200);
                }
                else if (_sapiReady)
                {
                    SpeakSapi(text);
                }
            }
        }

        private static void SpeakSapi(string text)
        {
            try
            {
                dynamic voice = _sapiVoice;
                voice.Speak("", 3);
                voice.Speak(text, 1);
            }
            catch
            {
            }
        }

        public static void Cancel()
        {
            lock (_lock)
            {
                if (_nvdaAvailable)
                {
                    try { nvdaController_cancelSpeech(); } catch { }
                }
                else if (_sapiReady)
                {
                    try
                    {
                        dynamic voice = _sapiVoice;
                        voice.Speak("", 3);
                    }
                    catch { }
                }
            }
        }

        public static void StopIfRecentlySpoken(int nonInterruptMs)
        {
            if (_nvdaAvailable)
            {
                double elapsed = (DateTime.UtcNow - _lastNvdaSpeak).TotalMilliseconds;
                if (elapsed < nonInterruptMs)
                {
                    Cancel();
                }
            }
        }
    }
}