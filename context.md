# Candy Crush Accessible - Contexto del Proyecto

## Resumen Ejecutivo

**Proyecto:** Versión accesible de Candy Crush Saga 2012 (C#/.NET 8 + BASS + NVDA/SAPI)  
**Estudio / Publisher:** Narayan Projects / RHcomunications  
**Versión:** v1.1.4 Lanzamiento Oficial (16 de Agosto de 2026)  
**Idiomas:** Español / Inglés (Localización completa)  
**Accesibilidad:** 100% jugable sin visión (screen reader, audio binaural 3D, navegación por teclado, actualizador OTA nativo automático y manual, panel táctico de potenciadores, manual README.html)  
**Estado:** Lanzamiento Oficial publicado en GitHub Release v1.1.4-08.16.2026 (`RHcomunications/CandyCrushAccessible`), 0 errores, 0 advertencias, 100% tests en verde.

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
   - Expansión de `Levels.cs` con 65 niveles guionizados artesanales cubriendo los 7 episodios originales (Prados Deliciosos, Cafetería de Postres, Bosque de Gomitas, Laguna de Limonada, Montaña de Mentebruma, Cañón de Caramelo, Valle del Malvavisco).
   - Modo procedural infinito activo desde el Nivel 66 en adelante.

8. **Manifiesto y Despliegue de Gala (`README.md` & GitHub Release)**:
   - `CandyCrushAccessible.csproj` firmado con metadatos oficiales de **Narayan Projects**.
   - `README.md` bilingüe creado en la raíz del proyecto.
1. **Lenguaje / Framework**: C# (.NET 8.0, Windows Forms x64).
2. **Audio**: BASS Audio Engine (`bass.dll` nativo) con `SoundEngine.cs` (cálculo de atenuación, volumen, paneo estereofónico y pitch dinámico por coordenadas).
3. **Accesibilidad**: Interop C nativo de NVDA (`nvdaControllerClient64.dll`) + fallback a SAPI (`System.Speech.Synthesis`) en `Speech.cs`.
4. **Motor de Juego (`src/Engine`)**:
   - `Board.cs`: Cuadrícula 8x8, simulación de gravedad, cascadas, matches de 3/4/5, caramelos especiales, combos legendarios, obstáculos (gelatina, glaseado, chocolate, regaliz) e ingredientes (cerezas, avellanas).
   - `Levels.cs`: Definición manual de los 65 niveles originales de 2012 + generador algorítmico infinito a partir del Nivel 66.
   - `Progress.cs`: Persistencia atómica de progreso del jugador, puntuaciones máximas, estrellas, vidas, lingotes de oro, monedas y boosters.
   - `Boosters.cs`: Definición y lógica de potenciadores (Martillo de Piruleta, Bomba de Color, Peces de Gelatina, Movimientos Extra, Tiempo Extra).
   - `DailyBonus.cs`: Rueda de recompensas diaria persistente.
   - `Updater.cs`: Comprobación automática de versiones contra la API pública de GitHub Releases con bypass de Rate-Limit HTTP y script `apply_update.cmd` para reinicio en caliente.
5. **Interfaz Gráfica / Accesible (`src/UI`)**:
   - `MainWindow.cs`: Bucle principal de eventos, máquina de estados de pantallas (`GameScreen`), renderizado GDI+ de alto contraste, navegación milimétrica por teclado y lector de coordenadas con audio espacial 3D.
6. **Localización (`src/Localization`)**:
   - `Localization.cs`: Diccionario bilingüe (Español e Inglés) para todas las cadenas de texto, mensajes de voz y consejos del Señor Toffee.

7. **Sonidos y Música**:
   - 153 efectos de sonido y pistas musicales auténticas de King (2012) organizados en `sounds_legacy/`.

8. **Control de Vidas y Economía**:
   - 5 vidas máximas, regeneración pasiva de 1 vida cada 30 minutos (incluso con el juego cerrado).
   - Bono diario cada 24 horas.

9. **Actualizador OTA Integrado**:
   - Chequeo al inicio y opción manual en el menú de Opciones.

10. **Panel Táctico, Calidad de Vida y Claridad Total en Niveles**:
    - **Panel Táctico de Potenciadores (Tecla `TAB`)**: Despliegue de panel contextual durante la partida para aplicar martillos, bombas de color, peces de gelatina, movimientos extra y tiempo extra en casillas exactas.
    - **Consulta de Vidas en Tiempo Real (Tecla `V`)**: Anuncio exacto de minutos y segundos restantes para la siguiente vida.
    - **Bono Diario Humanizado**: Formato en horas y minutos (`18h y 45m`).
    - **Manual de Usuario Integrado (`README.html`)**: Manual estilizado interactivo bilingüe (ES/EN) empaquetado en la raíz del juego.
    - **Niveles de Ingredientes Explícitos**: Nombres claros (Cereza / Avellana), colocación garantizada desde el turno 1 (`y = 0`) y anuncio de coordenadas en tiempo real (`R` / `2`).
    - **Niveles de Pedidos Desglosados**: Objetivos detallados item por item con seguimiento individual de progreso.

11. **Calibración de Puntuaciones Oficiales 2012 y Blindaje de Actualizador OTA**:
    - **Tabla Oficial de Puntuaciones 2012**: Rebalanceo completo de combinaciones especiales:
      - Bomba de Color + Bomba de Color: **+5,000 pts** (+ destrucción total de las 64 casillas).
      - Bomba de Color + Caramelo Especial (Rayado/Envuelto): **+4,000 pts**.
      - Caramelo Envuelto + Caramelo Envuelto: **+3,500 pts**.
      - Caramelo Rayado + Caramelo Envuelto: **+3,000 pts**.
      - Caramelo Rayado + Caramelo Rayado: **+2,000 pts**.
      - Pez de Gelatina + Rayado / Envuelto: **+2,500 / +3,000 pts**.
      - Destrucción de Gelatina: **+1,000 pts** por capa.
      - Destrucción de Glaseado / Merengue, Regaliz y Chocolate: **+200 pts** por bloque.
      - Destrucción básica de caramelos en cascada: **20 pts por caramelo * nivel de cascada** (60 pts para un match de 3).
    - **Blindaje del Actualizador OTA**: Inserción de `Environment.Exit(0)` inmediato al lanzar el instalador temporal y migración a `[System.IO.Compression.ZipFile]::ExtractToDirectory(..., $true)` con sobreescritura nativa forzada.

12. **Preservación Histórica 2012 y Suite de Pruebas de Humo Automatizada**:
    - **Integración Nativa `sounds_legacy`**: Todos los 153 archivos de audio auténticos de 2012 mapeados 1 a 1 en `AudioMap.cs`.
    - **Banda Sonora Contextual (OST 2012)**: Asignación dinámica en `MusicMap.cs` para Menú (`loop_1`), Victoria (`outro1`), Derrota (`intro2`), Puntuación/Gelatina (`loop5`), Pedidos (`soundtrack2`), Tiempo (`soundtrack3`) e Ingredientes (`soundtrack4`).
    - **Smoke Test Automatizado (`tests/SmokeTest`)**: Suite de validación automatizada que certifica 100% de éxito en resolución de sonidos, instanciación de 65 niveles y colocación táctica de potenciadores.

---

## Regla de Oro: Esquema Oficial de Versionado Semántico (SemVer) y Despliegues

Para todas las versiones futuras (parches de sonido, correcciones, eventos festivos o grandes expansiones), se mantendrá obligatoriamente la regla de oro:

### Formato de Etiquetado: `vMayor.Menor.Parche-MM.DD.AAAA`

| Tipo de Lanzamiento | Cuándo Usarlo | Ejemplo de Etiqueta (Tag) | Ejemplo de Título de Release |
|---|---|---|---|
| **Hotfixes / Parches Menores** | Arreglos de sonido, pulido de efectos, textos o bugs | `v1.1.1-08.16.2026`<br>`v1.1.2-08.20.2026` | *Candy Crush Accesible v1.1.1 (Audio Polish)* |
| **Eventos / Contenido Temático** | Niveles especiales (Navidad, Halloween, etc.), nuevos episodios, modos | `v1.2.0-12.24.2026`<br>`v1.3.0-04.05.2027` | *Candy Crush Accesible v1.2.0 (Especial Navideño 🎄)* |
| **Grandes Expansiones (V2)** | Nuevo motor, multijugador, overhaul gráfico/sonoro | `v2.0.0-08.14.2027` | *Candy Crush Accesible 2.0 (Anniversary Edition)* |

### Protocolo de Despliegue en 3 Pasos:
1. **Actualizar `CandyCrushAccessible.csproj`**:
   Mantener `<AssemblyVersion>` y `<FileVersion>` limpios en 4 dígitos semánticos (`Mayor.Menor.Parche.0`), evitando años o fechas en estos campos:
   ```xml
   <Version>1.2.0</Version>
   <AssemblyVersion>1.2.0.0</AssemblyVersion>
   <FileVersion>1.2.0.0</FileVersion>
   ```
2. **Compilar y Empaquetar Standalone**:
   ```powershell
   & "$env:LOCALAPPDATA\dotnet\dotnet.exe" publish "src\CandyCrushAccessible.csproj" -c Release -o "publish_out"
   Compress-Archive -Path "publish_out\*" -DestinationPath "CandyCrushAccessible-v1.2.0-Standalone.zip" -Force
   ```
3. **Publicar en GitHub Release**:
   ```powershell
   gh release create v1.1.5-08.16.2026 "CandyCrushAccessible-v1.1.5-Standalone.zip" --title "Candy Crush Accesible v1.1.5" --notes "Notas del parche..."
   ```

---

## Estado de Certificación Final

- **Tests Automatizados (`tests/SmokeTest`)**: **ALL TESTS PASSED (100% OK, 0 Errores)**.
- **Compilaciones**: Debug y Release OK (0 Errores, 0 Advertencias).
- **GitHub Release**: [https://github.com/RHcomunications/CandyCrushAccessible/releases/tag/v1.1.5-08.16.2026](https://github.com/RHcomunications/CandyCrushAccessible/releases/tag/v1.1.5-08.16.2026)
## Contacto / Referencias
- Repo GitHub: [https://github.com/RHcomunications/CandyCrushAccessible](https://github.com/RHcomunications/CandyCrushAccessible)
- Release v1.1.5: [https://github.com/RHcomunications/CandyCrushAccessible/releases/tag/v1.1.5-08.16.2026](https://github.com/RHcomunications/CandyCrushAccessible/releases/tag/v1.1.5-08.16.2026)
- Audio assets: `C:\Users\artik\Downloads\candy crush\sounds_legacy\`
- Save usuario: `%APPDATA%\CandyCrushAccessible\candycrush_progress.json`