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
- **Board Reading**:
  - `C`: Repeat current cell coordinate and candy description.
  - `R`: Announce current score, moves left, and level objectives.
  - `B`: Read entire board cell-by-cell.
  - `T`: Read current row.
  - `G`: Read current column.
  - `H`: Ask Mr. Toffee for a move hint.
  - `L`: Use Lollipop Hammer booster on selected cell.
  - `F1`: Announce Mr. Toffee tip.
  - `P` / `Esc`: Pause menu.
- **Interactive Tutorial**:
  - Press `1` on Page 3 for Striped Candy sweep demonstration.
  - Press `2` on Page 3 for Wrapped Candy explosion demonstration.
  - Press `3` on Page 3 for Color Bomb shockwave demonstration.

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

### Controles y Atajos de Teclado
- **Navegación**: `Flechas` o `WASD` para mover el cursor.
- **Seleccionar / Intercambiar**: `Intro` o `Espacio` para seleccionar un caramelo, luego `WASD` / `Flechas` para intercambiar con la celda vecina.
- **Lectura de Tablero**:
  - `C`: Repetir coordenada y descripción del caramelo actual.
  - `R`: Anunciar puntuación, movimientos restantes y objetivos del nivel.
  - `B`: Leer todo el tablero celda por celda.
  - `T`: Leer la fila actual.
  - `G`: Leer la columna actual.
  - `H`: Solicitar consejo de movimiento al Señor Toffee.
  - `L`: Usar el Martillo de Piruleta en la celda seleccionada.
  - `F1`: Escuchar consejo del Señor Toffee.
  - `P` / `Esc`: Menú de pausa.
- **Tutorial Interactivo**:
  - Pulsa `1` en la Página 3 para demostración de barrido de Caramelo Rayado.
  - Pulsa `2` en la Página 3 para demostración de explosión de Caramelo Envuelto.
  - Pulsa `3` en la Página 3 para demostración de onda expansiva de Bomba de Color.

---

### Credits & Technical Stack
- **Architecture**: MVC Pattern (Engine decoupled from WinForms UI & Speech).
- **Framework**: .NET 8 (Windows Desktop x64).
- **Audio Library**: BASS.Net Unmanaged Interop.
- **Publisher**: Narayan Projects / RHcommunications.
