# Candy Crush Accessible - Contexto del Proyecto

## Resumen Ejecutivo

**Proyecto:** Versión accesible de Candy Crush Saga 2012 (C#/.NET 8 + BASS + NVDA/SAPI)
**Idiomas:** Español / Inglés
**Accesibilidad:** 100% jugable sin visión (screen reader, audio binaural, navegación por teclado)
**Estado:** Funcional, tests pasando, build Release OK

---

## Objetivo Principal

Recrear **fielmente** la experiencia de Candy Crush Saga original (2012) para jugadores ciegos/low-vision, manteniendo:
- Mecánicas idénticas (tablero 8x8, caramelos, especiales, obstáculos, tipos de nivel)
- Progresión original (30 niveles guionizados + infinitos procedurales por episodios)
- **Audio binaural signature Narayan Projects** (dulce, acaramelado, estilo Bejeweled 3)
- Sistema de vidas, boosters, tienda con moneda (lingotes/Gold Bars)
- Guardado persistente en `%APPDATA%\CandyCrushAccessible\candycrush_progress.json`

---

## Arquitectura Técnica

### Estructura de Carpetas
```
src/
├── Audio/
│   ├── AudioMap.cs      # Mapeo claves -> archivos .mp3
│   └── SoundEngine.cs   # Motor BASS.NET, audio binaural 3D
├── Engine/
│   ├── Board.cs         # Lógica central tablero (1772 líneas)
│   ├── Levels.cs        # Niveles 1-30 + generador procedural infinito
│   ├── Orders.cs        # Sistema de pedidos (Order levels)
│   ├── Boosters.cs      # Tipos y nombres de boosters
│   ├── Episodes.cs      # Episodios + música por episodio
│   ├── GameProgress.cs  # Guardado, vidas, lingotes, boosters, unlocks
│   └── Localization.cs  # ES/EN (550+ claves)
��── UI/
    └── MainWindow.cs    # WinForms, teclado, TTS, pantallas (1580+ líneas)
```

### Dependencias Críticas
- **Bass.Net** (audio 3D, streaming, efectos)
- **NAudio** (fallback/compatibilidad)
- **System.Text.Json** (serialización save)
- **System.Speech / NVDA Controller** (TTS)

### Compilación y Tests
```bash
# Debug
& "$env:LOCALAPPDATA\dotnet\dotnet.exe" build "src\CandyCrushAccessible.csproj" -c Debug

# Release
& "$env:LOCALAPPDATA\dotnet\dotnet.exe" build "src\CandyCrushAccessible.csproj" -c Release

# Tests motor (usar SIEMPRE antes de commit)
& "$env:LOCALAPPDATA\dotnet\dotnet.exe" run --project "$env:TEMP\opencode\engine_test\EngineTest.csproj" -c Debug
```

---

## Audio Binaural "Signature Narayan Projects"

### Implementación en `SoundEngine.cs`
| Característica | Implementación |
|---|---|
| **Pan por columna** | A=-1.0 (izq) → H=+1.0 (der) |
| **Profundidad por fila** | Pitch: `1.05 - 0.015*row` (fila 0 agudo → fila 7 grave), Vol: `0.80 + 0.35*row/7` |
| **Barrido línea horizontal** | `PlayLineBlastSweep` L→R (pan -1→+1 en 300ms) |
| **Barrido línea vertical** | `PlayLineBlastSweep` arriba→abajo (pitch descendente) |
| **Explosión wrapped** | `PlayWrappedExplosion` 8 direcciones radiales simultáneas |
| **Bomba de color** | `PlayColorBombSweep` barrido radial desde centro |
| **Ambiente** | `PlayBinauralAmbientShimmer` shimmer estéreo suave (opcional) |

### Eventos Espacializados (todos pasan `panCol, row`)
- Swap, Match, Especiales creados/activados
- Regaliz, Gelatina, Chocolate, Glaseado, Ingredientes, Bombas
- Combos en cascada (pitch ascendente por paso)
- Sugar Crush (ráfaga)

---

## Cambios Implementados en Esta Sesión

### 1. Fixes de Bugs Reportados

| Bug | Archivo | Línea | Solución |
|---|---|---|---|
| Volumen opciones: flechas invertidas | `MainWindow.cs` | 916-919 | Izq=-1, Der=+1 en `AdjustOption` |
| Sonido gelatina incorrecto | `AudioMap.cs` | 41-42 | `"jelly"` → `square-removed2.mp3` |
| Boosters usan sonido genérico | `MainWindow.cs` | 811, 870 | `"klubb"` (klubb-kross1.mp3) martillo y boosters inicio |
| Estado nivel (tecla R) no adapta por tipo | `Board.cs` | 1598-1624 | `StatusText()` incluye `extra` según `LevelType` |
| Formato status no incluía campo extra | `Localization.cs` | 135, 365 | `{5}` en `status.format` |

### 2. Persistencia de Partida (Ya funcionaba, validado)
- Guardado automático en `GameProgress.Save()` tras cada acción relevante
- Ruta: `%APPDATA%\CandyCrushAccessible\candycrush_progress.json`
- Tests usan path temporal vía `GameProgress.SetSavePathForTesting()` para no sobrescribir save real

### 3. Sistema de Moneda (Gold Bars) y Tienda — **NUEVO COMPLETO**

#### GameProgress.cs — Nuevos Campos y Métodos
```csharp
public int GoldBars = 0;
public DateTime DailyBonusDue = DateTime.MinValue;

public static int GetBoosterUnlockLevel(BoosterType type)
public bool IsBoosterUnlocked(BoosterType type)
public static int GetBoosterPrice(BoosterType type)
public void AddGoldBars(int amount)
public bool SpendGoldBars(int amount)
public bool TryCollectDailyBonus()    // 5 gold/24h
public double DailyBonusTimeRemaining()
public void AwardLevelCompletion(int stars)  // 1-3 gold por estrellas
```

#### Tabla Unlock/Precios (Fiel al Original 2012)
| Booster | Nivel Desbloqueo | Precio (Gold) |
|---|---|---|
| Lollipop Hammer | 8 | 9 |
| Extra Moves (+5) | 10 | 9 |
| Jelly Fish | 12 | 19 |
| Color Bomb | 19 | 29 |
| Extra Time (+15s) | 20 | 19 |

#### Pantalla Shop (`GameScreen.Shop`)
- Acceso desde Menú Principal (nueva opción "Tienda/Shop")
- Navegación Up/Down, Enter compra, Escape vuelve
- Muestra: nombre, precio, estado (bloqueado/precio/poseído), lingotes actuales
- Bono diario recargable (5 gold cada 24h)

#### Integración en Flujo de Juego
- **Sin boosters gratis al inicio** (eliminado `GrantStarterBoosters`)
- **Pantalla Boosters filtra solo desbloqueados** (`GetAvailableBoosters()`)
- **Fall screen**: opción "Comprar 5 movimientos (9 lingotes)"
- **Fix en Guardado de Progreso**: Se convirtieron los campos de `GameProgress.cs` a propiedades con getters/setters auto-implementados (`{ get; set; }`). La serialización System.Text.Json ahora guarda y carga correctamente `CurrentLevel`, `BestStars`, `BestScores`, `GoldBars`, `Coins` y `BoosterCounts`, manteniendo el progreso entre ejecuciones sin reiniciar el juego.
- **Sistema de Monedas Virtuales y Paquetes de Lingotes**:
  - Introducido el saldo de **Monedas** (`Coins`), obtenidas al ganar niveles (+20 por estrella) y en el bono diario (+50 monedas).
  - Añadidos paquetes de lingotes en la tienda comprables con monedas virtuales:
    - *Paquete Chico*: 10 Lingotes por 100 monedas.
    - *Paquete Mediano*: 30 Lingotes por 250 monedas.
    - *Paquete Grande*: 70 Lingotes por 500 monedas.
- **Nombres y Formas Auténticas de Caramelos**:
  - Se actualizaron las descripciones y lecturas de voz para reflejar la iconografía oficial de Candy Crush Saga (2012):
    - Red ➔ *frijol rojo* (Red Jelly Bean)
    - Blue ➔ *paleta azul* (Blue Lollipop Drop)
    - Green ➔ *gota verde* (Green Square Drop)
    - Yellow ➔ *gota amarilla* (Yellow Lemon Drop)
    - Orange ➔ *rombo naranja* (Orange Lozenge)
    - Purple ➔ *flor morada* (Purple Cluster)
- **Sugar Crush Épico con Efectos de Caramelos Especiales (Audio Espacializado)**:
  - Se implementó `ActivationsDetailed` en `Board.cs` para registrar cada caramelo especial detonado durante la ráfaga de Sugar Crush con su tipo y posición exacta `(x, y)`.
  - Se rediseñó `PlaySugarCrushSequence` en `SoundEngine.cs` para ejecutar la secuencia de audios propia de cada caramelo detonado (barrido de rayados con pitch, explosiones wrapped 3D, ondas de colorbomb y bocados de peces) distribuidos espacialmente en el campo auditivo en vivo.
- **Refactorización Completa y Auditoría de Código**:
  - **Ubicación Fiel de Caramelos Especiales**: Corregido el algoritmo de generación en `Board.cs`. Al realizar un swap o combinación, el especial creado (rayado, envuelto o bomba de color) se posiciona exactamente en la casilla de destino `(targetX, targetY)` tocada por el jugador.
  - **Limpieza y Estabilidad**: Auditoría de serialización, métodos auxiliares y eliminación de redundancias. Compilación limpia (0 advertencias, 0 errores en Debug y Release) y paso del 100% de la suite de pruebas del motor.

---

## Estado Actual del Código

### Tests: **TODOS PASAN** (Debug + Release)
```
Level 1-30 + Generados 31-60 + Licorice 8 + Orders 25/29 + Sugar Crush + Lives
ALL TESTS PASSED
```

### Build: **OK** (0 warnings, 0 errors)

### Funcionalidades Completas
- [x] 30 niveles guionizados + infinitos procedurales (episodios 10 niveles)
- [x] 5 tipos de nivel: Score, Jelly, Ingredient, Timed, Orders
- [x] Obstáculos: Gelatina (simple/doble), Glaseado, Chocolate, Regaliz, Bombas
- [x] Especiales: Rayado, Envuelto, Bomba color, Pez, combos
- [x] Sugar Crush (ráfaga final con movimientos restantes)
- [x] Vidas (regeneración 30 min, consumen solo al PERDER)
- [x] Audio binaural 3D completo (pan, profundidad, barridos, explosiones)
- [x] Navegación tablero con direcciones verbales ("Puedes intercambiar a la derecha con C2")
- [x] Pista verbal (H) sin mover cursor
- [x] Personaje Tiffi/Toffee al iniciar nivel (frases rotativas)
- [x] Secuencia estrellas victoria (1/2/3-star.mp3)
- [x] Música por episodio
- [x] **Sistema lingotes + tienda + unlocks progresivos + bono diario**
- [x] **Guardado persistente real**

---

## Pendientes / Próximos Pasos

### Prioridad Alta
- [ ] **Configuración usuario para ambiente binaural** (toggle On/Off en Opciones)
- [ ] Validación manual completa: partida real verificando audio 3D, navegación, tienda, persistencia

### Prioridad Media
- [ ] Tutorial interactivo actualizado con nuevo flujo boosters/tienda
- [ ] Sonidos adicionales: compra exitosa, error compra, bono diario recogido (actualmente usan "klubb"/"invalid")
- [ ] Indicador visual lingotes en HUD partida (badge esquina)

### Prioridad Baja / Nice-to-Have
- [ ] Logros/achievements (primer compra, racha diaria, etc.)
- [ ] Paquetes de lingotes "oferta" (simular IAP sin pago real)
- [ ] Estadísticas detalladas en menú (niveles jugados, oro ganado/gastado, boosters usados)

---

## Archivos Clave para Continuar

| Archivo | Responsabilidad |
|---|---|
| `src/Engine/Board.cs` | Motor tablero, detección matches, cascadas, estado nivel |
| `src/Engine/GameProgress.cs` | **Persistencia, vidas, lingotes, boosters, unlocks, daily bonus** |
| `src/Engine/Levels.cs` | Definición niveles 1-30 + generador procedural |
| `src/Engine/Localization.cs` | **Todas las cadenas ES/EN** |
| `src/Audio/SoundEngine.cs` | **Audio binaural 3D, secuencias, música** |
| `src/Audio/AudioMap.cs` | Mapeo sonido -> archivo |
| `src/UI/MainWindow.cs` | UI, teclado, pantallas, flujo juego, **Shop/Boosters/Fail integrados** |
| `%TEMP%\opencode\engine_test\Program.cs` | Suite tests automatizados |

---

## Notas para Próximos Asistentes

1. **NUNCA** asumir que un sonido existe en `AudioMap.cs` — verificar antes de usar clave nueva
2. **SIEMPRE** pasar coordenadas `(panCol, row)` a `SoundEngine.PlaySound()` para eventos espaciales
3. Tests en `$env:TEMP\opencode\engine_test\` usan save temporal — no tocar save real del usuario
4. `GameProgress.Load()` llama `TryCollectDailyBonus()` automáticamente al iniciar app
5. Boosters en pantalla de selección **filtrados por `IsBoosterUnlocked()`** — no hardcodear lista
6. Formato `status.format` usa 6 parámetros: nivel, score, target, objetivo, extra(jelly/ingredientes/orders/tiempo), movimientos

---

## Contacto / Referencias
- Repo original: (local) `C:\Users\artik\Downloads\candy crush`
- Audio assets: `C:\Users\artik\Downloads\candy crush\sounds\`
- Save usuario: `%APPDATA%\CandyCrushAccessible\candycrush_progress.json`