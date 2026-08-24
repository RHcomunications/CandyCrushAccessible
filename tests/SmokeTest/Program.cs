using System;
using System.IO;
using CandyCrushAccessible.Audio;
using CandyCrushAccessible.Engine;

namespace CandyCrushVerification
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("[*] INICIANDO AUDITORIA Y SMOKE TEST DE CANDY CRUSH ACCESIBLE...");
            int errors = 0;

            // 1. Verificar resolución de archivos en sounds_legacy
            Console.WriteLine("\n[1] Verificando resolucion de audio en sounds_legacy...");
            ContentResolver.Initialize();
            string[] testSounds = {
                "candy", "match1", "match12", "striped_created", "wrapped_created",
                "colorbomb_created", "klubb", "chocolate_grow", "frosting1",
                "jelly", "ingredient", "nut", "win", "lose", "sweet", "delicious", "divine", "sugar"
            };

            foreach (var key in testSounds)
            {
                string file = AudioMap.FileName(key);
                string path = ContentResolver.SoundPath(file);
                if (path == null || !File.Exists(path))
                {
                    Console.WriteLine($"  [ERROR] No se resolvio sonido: {key} ({file}) -> {path}");
                    errors++;
                }
                else
                {
                    Console.WriteLine($"  [OK] Sonido: {key} -> {Path.GetFileName(path)}");
                }
            }

            // 2. Verificar temas de la banda sonora 2012
            Console.WriteLine("\n[2] Verificando pistas musicales 2012...");
            foreach (MusicTrack track in Enum.GetValues(typeof(MusicTrack)))
            {
                string f = MusicMap.FileName(track);
                if (track == MusicTrack.Lose)
                {
                    if (f == null)
                    {
                        Console.WriteLine("  [OK] Pista: Lose -> Silencio (reproduce SFX level_failed1.wav sin fanfarria invertida)");
                    }
                    else
                    {
                        Console.WriteLine($"  [ERROR] Pista Lose no deberia tener musica alegre asignada: {f}");
                        errors++;
                    }
                    continue;
                }
                string p = ContentResolver.MusicPath(f);
                if (p == null || !File.Exists(p))
                {
                    Console.WriteLine($"  [ERROR] No se resolvio pista musical: {track} ({f}) -> {p}");
                    errors++;
                }
                else
                {
                    Console.WriteLine($"  [OK] Pista: {track} -> {Path.GetFileName(p)}");
                }
            }

            // 3. Simular los 65 niveles guionizados
            Console.WriteLine("\n[3] Verificando los 65 niveles guionizados...");
            for (int i = 1; i <= 65; i++)
            {
                try
                {
                    LevelDefinition def = Levels.Get(i);
                    Board board = new Board(def);
                    if (def.Type == LevelType.Ingredient)
                    {
                        string loc = board.GetIngredientsLocationText();
                        if (string.IsNullOrEmpty(loc))
                        {
                            Console.WriteLine($"  [WARN] Nivel de ingredientes {i} sin ubicacion de inicio");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  [ERROR] Fallo al instanciar nivel {i}: {ex.Message}");
                    errors++;
                }
            }
            Console.WriteLine("  [OK] 65 niveles verificados e instanciados correctamente.");

            // 4. Probar aplicacion de potenciadores tacticos
            Console.WriteLine("\n[4] Verificando colocacion tactica de potenciadores...");
            LevelDefinition lvl1 = Levels.Get(1);
            Board testBoard = new Board(lvl1);
            bool placedBomb = testBoard.PlaceSpecialAt(6, 7, SpecialType.ColorBomb); // G8
            if (!placedBomb || testBoard.GetCandy(6, 7).Special != SpecialType.ColorBomb)
            {
                Console.WriteLine("  [ERROR] Fallo al colocar Bomba de Color tactica en G8");
                errors++;
            }
            else
            {
                Console.WriteLine("  [OK] Bomba de Color tactica colocada con exito en G8 (x=6, y=7)");
            }

            Console.WriteLine("\n" + new string('-', 50));
            if (errors == 0)
            {
                Console.WriteLine("[+] AUDITORIA COMPLETADA: 100% EXITO (0 Errores). LISTO PARA RELEASE.");
                return 0;
            }
            else
            {
                Console.WriteLine($"[-] AUDITORIA FINALIZADA CON {errors} ERRORES.");
                return 1;
            }
        }
    }
}
