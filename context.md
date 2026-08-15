# Candy Crush Accessible - Contexto del Proyecto

## Resumen Ejecutivo

**Proyecto:** Versión accesible de Candy Crush Saga 2012 (C#/.NET 8 + BASS + NVDA/SAPI)  
**Estudio / Publisher:** Narayan Projects / RHcomunications  
**Versión:** v1.0 Lanzamiento Oficial (14 de Agosto de 2026)  
**Idiomas:** Español / Inglés (Localización completa)  
**Accesibilidad:** 100% jugable sin visión (screen reader, audio binaural 3D, navegación por teclado)  
**Estado:** Lanzamiento Oficial publicado en GitHub Release v1.0-08.14.2026 (`RHcomunications/CandyCrushAccessible`), 0 errores, 0 advertencias, 100% tests en verde.

---

## Objetivo Principal

Recrear **fielmente** la experiencia de Candy Crush Saga original (2012) para jugadores ciegos/low-vision, manteniendo:
- Mecánicas idénticas (tablero 8x8, caramelos, especiales, obstáculos, tipos de nivel).
- Progresión original de lanzamiento (65 niveles guionizados a mano en 7 episodios + infinitos procedurales a partir del Nivel 66).
- **Audio binaural signature Narayan Projects** (dulce, acaramelado, estilo Bejeweled 3).
- Sistema de vidas, boosters, tienda con economía dual (lingotes de oro / monedas virtuales).
- Guardado persistente en `%APPDATA%\CandyCrushAccessible\candycrush_progress.json`.

---

## Arquitectura Técnica

### Estructura de Carpetas
```
src/
├── Accessibility/
│   └── Speech.cs        # Interop NVDA Controller Client / SAPI Speech Fallback
├── Audio/
│   ├── AudioMap.cs      # Mapeo de claves -> archivos .mp3 (incluye efectos de tienda y especiales)
│   ├── ContentResolver.cs # Resolución de recursos incrustados y en disco
│   ├── MusicMap.cs      # Mapeo de pistas de música por episodio y pantallas
│   └── SoundEngine.cs   # Motor BASS.NET, audio binaural 3D, barridos de especiales y shimmer
├── Engine/
│   ├── Board.cs         # Lógica central del tablero y física de caramelos
│   ├── Boosters.cs      # Tipos y nombres de potenciadores
│   ├── Candy.cs          # Definición de tipos y colores de caramelos
│   ├── Episodes.cs      # Definición de los 7 episodios originales (Niveles 1-65) y nombres procedurales
│   ├── GameProgress.cs  # Guardado JSON, vidas, lingotes, monedas, boosters, unlocks, IsDevMode
│   ├── Levels.cs        # 65 niveles guionizados artesanales + generador procedural infinito (>=66)
│   ├── Localization.cs  # Cadenas bilingües ES/EN
│   └── Orders.cs        # Sistema de pedidos (Order levels)
├── UI/
│   └── MainWindow.cs    # WinForms, teclado, renderizado, menú de opciones, tienda y tutorial
└── Program.cs           # Punto de entrada de la aplicación WinForms x64
```

### Compilación y Publicación
```bash
# Debug Build
& "$env:LOCALAPPDATA\dotnet\dotnet.exe" build "src\CandyCrushAccessible.csproj" -c Debug

# Release Build
& "$env:LOCALAPPDATA\dotnet\dotnet.exe" build "src\CandyCrushAccessible.csproj" -c Release

# Suite de Tests Automatizados del Motor
& "$env:LOCALAPPDATA\dotnet\dotnet.exe" run --project "$env:TEMP\opencode\engine_test\EngineTest.csproj" -c Debug
```

---

## Audio Binaural "Signature Narayan Projects"

### Implementación en `SoundEngine.cs`
| Característica | Implementación |
|---|---|
| **Pan por columna** | A=-1.0 (izq) → H=+1.0 (der) |
| **Profundidad por fila** | Pitch: `1.05 - 0.015*row` (fila 0 agudo → fila 7 grave), Vol: `0.80 + 0.35*row/7` |
| **Barrido línea horizontal (`striped`)** | `PlayLineBlastSweep` L→R (pan -1→+1 en 300ms) |
| **Barrido línea vertical (`striped`)** | `PlayLineBlastSweep` arriba→abajo (pitch descendente) |
| **Explosión radial (`wrapped`)** | `PlayWrappedExplosion` 8 direcciones radiales simultáneas |
| **Onda de choque (`colorbomb`)** | `PlayColorBombSweep` barrido radial desde el centro |
| **Ambiente Binaural** | `PlayBinauralAmbientShimmer` shimmer estéreo suave (toggleable en Opciones) |

---

## Novedades e Hitos de la Versión v1.0 (14 de Agosto de 2026)

1. **Fixes de Sonido y Música en Continuación / Menús**:
   - Corrección de la música del episodio al comprar +5 movimientos tras pantalla de derrota.
   - Reanudación automática del tema del menú (`MusicTrack.Menu`) al volver al menú principal o mapa de episodios.
   - Reparada excepción `FormatException` en la tienda del menú de derrota (`HandleFailedKeys`) pasando lingotes y monedas a `shop.gold`.

2. **Modo Desarrollador (Versión DEBUG)**:
   - Activación de `GameProgress.IsDevMode = true` en compilaciones DEBUG: otorga 99 vidas (sin consumo), 999 lingotes, 9999 monedas y todos los niveles desbloqueados para pruebas continuas de laboratorio.
   - Las pruebas automatizadas desactivan `IsDevMode` para certificar el comportamiento estándar de consumo de vidas y partidas reales.

3. **Corrección de Gravedad en Obstáculos (Nivel 12 e Ingredientes/Glaseados)**:
   - Se reescribió `GravityAndRefill` en `Board.cs` con un algoritmo iterativo completo que permite desplazar caramelos y rellenar casillas superiores sobre bloques destruidos o glaseados sin dejar celdas vacías flotantes.

4. **Toggle de Ambiente Binaural en Opciones**:
   - Propiedad `BinauralAmbientEnabled` persistida en el JSON de progreso.
   - Opción navegable en la pantalla de Opciones (`MainWindow.cs`) mediante flechas o Enter, con anuncio de voz de estado y early return en `SoundEngine.cs`.

5. **Audio Auténtico en la Economía y Tienda (`AudioMap.cs` & `MainWindow.cs`)**:
   - Asignación de audios dedicados para la tienda: `"shop_buy"` (`button-press.mp3`), `"shop_error"` (`negative-switch-sound1.mp3`) y `"daily_bonus"` (`episode-unlocked-fanfare.mp3`).

6. **Tutorial Interactivo de Caramelos Especiales (Teclas 1, 2 y 3)**:
   - Actualización pedagógica de `tutorial.page3` (ES/EN).
   - En la página 3 del tutorial, las teclas `1` (`striped`), `2` (`wrapped`) y `3` (`colorbomb`) ejecutan demostraciones auditivas aisladas binaurales sin alterar el progreso del usuario.

7. **Expansión "Dosis Original 2012" a 65 Niveles**:
   - Expansión de `Levels.cs` con 65 niveles guionados artesanales cubriendo los 7 episodios originales (Prados Deliciosos, Cafetería de Postres, Bosque de Gomitas, Laguna de Limonada, Montaña de Mentebruma, Cañón de Caramelo, Valle del Malvavisco).
   - Modo procedural infinito activo desde el Nivel 66 en adelante.

8. **Manifiesto y Despliegue de Gala (`README.md` & GitHub Release)**:
   - `CandyCrushAccessible.csproj` firmado con metadatos oficiales de **Narayan Projects**.
   - `README.md` bilingüe creado en la raíz del proyecto.
   - Repositorio público sincronizado en `RHcomunications/CandyCrushAccessible` y Release v1.0-08.14.2026 publicado formalmente con binarios ejecutable y DLLs nativas.

---

## Estado de Certificación Final

- **Tests Automatizados (`EngineTest.csproj`)**: **ALL TESTS PASSED** (Simulación completa de los 65 niveles guionados y generados).
- **Compilaciones**: Debug y Release OK (0 Errores, 0 Advertencias).
- **GitHub Release**: [https://github.com/RHcomunications/CandyCrushAccessible/releases/tag/v1.0-08.14.2026](https://github.com/RHcomunications/CandyCrushAccessible/releases/tag/v1.0-08.14.2026)
## Contacto / Referencias
- Repo GitHub: [https://github.com/RHcomunications/CandyCrushAccessible](https://github.com/RHcomunications/CandyCrushAccessible)
- Release v1.0: [https://github.com/RHcomunications/CandyCrushAccessible/releases/tag/v1.0-08.14.2026](https://github.com/RHcomunications/CandyCrushAccessible/releases/tag/v1.0-08.14.2026)
- Audio assets: `C:\Users\artik\Downloads\candy crush\sounds\`
- Save usuario: `%APPDATA%\CandyCrushAccessible\candycrush_progress.json`