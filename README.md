# Candy Crush Accessible (2012 Edition) 🍬🔊

**Developed by Narayan Projects**  
*Official Release Date: August 14, 2026*

---

## English

### Overview
**Candy Crush Accessible** is a faithful, 100% accessible digital preservation and standalone implementation of the original 2012 *Candy Crush Saga* launch experience. Engineered from the ground up in **C# / .NET 8**, it integrates a dedicated binaural audio engine, NVDA screen reader controller support, SAPI speech synthesis, and full keyboard navigation.

### Key Features
- **Signature Narayan Projects Audio Engine**: 3D binaural spatialized audio featuring column panning, row pitch scaling, horizontal/vertical line sweeps, radial wrapped explosions, and color bomb shockwaves.
- **65 Handcrafted Original Levels**: Recreating the first 7 episodes of the original 2012 release (*Mouth Watering Meadows*, *Dessert Diner*, *Gummy Grove*, *Lemonade Lake*, *Minty Meadow*, *Easter Bunne*, *Bubblegum Bridge*).
- **Infinite Procedural Mode**: Automatic seamless transition to algorithmically generated levels starting from Level 66 onward.
- **Full In-Game Economy & Shop**: Gold Bars, virtual Coins, Booster packages (Lollipop Hammer, Extra Moves, Jelly Fish, Color Bomb, Extra Time), and Daily Bonus rewards.
- **Accessibility & Customization**: Native NVDA screen reader interop, SAPI fallback, high-contrast visual UI, and a toggleable Binaural Ambient Shimmer in the Options menu.

### Controls & Keyboard Shortcuts
- **Navigation**: `Arrow Keys` or `WASD` to move cursor.
- **Select / Swap**: `Enter` or `Space` to select a candy, then `WASD` / `Arrow Keys` to swap with an adjacent cell.
- **Tactical Booster Panel & Controls**:
  - `TAB`: Open/Close In-Game Tactical Booster Panel.
  - `V`: Check real-time lives count and precise countdown to next life.
  - `C`: Repeat current cell coordinate and candy description.
  - `R`: Announce current score, moves left, and level objectives.
  - `B`: Read entire board cell-by-cell.
  - `T`: Read current row.
  - `G`: Read current column.
  - `H`: Ask Mr. Toffee for a move hint.
  - `L`: Use Lollipop Hammer booster on selected cell.
  - `F1`: Announce Mr. Toffee tip.
  - `P` / `Esc`: Pause menu.

### Version History (Changelog)
- **v1.1.6 (August 16, 2026)**: Fixed OTA extraction command in `apply_update.cmd` using native Windows `tar.exe -xf` with automated fallback to `Expand-Archive -LiteralPath -Force` to prevent PowerShell 5.1 type cast failures.
- **v1.1.5 (August 16, 2026)**: 100% authentic 2012 historical sound preservation (direct integration of all 153 assets in `sounds_legacy/`), contextual OST track mapping by mode (Menu, Win, Fail, Score/Jelly, Orders, Timed, Ingredients), and automated smoke test certification.
- **v1.1.4 (August 16, 2026)**: Calibrated 2012 scoring table for combos (Color Bomb + Color Bomb +5000, Bomb + Special +4000, Wrapped + Wrapped +3500, Striped + Wrapped +3000, Striped + Striped +2000) and obstacles (Jelly +1000, Frosting/Licorice/Chocolate +200), plus hardened OTA updater with `ZipFile.ExtractToDirectory` and immediate clean process shutdown.
- **v1.1.3 (August 15, 2026)**: Critical standalone hotfix embedding `SelfContained` runtime directly into csproj to ensure 100% of .NET 8 runtime DLLs (`coreclr.dll`) are bundled without requiring pre-installed frameworks.
- **v1.1.2 (August 15, 2026)**: Explicit ingredient names (cherries/nuts), guaranteed turn-1 board placement, real-time board tile coordinates (`R` / `2`), and detailed itemized order objectives.
- **v1.1.1 (August 15, 2026)**: Added Tactical Booster Panel (`TAB` key) for in-game precision booster application, real-time live check (`V` key), human-readable Daily Bonus timer (hours/mins), and integrated `README.html` user manual.
- **v1.1.0 (August 15, 2026)**: Major OTA updater hotfix (strict SemVer upgrade to `1.1.0` resolving prior build math comparisons, active process wait loop and audio device shutdown in `apply_update.cmd`).
- **v1.0.3 (August 15, 2026)**: Fixed BASS PInvoke delegate memory cleanup on `BASS_SYNC_END`, continuous background life regeneration across all screens, and Level 65 end-of-episode transition to procedural mode.
- **v1.0.2 (August 15, 2026)**: Manual updater check button in Options, 5% granular volume control with JSON persistence, and removed music ducking.
- **v1.0.1 (August 15, 2026)**: Self-contained standalone package deployment.
- **v1.0.0 (August 14, 2026)**: Initial official release with 65 levels, 3D binaural audio engine, and shop economy.

---

## Español

### Descripción General
**Candy Crush Accesible** es una recreación fiel y 100% accesible de la experiencia original de lanzamiento de *Candy Crush Saga* (2012). Desarrollado desde cero en **C# / .NET 8**, integra un motor de audio binaural dedicado, compatibilidad nativa con el lector de pantalla NVDA, síntesis de voz SAPI y navegación completa por teclado.

