using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using CandyCrushAccessible.Engine;

namespace CandyCrushAccessible.Audio
{
    public static class SoundEngine
    {
        private const uint BASS_SAMPLE_LOOP = 4;
        private const uint BASS_UNICODE = 0x80000000;
        private const int BASS_ATTRIB_VOL = 2;
        private const int BASS_ATTRIB_FREQ = 1;
        private const int BASS_ATTRIB_PAN = 3;
        private const int BASS_SYNC_END = 2;
        private const int BASS_FX_DX8_PARAMEQ = 8;

        [StructLayout(LayoutKind.Sequential)]
        private struct BASS_DX8_PARAMEQ
        {
            public float fCenter;
            public float fBandwidth;
            public float fGain;
        }

        private const int BaseFreq = 44100;

        // ---- Spatial Audio Object Model (Dolby-style) ----
        // Cada sonido se trata como un OBJETO situado en un espacio 3D
        // (columna -> eje X lateral, fila -> eje Y de profundidad, Z = 0 en el plano del tablero).
        // Un "oyente" fijo se sitúa frente al centro del tablero y el motor calcula
        // paneo por acimut, atenuación por distancia y altura tonal por profundidad.
        private const float BoardHalfWidth = 3.5f;      // centro tablero: (3.5, 3.5)
        private const float DepthCompression = 0.5f;     // compresión del eje de profundidad
        private const float ListenerDistance = 7.0f;     // distancia del oyente al centro del tablero
        private const float DistanceRolloff = 0.22f;     // fuerza de atenuación por distancia
        private const float PanStrength = 0.95f;         // intensidad del paneo (0.5 = sutil, 1 = extremo)
        private const float MinVolume = 0.08f;           // volumen mínimo para objetos lejanos
        private const float DepthPitchFactor = 0.003f;   // brillo tonal por proximidad
        private const float MinDistanceForAttenuation = 4.5f;

        private static bool _initialized;
        private static int _musicHandle;
        private static float _musicVolume = 0.45f;
        private static float _sfxVolume = 0.8f;
        private static float _voiceVolume = 0.9f;
        private static readonly List<int> ActiveStreams = new List<int>();
        private static readonly List<SyncProc> SyncDelegates = new List<SyncProc>();
        private static readonly Random Rng = new Random();
        private static Timer _duckTimer;

        private delegate void SyncProc(int handle, int channel, int data, IntPtr user);