### Características Principales
- **Motor de Audio "Signature Narayan Projects"**: Audio espacial 3D binaural con paneo por columna, pitch dinámico por fila, barridos de línea horizontales/verticales, explosiones radiales de caramelos envueltos y ondas expansivas de bombas de color.
- **65 Niveles Artesanales Fieles al Original**: Recreación exacta de los primeros 7 episodios de 2012 (*Prados Deliciosos*, *Cafetería de Postres*, *Bosque de Gomitas*, *Laguna de Limonada*, *Montaña de Mentebruma*, *Cañón de Caramelo*, *Valle del Malvavisco*).
- **Modo Procedural Infinito**: Transición fluida y automática a niveles generados por algoritmo a partir del Nivel 66 en adelante.
- **Economía Completa y Tienda**: Lingotes de Oro, Monedas virtuales, paquetes de Potenciadores (Martillo de Piruleta, Movimientos Extra, Peces de Gelatina, Bomba de Color, Tiempo Extra) y Bono Diario.
- **Accesibilidad y Personalización**: Interop nativo con NVDA, síntesis SAPI, interfaz visual de alto contraste y conmutador (Toggle) de Ambiente Binaural en el menú de Opciones.
- **Manual Integrado**: Documentación completa e interactiva en el archivo `README.html` incluido en la carpeta del juego.

### Controles y Atajos de Teclado
- **Navegación**: `Flechas` o `WASD` para mover el cursor.
- **Seleccionar / Intercambiar**: `Intro` o `Espacio` para seleccionar un caramelo, luego `WASD` / `Flechas` para intercambiar con la celda vecina.
- **Panel Táctico y Accesibilidad**:
  - `TAB`: Abrir/Cerrar el Panel Táctico de Potenciadores sobre la casilla actual.
  - `V`: Consultar vidas restantes y cuenta regresiva exacta para la próxima vida.
  - `C`: Repetir coordenada y descripción del caramelo actual.
  - `R`: Anunciar puntuación, movimientos restantes y objetivos del nivel.
  - `B`: Leer todo el tablero celda por celda.
  - `T`: Leer la fila actual.
  - `G`: Leer la columna actual.
  - `H`: Solicitar consejo de movimiento al Señor Toffee.
  - `L`: Usar el Martillo de Piruleta en la celda seleccionada.
  - `F1`: Escuchar consejo del Señor Toffee.
  - `P` / `Esc`: Menú de pausa.

### Historial de Versiones
- **v1.1.6 (16 de Agosto de 2026)**: Corrección crítica de extracción en `apply_update.cmd` mediante `tar.exe -xf` nativo con fallback a `Expand-Archive -LiteralPath -Force` para solventar fallos de tipos en PowerShell 5.1.
- **v1.1.5 (16 de Agosto de 2026)**: Preservación histórica 2012 pura (integración directa de los 153 assets en `sounds_legacy/`), mapeo dinámico de banda sonora por modo (Menú, Victoria, Derrota, Puntuación/Gelatina, Pedidos, Tiempo, Ingredientes) y certificación con suite de pruebas de humo automatizada.
- **v1.1.4 (16 de Agosto de 2026)**: Calibración oficial de la tabla de puntuaciones 2012 para combinaciones especiales (Bomba+Bomba +5000, Bomba+Especial +4000, Envuelto+Envuelto +3500, Rayado+Envuelto +3000, Rayado+Rayado +2000) y obstáculos (Gelatina +1000, Glaseado/Regaliz/Chocolate +200), y blindaje del actualizador OTA con `ZipFile.ExtractToDirectory` y cierre de proceso inmediato.
- **v1.1.3 (15 de Agosto de 2026)**: Hotfix crítico de empaquetado autocontenido (SelfContained embebido en csproj) que incluye todas las librerías de .NET 8 (`coreclr.dll`) sin requerir runtime externo.
- **v1.1.2 (15 de Agosto de 2026)**: Nombres explícitos de ingredientes (cerezas/avellanas), aparición garantizada desde el turno 1, anuncio en tiempo real de casillas en el tablero (`R` / `2`), y objetivos de pedidos desglosados item por item.
- **v1.1.1 (15 de Agosto de 2026)**: Panel Táctico de Potenciadores in-game (tecla `TAB`), consulta de vidas en tiempo real (`V`), formato humanizado del bono diario en horas y minutos, y manual interactivo `README.html`.
- **v1.1.0 (15 de Agosto de 2026)**: Hotfix mayor del actualizador OTA (salto a versión `1.1.0`, espera activa de proceso y liberación de controladores BASS antes de extraer).
- **v1.0.3 (15 de Agosto de 2026)**: Optimización de memoria en delegados BASS (`BASS_SYNC_END`), regeneración de vidas en segundo plano en todas las pantallas, y fanfarria de fin de episodio en Nivel 65.
- **v1.0.2 (15 de Agosto de 2026)**: Botón de actualización manual en Opciones, ajuste granular de volumen al 5% con persistencia y eliminación del ducking musical.
- **v1.0.1 (15 de Agosto de 2026)**: Despliegue autocontenido Standalone (.NET 8).
- **v1.0.0 (14 de Agosto de 2026)**: Lanzamiento oficial inicial.

---

### Credits & Technical Stack
- **Architecture**: MVC Pattern (Engine decoupled from WinForms UI & Speech).
- **Framework**: .NET 8 (Windows Desktop x64).
- **Audio Library**: BASS.Net Unmanaged Interop.
- **Publisher**: Narayan Projects / RHcommunications.