        [DllImport("bass.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int BASS_Init(int device, uint freq, uint flags, IntPtr win, IntPtr dsguid);

        [DllImport("bass.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool BASS_Free();

        [DllImport("bass.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int BASS_ErrorGetCode();

        [DllImport("bass.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int BASS_StreamCreateFile(bool mem, [MarshalAs(UnmanagedType.LPWStr)] string file, long offset, long length, uint flags);

        [DllImport("bass.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool BASS_ChannelPlay(int handle, bool restart);

        [DllImport("bass.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool BASS_ChannelStop(int handle);

        [DllImport("bass.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool BASS_StreamFree(int handle);

        [DllImport("bass.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool BASS_ChannelSetAttribute(int handle, int attrib, float value);

        [DllImport("bass.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool BASS_ChannelGetAttribute(int handle, int attrib, out float value);

        [DllImport("bass.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int BASS_ChannelSetSync(int handle, int type, long param, SyncProc proc, IntPtr user);

        [DllImport("bass.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int BASS_ChannelSetFX(int handle, int type, int priority);

        [DllImport("bass.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool BASS_FXSetParameters(int handle, ref BASS_DX8_PARAMEQ par);

        [DllImport("bass.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool BASS_ChannelSlideAttribute(int handle, int attrib, float value, uint time);

        public static void Init()
        {
            if (_initialized) return;
            _initialized = BASS_Init(-1, 44100, 0, IntPtr.Zero, IntPtr.Zero) != 0;
            if (!_initialized && BASS_ErrorGetCode() == 20)
            {
                _initialized = true;
            }
        }

        public static void Shutdown()
        {
            if (!_initialized) return;
            StopAll();
            BASS_Free();
            _initialized = false;
        }

        public static bool IsInitialized { get { return _initialized; } }

        public static float MusicVolume
        {
            get { return _musicVolume; }
            set
            {
                _musicVolume = value;
                if (_musicHandle != 0)
                {
                    BASS_ChannelSetAttribute(_musicHandle, BASS_ATTRIB_VOL, _musicVolume);
                }
            }
        }

        public static float SfxVolume
        {
            get { return _sfxVolume; }
            set { _sfxVolume = value; }
        }

        public static float VoiceVolume
        {
            get { return _voiceVolume; }
            set { _voiceVolume = value; }
        }

        public static void PlayMusic(MusicTrack track, bool loop = true)
        {
            if (!_initialized) return;
            StopMusic();
            string file = MusicMap.FileName(track);
            if (file == null) return;
            string path = ContentResolver.MusicPath(file);
            if (path == null) return;
            uint flags = BASS_UNICODE;
            if (loop) flags |= BASS_SAMPLE_LOOP;
            _musicHandle = BASS_StreamCreateFile(false, path, 0, 0, flags);
            if (_musicHandle == 0) return;
            BASS_ChannelSetAttribute(_musicHandle, BASS_ATTRIB_VOL, _musicVolume);
            BASS_ChannelPlay(_musicHandle, false);
        }

        public static void PlayLevelMusic(LevelType levelType)
        {
            if (!_initialized) return;
            StopMusic();
            string file = MusicMap.GetTrackForLevelType(levelType);
            if (file == null) return;
            string path = ContentResolver.MusicPath(file);
            if (path == null) return;
            _musicHandle = BASS_StreamCreateFile(false, path, 0, 0, BASS_SAMPLE_LOOP | BASS_UNICODE);
            if (_musicHandle == 0) return;
            BASS_ChannelSetAttribute(_musicHandle, BASS_ATTRIB_VOL, _musicVolume);
            BASS_ChannelPlay(_musicHandle, false);
        }

        public static void StopMusic()
        {
            if (_musicHandle != 0)
            {
                BASS_ChannelStop(_musicHandle);
                BASS_StreamFree(_musicHandle);
                _musicHandle = 0;
            }
        }

        /// <summary>
        /// Audrik Sound Design v2: Ducking nativo y suave sin artefactos de fase.
        /// Desliza el volumen de la música con BASS_ChannelSlideAttribute en 200ms y lo restaura suavemente en 1000ms.
        /// </summary>
        public static void DuckMusic(float duckLevel = 0.35f, int durationMs = 1500)
        {
            if (_musicHandle == 0) return;
            float targetDuckVol = _musicVolume * duckLevel;
            BASS_ChannelSlideAttribute(_musicHandle, BASS_ATTRIB_VOL, targetDuckVol, 200);

            if (_duckTimer != null)
            {
                _duckTimer.Change(durationMs, Timeout.Infinite);
            }
            else
            {
                _duckTimer = new Timer(delegate
                {
                    if (_musicHandle != 0)
                    {
                        BASS_ChannelSlideAttribute(_musicHandle, BASS_ATTRIB_VOL, _musicVolume, 1000);
                    }
                }, null, durationMs, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Audrik Sound Design v2: Realce ASMR cristalino en agudos (+4dB a 8000Hz) con gain staging.
        /// Se aplica headroom previo (-3dB / 0.70x) antes del EQ paramétrico para evitar clipping digital.
        /// </summary>
        public static void ApplySweetenerEQ(int channel, ref float volume)
        {
            try
            {
                volume *= 0.70f;
                int fx = BASS_ChannelSetFX(channel, BASS_FX_DX8_PARAMEQ, 0);
                if (fx != 0)
                {
                    BASS_DX8_PARAMEQ eq = new BASS_DX8_PARAMEQ
                    {
                        fCenter = 8000f,
                        fBandwidth = 18f,
                        fGain = 4.0f
                    };
                    BASS_FXSetParameters(fx, ref eq);
                }
            }
            catch { }
        }

        /// <summary>
        /// Audrik Sound Design v2: Realce Low-Punch envolvente (+5dB a 80Hz) con gain staging.
        /// Se aplica headroom previo (-4dB / 0.65x) para otorgar cuerpo y peso sin saturar el techo digital.
        /// </summary>
        public static void ApplyLowPunchEQ(int channel, ref float volume)
        {
            try
            {
                volume *= 0.65f;
                int fx = BASS_ChannelSetFX(channel, BASS_FX_DX8_PARAMEQ, 0);
                if (fx != 0)
                {
                    BASS_DX8_PARAMEQ eq = new BASS_DX8_PARAMEQ
                    {
                        fCenter = 80f,
                        fBandwidth = 24f,
                        fGain = 5.0f
                    };
                    BASS_FXSetParameters(fx, ref eq);
                }
            }
            catch { }
        }

        public static void PlaySound(string key, int col = -1, int row = -1, double pitch = 1.0, double volumeScale = 1.0)
        {
            if (!_initialized) return;
            string file = AudioMap.FileNameWithFallback(key);
            string path = ContentResolver.SoundPath(file);
            if (path == null) return;

            int h = BASS_StreamCreateFile(false, path, 0, 0, BASS_UNICODE);
            if (h == 0) return;

            double finalPitch = pitch;
            float vol = _sfxVolume * (float)volumeScale;

            // Audrik Sound Design v2: Gain Staging previo + Ecualización paramétrica segura
            if (key == "candy" || key == "candy2" || key == "candy3" || key == "candy4" ||
                key == "square" || key == "square2" || key == "jelly" || key == "jelly2" ||
                key == "frosting1" || key == "frosting2" || key == "striped_created" || key == "nut")
            {
                ApplySweetenerEQ(h, ref vol);
            }
            else if (key == "bomb" || key == "wrapped_explosion" || key == "colorbomb" ||
                     key == "supercolorbomb" || key == "sugar" || key == "wrapped_created")
            {
                ApplyLowPunchEQ(h, ref vol);
            }

            if (col >= 0 && row >= 0)
            {
                ApplyObjectSpatialization(h, col, row, ref finalPitch, ref vol);
            }
            else if (col >= 0)
            {
                float pan = (col - BoardHalfWidth) / BoardHalfWidth;
                pan = Math.Max(-1f, Math.Min(1f, pan));
                BASS_ChannelSetAttribute(h, BASS_ATTRIB_PAN, pan);
            }

            if (finalPitch < 0.5) finalPitch = 0.5;
            if (finalPitch > 2.0) finalPitch = 2.0;
            if (finalPitch != 1.0)
            {
                BASS_ChannelSetAttribute(h, BASS_ATTRIB_FREQ, (int)(BaseFreq * finalPitch));
            }
            if (vol > 1.0f) vol = 1.0f;
            if (vol < 0.0f) vol = 0.0f;
            BASS_ChannelSetAttribute(h, BASS_ATTRIB_VOL, vol);
            BASS_ChannelPlay(h, false);

            lock (ActiveStreams)
            {
                ActiveStreams.Add(h);
            }
            SyncProc proc = null;
            proc = delegate(int handle, int channel, int data, IntPtr user)
            {
                BASS_StreamFree(handle);
                lock (ActiveStreams)
                {
                    ActiveStreams.Remove(handle);
                }
                lock (SyncDelegates)
                {
                    SyncDelegates.Remove(proc);
                }
            };
            lock (SyncDelegates)
            {
                SyncDelegates.Add(proc);
            }
            BASS_ChannelSetSync(h, BASS_SYNC_END, 0, proc, IntPtr.Zero);
        }

        /// <summary>
        /// Aplica el modelo de audio por objetos (principio Dolby) con la firma Audrik v2:
        /// calcula paneo por acimut equal-power, atenuación por distancia y una variación
        /// sutil de tono por gravedad en el eje Y (máx ±2.97%) para conservar transitorios limpios.
        /// </summary>
        private static void ApplyObjectSpatialization(int h, int col, int row, ref double pitch, ref float vol)
        {
            // Posición del objeto en el espacio del mundo (centrado en el tablero)
            float wx = (col - BoardHalfWidth) * 1.0f;          // lateral: -3.5 (izq) .. +3.5 (der)
            float wy = (BoardHalfWidth - row) * DepthCompression; // profundidad: +1.75 (fila 0, lejos) .. -1.75 (fila 7, cerca)
            float wz = 0f;

            // Oyente fijo frente al centro del tablero
            float lx = 0f;
            float ly = -ListenerDistance;
            float lz = 0f;

            float dx = wx - lx;
            float dy = wy - ly;
            float dz = wz - lz;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
            if (dist < 0.01f) dist = 0.01f;

            // 1) Paneo por acimut (equal-power, más natural que lineal)
            float azimuth = (float)Math.Atan2(dx, dy);
            float pan = (float)Math.Sin(azimuth) * PanStrength;
            pan = Math.Max(-1f, Math.Min(1f, pan));
            BASS_ChannelSetAttribute(h, BASS_ATTRIB_PAN, pan);

            // 2) Atenuación por distancia (roll-off tipo Dolby: 1/(1 + k*d))
            float att = 1f;
            if (dist > MinDistanceForAttenuation)
            {
                att = 1f / (1f + DistanceRolloff * (dist - MinDistanceForAttenuation));
                att = Math.Max(MinVolume, Math.Min(1f, att));
            }
            vol *= att;

            // 3) Audrik Sound Design v2: Tono por profundidad y gravedad sutil y natural (±2.97% máx)
            double depthPitch = 1.0 + (ListenerDistance - dist) * DepthPitchFactor;
            double gravityPitch = 1.0 + (3.5 - row) * 0.0085; // +2.97% arriba a -2.97% abajo
            pitch *= (depthPitch * gravityPitch);
        }

        public static void PlayVoice(string key)
        {
            if (!_initialized) return;
            string file = AudioMap.FileNameWithFallback(key);
            string path = ContentResolver.SoundPath(file);
            if (path == null) return;

            // Audrik Sound Design v2: Ducking suave de la música por slide (fade-out 200ms, retorno 1000ms a los 1.5s)
            DuckMusic(0.35f, 1500);

            int h = BASS_StreamCreateFile(false, path, 0, 0, BASS_UNICODE);
            if (h == 0) return;
            // Voz 100% limpia sin ecualizadores ni alteraciones de fase
            BASS_ChannelSetAttribute(h, BASS_ATTRIB_VOL, _voiceVolume);
            BASS_ChannelPlay(h, false);

            lock (ActiveStreams)
            {
                ActiveStreams.Add(h);
            }
            SyncProc proc = null;
            proc = delegate(int handle, int channel, int data, IntPtr user)
            {
                BASS_StreamFree(handle);
                lock (ActiveStreams)
                {
                    ActiveStreams.Remove(handle);
                }
                lock (SyncDelegates)
                {
                    SyncDelegates.Remove(proc);
                }
            };
            lock (SyncDelegates)
            {
                SyncDelegates.Add(proc);
            }
            BASS_ChannelSetSync(h, BASS_SYNC_END, 0, proc, IntPtr.Zero);
        }

        public static void PlayCandySound(int x, int y, CandyColor color)
        {
            double pitch = PitchForColor(color);
            PlaySound("candy", x, y, pitch, 1.0);
        }

        public static double PitchForColor(CandyColor color)
        {
            switch (color)
            {
                case CandyColor.Red: return 0.85;
                case CandyColor.Blue: return 0.95;
                case CandyColor.Green: return 1.05;
                case CandyColor.Yellow: return 1.15;
                case CandyColor.Orange: return 1.25;
                case CandyColor.Purple: return 1.35;
            }
            return 1.0;
        }

        public static void PlayMatchSound(int comboLevel, int col = -1)
        {
            string key = "match" + Math.Max(1, Math.Min(comboLevel, 12));
            PlaySound(key, col, -1, 1.0, 1.5);
        }

        public static void PlayMatchSequence(int comboLevel, int col = -1)
        {
            int n = Math.Max(1, Math.Min(comboLevel, 12));
            System.Threading.Timer t = null;
            int i = 1;
            t = new System.Threading.Timer(delegate
            {
                if (i <= n)
                {
                    PlayMatchSound(i, col);
                    i++;
                    if (i > n) t.Dispose();
                }
            }, null, 0, 220);
        }

        public static void PlayStarSequence(int stars)
        {
            int n = Math.Max(1, Math.Min(stars, 3));
            System.Threading.Timer t = null;
            int i = 1;
            t = new System.Threading.Timer(delegate
            {
                if (i <= n)
                {
                    PlaySound("star" + i, -1, -1, 1.0, 1.6);
                    i++;
                    if (i > n) t.Dispose();
                }
            }, null, 900, 700);
        }

        public static void PlaySugarCrushSequence(TurnResult result)
        {
            PlaySound("sugar");
            if (result == null || result.ActivationsDetailed.Count == 0)
            {
                int n = Math.Max(1, Math.Min(result != null ? result.SugarCrushMoves : 5, 12));
                System.Threading.Timer t = null;
                int i = 1;
                t = new System.Threading.Timer(delegate
                {
                    if (i <= n)
                    {
                        PlayMatchSound(i);
                        i++;
                        if (i > n) t.Dispose();
                    }
                }, null, 300, 150);
                return;
            }

            System.Threading.Timer seqTimer = null;
            int step = 0;
            seqTimer = new System.Threading.Timer(delegate
            {
                if (step < result.ActivationsDetailed.Count)
                {
                    var item = result.ActivationsDetailed[step];
                    SpecialType type = item.Item1;
                    int x = item.Item2;
                    int y = item.Item3;
                    bool isVert = item.Item4;

                    switch (type)
                    {
                        case SpecialType.Striped:
                            PlayLineBlastSweep(x, y, !isVert);
                            break;
                        case SpecialType.Wrapped:
                            PlayWrappedExplosion(x, y);
                            break;
                        case SpecialType.ColorBomb:
                            PlayColorBombSweep(x, y);
                            break;
                        case SpecialType.Fish:
                            PlaySound("fish_eating", x, y);
                            PlaySound("fish_bite", x, y);
                            break;
                    }
                    PlayMatchSound(Math.Min(12, step + 1), x);
                    step++;
                }
                else
                {
                    seqTimer.Dispose();
                }
            }, null, 400, 220);
        }

        public static void PlayLineBlastSweep(int x, int y, bool isHorizontal = true)
        {
            if (isHorizontal)
            {
                System.Threading.Timer t = null;
                int c = 0;
                t = new System.Threading.Timer(delegate
                {
                    if (c < 8)
                    {
                        PlaySound("lineblast", c, y, 1.0 + (c * 0.02), 1.2);
                        c += 2;
                        if (c >= 8) t.Dispose();
                    }
                }, null, 0, 45);
            }
            else
            {
                System.Threading.Timer t = null;
                int r = 0;
                t = new System.Threading.Timer(delegate
                {
                    if (r < 8)
                    {
                        PlaySound("lineblast", x, r, 1.1 - (r * 0.02), 1.2);
                        r += 2;
                        if (r >= 8) t.Dispose();
                    }
                }, null, 0, 45);
            }
        }

        public static void PlayWrappedExplosion(int x, int y)
        {
            PlaySound("wrapped_explosion", x, y, 0.95, 1.6);
        }

        public static void PlayColorBombSweep(int centerX, int centerY)
        {
            PlaySound("colorbomb", centerX, centerY, 1.0, 1.6);
            System.Threading.Timer t = null;
            int step = 0;
            t = new System.Threading.Timer(delegate
            {
                step++;
                if (step <= 4)
                {
                    int leftCol = Math.Max(0, centerX - step * 2);
                    int rightCol = Math.Min(7, centerX + step * 2);
                    PlaySound("candy", leftCol, centerY, 1.2 + (step * 0.05), 0.8);
                    PlaySound("candy", rightCol, centerY, 1.2 + (step * 0.05), 0.8);
                }
                else
                {
                    t.Dispose();
                }
            }, null, 60, 50);
}
        
        public static bool BinauralAmbientEnabled = true;

        public static void PlayBinauralAmbientShimmer()
        {
            if (!_initialized || !BinauralAmbientEnabled) return;
            for (int i = 0; i < 8; i++)
            {
                int col = i;
                int row = 3 + Rng.Next(2);
                float pitch = 1.3f + (col * 0.02f);
                System.Threading.Timer t = null;
                int step = 0;
                t = new System.Threading.Timer(delegate
                {
                    step++;
                    if (step <= 3)
                    {
                        PlaySound("candy", col, row, pitch, 0.15f);
                    }
                    else
                    {
                        t.Dispose();
                    }
                }, null, 2000 + Rng.Next(3000), 800 + Rng.Next(400));
            }
        }
        
        public static void StopAll()
        {
            if (_duckTimer != null)
            {
                _duckTimer.Dispose();
                _duckTimer = null;
            }
            lock (ActiveStreams)
            {
                foreach (int h in ActiveStreams)
                {
                    BASS_ChannelStop(h);
                    BASS_StreamFree(h);
                }
                ActiveStreams.Clear();
            }
            lock (SyncDelegates)
            {
                SyncDelegates.Clear();
            }
            StopMusic();
        }
    }
}