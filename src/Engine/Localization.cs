using System;
using System.Collections.Generic;

namespace CandyCrushAccessible.Engine
{
    public enum Language
    {
        Spanish,
        English
    }

    public static class Localization
    {
        public static Language Current = Language.Spanish;

        public static string Get(string key)
        {
            if (Current == Language.Spanish)
            {
                if (Spanish.ContainsKey(key)) return Spanish[key];
            }
            else
            {
                if (English.ContainsKey(key)) return English[key];
            }
            return key;
        }

        public static string C(CandyColor color)
        {
            switch (color)
            {
                case CandyColor.Red: return Get("color.red");
                case CandyColor.Blue: return Get("color.blue");
                case CandyColor.Green: return Get("color.green");
                case CandyColor.Yellow: return Get("color.yellow");
                case CandyColor.Orange: return Get("color.orange");
                case CandyColor.Purple: return Get("color.purple");
            }
            return "";
        }

        public static string S(SpecialType special)
        {
            switch (special)
            {
                case SpecialType.Striped: return Get("special.striped");
                case SpecialType.Wrapped: return Get("special.wrapped");
                case SpecialType.ColorBomb: return Get("special.colorbomb");
                case SpecialType.Fish: return Get("special.fish");
            }
            return "";
        }

        public static string I(IngredientType ingredient)
        {
            switch (ingredient)
            {
                case IngredientType.Cherry: return Get("ingredient.cherry");
                case IngredientType.Nut: return Get("ingredient.nut");
            }
            return "";
        }

        public static string StarLabel(int stars)
        {
            return Get("star." + stars);
        }

        private static readonly Dictionary<string, string> Spanish = new Dictionary<string, string>
        {
            {"game.title", "Candy Crush Accesible"},
            {"game.subtitle", "Edición 2012"},
            {"mainmenu.play", "Jugar"},
            {"mainmenu.continue", "Continuar"},
            {"mainmenu.shop", "Tienda"},
            {"mainmenu.tutorial", "Tutorial"},
            {"mainmenu.options", "Opciones"},
            {"mainmenu.quit", "Salir"},
            {"mainmenu.prompt", "Usa las flechas arriba y abajo y pulsa intro para elegir"},

            {"menu.levelmap", "Mapa de niveles"},
            {"menu.level", "Nivel"},
            {"menu.back", "Volver"},

            {"color.red", "frijol rojo"},
            {"color.blue", "paleta azul"},
            {"color.green", "gota verde"},
            {"color.yellow", "gota amarilla"},
            {"color.orange", "rombo naranja"},
            {"color.purple", "flor morada"},

            {"special.striped", "rayado"},
            {"special.wrapped", "envuelto"},
            {"special.colorbomb", "bomba de color"},
            {"special.fish", "pez"},

            {"ingredient.cherry", "cereza"},
            {"ingredient.nut", "nuez"},

            {"element.jelly", "gelatina"},
            {"element.jelly2", "gelatina doble"},
            {"element.chocolate", "chocolate"},
            {"element.licorice", "regaliz"},
            {"element.bomb", "bomba"},
            {"element.timecandy", "caramelo de tiempo"},

            {"obj.score", "Consigue {0} puntos en {1} movimientos"},
            {"obj.score.short", "{0} puntos"},
            {"obj.jelly", "Elimina toda la gelatina en {0} movimientos"},
            {"obj.ingredient", "Lleva {0} a la parte inferior en {1} movimientos"},
            {"obj.timed", "Consigue {0} puntos en {1} segundos"},
            {"obj.order", "Completa los pedidos en {0} movimientos"},
            {"order.candy", "recoge {0} caramelos {1}"},
            {"order.striped", "recoge {0} caramelos rayados"},
            {"order.wrapped", "recoge {0} caramelos envueltos"},
            {"order.colorbomb", "recoge {0} bombas de color"},
            {"order.fish", "recoge {0} peces de caramelo"},
            {"order.remaining", "Pedidos restantes: {0}"},
            {"msg.order.updated", "Pedido: {0}"},
            {"order.complete", "Pedidos completados: {0} de {1}"},

            {"score", "Puntuación"},
            {"moves", "Movimientos"},
            {"time", "Tiempo"},
            {"moves.count", "Movimientos: {0}"},
            {"time.count", "Tiempo: {0}"},
            {"target", "Objetivo"},
            {"remaining", "Restantes"},
            {"jelly.remaining", "Gelatina restante: {0}"},
            {"jelly.status.detail", "Gelatina restante: {0} casillas ({1}% eliminado)"},
            {"score.status.detail", "Puntuación actual: {0} de {1} ({2}% completado)"},
            {"ingredient.status.detail", "Ingredientes restantes: {0} de {1} ({2}% recolectado)"},
            {"timed.status.detail", "Tiempo restante: {0}s de {1}s ({2}% del tiempo restante)"},
            {"order.status.detail", "Pedidos completados: {0} de {1} ({2}% completado), restan {3} por recolectar"},
            {"ingredients", "Ingredientes"},
            {"level", "Nivel"},
            {"cascade", "Cascada"},
            {"combo", "Combinación"},

            {"status.format", "Nivel {0}. Puntuación {1} de {2}. {3}. {4}. {5}"},
            {"cell.format", "{0}{1}: {2}"},
            {"cell.full", "{0}{1}: caramelo {2}"},
            {"cell.special", "{0}{1}: caramelo {2} {3}"},
            {"cell.ingredient", "{0}{1}: {2}"},
            {"cell.empty", "{0}{1}: vacío"},
            {"cell.chocolate", "{0}{1}: chocolate"},
            {"cell.jelly.suffix", ", {0}"},
            {"cell.locked", ", bloqueado con regaliz"},
            {"cell.frosting", ", cubierto de glaseado"},
            {"cell.frosting2", ", glaseado doble"},
            {"cell.bomb.suffix", ", bomba con {0} turnos"},
            {"cell.time.suffix", ", caramelo de tiempo"},
            {"row.read", "Fila {0}. {1}"},
            {"column.read", "Columna {0}. {1}"},

            {"msg.selected", "Seleccionado {0}"},
            {"msg.swapped", "Intercambio"},
            {"msg.invalid", "Movimiento no válido"},
            {"msg.locked", "Bloqueado con regaliz. Haz coincidencias junto a él para romperlo"},
            {"msg.frosted", "Cubierto de glaseado. Haz coincidencias junto a él para romperlo"},
            {"msg.match", "¡Combinación!"},
            {"msg.no.move", "No hay movimientos. Reorganizando tablero..."},
            {"msg.no.moves.hint", "No hay movimientos válidos. Pulsa H para un consejo"},
            {"msg.win", "¡Nivel completado!"},
            {"msg.lose", "Movimientos agotados. Has perdido"},
            {"msg.lose.time", "Tiempo agotado. Has perdido"},
            {"msg.star1", "Una estrella"},
            {"msg.star2", "Dos estrellas"},
            {"msg.star3", "Tres estrellas"},
            {"msg.sugar", "¡Sugar Crush! Dulces especiales por los movimientos restantes"},
            {"msg.newlevel", "Nivel {0} desbloqueado"},
            {"msg.hint", "Consejo: intercambia {0} y {1} para combinar"},
            {"msg.move.available", "Puedes intercambiar con {0}"},
            {"msg.move.available.dir", "Puedes intercambiar {0} con {1}"},
            {"dir.up", "arriba"},
            {"dir.down", "abajo"},
            {"dir.left", "a la izquierda"},
            {"dir.right", "a la derecha"},
            {"msg.hint.none", "No se encontró ningún consejo"},
            {"msg.cascade", "Cascada {0}"},
            {"msg.special.created", "¡Creado caramelo {0}!"},
            {"msg.bomb.warning", "¡Cuidado! La bomba en {0} explota en {1} turnos"},
            {"msg.bomb.explode", "¡Bomba explotada!"},
            {"msg.time5", "¡Caramelo de tiempo! +5 segundos"},
            {"msg.ingredient.arrive", "Ingrediente en la parte inferior. ¡Recógelo!"},
            {"msg.board", "Tablero"},

            {"hint.nav", "Flechas o WASD para moverte. Intro selecciona. C repite. R estado. B tablero. T fila. G columna. H consejo. L martillo. F1 consejo del Señor Toffee. P pausa"},

            {"tutorial.title", "Tutorial"},
            {"tutorial.page1", "Página 1 de 4. En el tablero, mueve el cursor con las flechas o WASD. Al moverte escucharás el color de cada caramelo y un sonido que sube o baja según la columna. Pulso de caramelo: mismo color más grave o agudo según la posición."},
            {"tutorial.page2", "Página 2 de 4. Para mover un caramelo, pulsa W, A, S o D para intercambiarlo con el vecino en esa dirección. Deben quedar tres o más del mismo color en línea. Intro selecciona un caramelo y luego puedes intercambiarlo."},
            {"tutorial.page3", "Página 3 de 4. Cuatro en línea crea un caramelo rayado (striped). Cinco en T o L crea un envuelto (wrapped). Cinco en línea crea una bomba de color (colorbomb). Un cuadrado de cuatro crea un pez. En esta página, pulsa 1 para escuchar el barrido horizontal/vertical del rayado, 2 para la explosión radial del envuelto o 3 para la onda expansiva de la bomba de color."},
            {"tutorial.page4", "Página 4 de 4. C repite el caramelo actual. R anuncia la puntuación y los objetivos. B lee todo el tablero. T lee la fila y G la columna. H da un consejo. L usa el martillo de piruleta. F1 pide un consejo al Señor Toffee. Esc o P abren la pausa. Antes de cada nivel puedes elegir potenciadores. El Señor Toffee te guiará en cada episodio."},

            {"options.title", "Opciones"},
            {"options.language", "Idioma"},
            {"options.music", "Volumen de música"},
            {"options.sfx", "Volumen de efectos"},
            {"options.voice", "Volumen de voz"},
            {"options.binaural", "Ambiente binaural"},
            {"options.value", "Opción {0}: {1}"},
            {"options.language.value", "Idioma: {0}"},
            {"options.binaural.value", "Ambiente binaural: {0}"},

            {"update.available", "Actualización disponible: {0}. Novedades: {1}. Pulsa Intro para descargar o Escape para ignorar por ahora."},
            {"update.downloading", "Descargando actualización. Pulsa 1 para megabytes descargados, 2 para tamaño total, 3 para velocidad o Espacio para porcentaje."},
            {"update.complete", "Descarga completada. El juego se reiniciará para aplicar la actualización."},
            {"update.mb_downloaded", "{0} megabytes descargados"},
            {"update.mb_total", "Tamaño total {0} megabytes"},
            {"update.speed", "{0} megabytes por segundo"},
            {"update.percent", "{0} por ciento"},

            {"pause.title", "Pausa"},
            {"pause.resume", "Reanudar"},
            {"pause.restart", "Reiniciar nivel"},
            {"pause.quit", "Salir al menú"},

            {"complete.title", "Nivel completado"},
            {"complete.score", "Puntuación: {0}"},
            {"complete.stars", "Estrellas: {0}"},
            {"lives.count", "Vidas: {0}"},
            {"lives.lost", "Has perdido una vida. Te quedan {0}."},
            {"msg.no.lives", "No te quedan vidas. La próxima llegará en {0} minutos."},
            {"msg.frosting.clear", "¡Glaseado destruido!"},
            {"complete.next", "Intro para continuar"},
            {"failed.title", "Nivel fallido"},
            {"failed.retry", "Reintentar"},
            {"failed.extramoves", "Comprar 5 movimientos ({0} lingotes)"},
            {"failed.menu", "Menú"},

            {"yes", "Sí"},
            {"no", "No"},
            {"none", "ninguno"},
            {"selected", "seleccionado"},
            {"ok", "Aceptar"},

            {"star.1", "una estrella"},
            {"star.2", "dos estrellas"},
            {"star.3", "tres estrellas"},
            {"stars.0", "cero estrellas"},
            {"stars.1", "una estrella"},
            {"stars.2", "dos estrellas"},
            {"stars.3", "tres estrellas"},

            {"voice.affirm.cascade", "¡Delicioso!"},
            {"voice.affirm.combo", "¡Tasty!"},
            {"voice.affirm.great", "¡Genial!"},
            {"voice.affirm.sweet", "¡Dulce!"},
            {"voice.affirm.awesome", "¡Increíble!"},

            {"lang.spanish", "Español"},
            {"lang.english", "English"},

            {"episode", "Episodio"},
            {"episode.1", "Prados Deliciosos"},
            {"episode.2", "Cafetería de Postres"},
            {"episode.3", "Bosque de Gomitas"},
            {"episode.4", "Laguna de Limonada"},
            {"episode.5", "Montaña de Mentebruma"},
            {"episode.6", "Cañón de Caramelo"},
            {"episode.7", "Valle del Malvavisco"},
            {"episode.intro.1", "Bienvenido a Prados Deliciosos. Soy el Señor Toffee, tu mayordomo. Tiffi está deseando verte jugar. ¡Buena suerte!"},
            {"episode.intro.2", "¡Increíble! Has llegado a la Cafetería de Postres. Tiffi está encantada. Sigue combinando caramelos."},
            {"episode.intro.3", "El Bosque de Gomitas te espera. Cuidado con el chocolate y las bombas. Tiffi confía en ti."},
            {"episode.intro.4", "¡Bienvenido a Laguna de Limonada! Refréscate pero ojo con los pedidos de caramelos especiales."},
            {"episode.intro.5", "Estás en la Montaña de Mentebruma. El glaseado y las bombas pondrán a prueba tus reflejos."},
            {"episode.intro.6", "¡Adelante en el Cañón de Caramelo! Mantén la calma frente al chocolate que se expande."},
            {"episode.intro.7", "¡Llegaste al Valle del Malvavisco! El pico máximo de la aventura original. Demuestra tu maestría."},
            {"episode.intro.new", "¡Nuevo episodio! Los caramelos no tienen fin. Tiffi y yo seguiremos contigo."},
            {"ep.name.adj.1", "Dulce"},
            {"ep.name.adj.2", "Dorado"},
            {"ep.name.adj.3", "Carmesí"},
            {"ep.name.adj.4", "Esmeralda"},
            {"ep.name.adj.5", "Brillante"},
            {"ep.name.adj.6", "Místico"},
            {"ep.name.adj.7", "Real"},
            {"ep.name.adj.8", "Florido"},
            {"ep.name.noun.1", "Bosque"},
            {"ep.name.noun.2", "Jardín"},
            {"ep.name.noun.3", "Montaña"},
            {"ep.name.noun.4", "Templo"},
            {"ep.name.noun.5", "Ciudad"},
            {"ep.name.noun.6", "Mansión"},
            {"ep.name.noun.7", "Torre"},
            {"ep.name.noun.8", "Valle"},
            {"ep.name.noun.9", "Bahía"},
            {"ep.name.noun.10", "Cascada"},
            {"episode.complete", "¡Episodio completado, Tiffi está orgullosa! El siguiente episodio es {0}."},

            {"character.toffee", "Señor Toffee"},
            {"character.tiffi", "Tiffi"},

            {"toffee.tip.1", "Consejo del Señor Toffee: cuatro en línea crean un caramelo rayado."},
            {"toffee.tip.2", "Consejo del Señor Toffee: cinco en línea crean una bomba de color. Úsala con otro caramelo."},
            {"toffee.tip.3", "Consejo del Señor Toffee: un cuadrado de cuatro crea un pez que destruye la gelatina."},
            {"toffee.tip.4", "Consejo del Señor Toffee: combina dos especiales entre sí para efectos gigantes."},
            {"toffee.tip.5", "Consejo del Señor Toffee: si no hay movimientos, pulsa H y te daré un consejo."},
            {"toffee.tip.6", "Consejo del Señor Toffee: el martillo de piruleta destruye cualquier caramelo u obstáculo."},
            {"toffee.level.1", "El Señor Toffee dice: ¡A por ello!"},
            {"toffee.level.2", "Tiffi dice: ¡Tú puedes!"},
            {"toffee.level.3", "El Señor Toffee dice: ¡Buena suerte!"},
            {"toffee.level.4", "Tiffi dice: ¡Aplasta esos caramelos!"},

            {"booster.hammer", "Martillo de piruleta"},
            {"booster.moves", "Movimientos extra"},
            {"booster.time", "Tiempo extra"},
            {"booster.colorbomb", "Bomba de color"},
            {"booster.fish", "Peces iniciales"},
            {"booster.shop", "Elige potenciadores para el nivel"},
            {"booster.play", "Jugar nivel"},
            {"booster.count", "Tienes {0}"},
            {"booster.selected", "Seleccionados {0} de 3"},
            {"booster.max", "Máximo tres potenciadores"},
            {"booster.used", "Usaste {0}. Te quedan {1}"},
            {"booster.awarded", "¡Ganaste potenciadores! {0}"},
            {"booster.applied", "Potenciadores activos: {0}"},
            {"booster.none", "No te quedan {0}"},
            {"booster.plus.moves", "¡Más cinco movimientos!"},
            {"booster.plus.time", "¡Más quince segundos!"},
            {"extra.moves.purchased", "¡Compraste {0} movimientos extra!"},
            {"hammer.used", "¡Martillo! Destruiste el caramelo en {0}"},

            {"shop.title", "Tienda"},
            {"shop.gold", "Lingotes: {0} | Monedas: {1}"},
            {"shop.lollipop", "Martillo de piruleta"},
            {"shop.extramoves", "Movimientos extra (+5)"},
            {"shop.jellyfish", "Peces de gelatina"},
            {"shop.colorbomb", "Bomba de color"},
            {"shop.extratime", "Tiempo extra (+15s)"},
            {"shop.goldpack1", "Paquete Chico: 10 Lingotes (100 monedas)"},
            {"shop.goldpack2", "Paquete Mediano: 30 Lingotes (250 monedas)"},
            {"shop.goldpack3", "Paquete Grande: 70 Lingotes (500 monedas)"},
            {"shop.daily", "Bono diario"},
            {"shop.price", "Precio: {0} lingotes"},
            {"shop.locked", "Bloqueado"},
            {"shop.unlock.at", "Se desbloquea en nivel {0}"},
            {"shop.owned", "Comprado"},
            {"shop.owned.count", "Tienes {0}"},
            {"shop.purchased", "¡Comprado!"},
            {"shop.notenough", "Lingotes insuficientes ({0} lingotes requeridos)"},
            {"shop.coins.notenough", "Monedas insuficientes. Gana más jugando niveles"},
            {"shop.collect", "Recoger"},
            {"shop.daily.wait", "Disponible en {0} min"},
            {"shop.daily.collected", "¡Bono diario recogido! +5 Lingotes y +50 Monedas"}
        };

        private static readonly Dictionary<string, string> English = new Dictionary<string, string>
        {
            {"game.title", "Accessible Candy Crush"},
            {"game.subtitle", "2012 Edition"},
            {"mainmenu.play", "Play"},
            {"mainmenu.continue", "Continue"},
            {"mainmenu.shop", "Shop"},
            {"mainmenu.tutorial", "Tutorial"},
            {"mainmenu.options", "Options"},
            {"mainmenu.quit", "Quit"},
            {"mainmenu.prompt", "Use up and down arrows and press enter to choose"},

            {"menu.levelmap", "Level map"},
            {"menu.level", "Level"},
            {"menu.back", "Back"},

            {"color.red", "red jelly bean"},
            {"color.blue", "blue lollipop drop"},
            {"color.green", "green square drop"},
            {"color.yellow", "yellow lemon drop"},
            {"color.orange", "orange lozenge"},
            {"color.purple", "purple cluster"},

            {"special.striped", "striped"},
            {"special.wrapped", "wrapped"},
            {"special.colorbomb", "color bomb"},
            {"special.fish", "fish"},

            {"ingredient.cherry", "cherry"},
            {"ingredient.nut", "hazelnut"},

            {"element.jelly", "jelly"},
            {"element.jelly2", "double jelly"},
            {"element.chocolate", "chocolate"},
            {"element.licorice", "licorice"},
            {"element.bomb", "bomb"},
            {"element.timecandy", "time candy"},

            {"obj.score", "Score {0} points in {1} moves"},
            {"obj.score.short", "{0} points"},
            {"obj.jelly", "Clear all the jelly in {0} moves"},
            {"obj.ingredient", "Get {0} to the bottom in {1} moves"},
            {"obj.timed", "Score {0} points in {1} seconds"},
            {"obj.order", "Complete the orders in {0} moves"},
            {"order.candy", "collect {0} {1} candies"},
            {"order.striped", "collect {0} striped candies"},
            {"order.wrapped", "collect {0} wrapped candies"},
            {"order.colorbomb", "collect {0} color bombs"},
            {"order.fish", "collect {0} candy fish"},
            {"order.remaining", "Remaining orders: {0}"},
            {"msg.order.updated", "Order: {0}"},
            {"order.complete", "Orders completed: {0} of {1}"},

            {"score", "Score"},
            {"moves", "Moves"},
            {"time", "Time"},
            {"moves.count", "Moves: {0}"},
            {"time.count", "Time: {0}"},
            {"target", "Target"},
            {"remaining", "Remaining"},
            {"jelly.remaining", "Jelly remaining: {0}"},
            {"jelly.status.detail", "Jelly remaining: {0} tiles ({1}% cleared)"},
            {"score.status.detail", "Current score: {0} of {1} ({2}% completed)"},
            {"ingredient.status.detail", "Ingredients remaining: {0} of {1} ({2}% collected)"},
            {"timed.status.detail", "Time remaining: {0}s of {1}s ({2}% time remaining)"},
            {"order.status.detail", "Orders completed: {0} of {1} ({2}% completed), {3} remaining to collect"},
            {"ingredients", "Ingredients"},
            {"level", "Level"},
            {"cascade", "Cascade"},
            {"combo", "Combo"},

            {"status.format", "Level {0}. Score {1} of {2}. {3}. {4}. {5}"},
            {"cell.format", "{0}{1}: {2}"},
            {"cell.full", "{0}{1}: {2} candy"},
            {"cell.special", "{0}{1}: {2} {3} candy"},
            {"cell.ingredient", "{0}{1}: {2}"},
            {"cell.empty", "{0}{1}: empty"},
            {"cell.chocolate", "{0}{1}: chocolate"},
            {"cell.jelly.suffix", ", {0}"},
            {"cell.locked", ", locked with licorice"},
            {"cell.frosting", ", covered in frosting"},
            {"cell.frosting2", ", double frosting"},
            {"cell.bomb.suffix", ", bomb with {0} turns"},
            {"cell.time.suffix", ", time candy"},
            {"row.read", "Row {0}. {1}"},
            {"column.read", "Column {0}. {1}"},

            {"msg.selected", "Selected {0}"},
            {"msg.swapped", "Swap"},
            {"msg.invalid", "Invalid move"},
            {"msg.locked", "Locked with licorice. Match candies next to it to break it"},
            {"msg.frosted", "Covered in frosting. Match candies next to it to break it"},
            {"msg.match", "Match!"},
            {"msg.no.move", "No moves. Reshuffling board..."},
            {"msg.no.moves.hint", "No valid moves. Press H for a hint"},
            {"msg.win", "Level completed!"},
            {"msg.lose", "No moves left. You lose"},
            {"msg.lose.time", "Time's up. You lose"},
            {"msg.star1", "One star"},
            {"msg.star2", "Two stars"},
            {"msg.star3", "Three stars"},
            {"msg.sugar", "Sugar Crush! Special candies for the remaining moves"},
            {"msg.newlevel", "Level {0} unlocked"},
            {"msg.hint", "Hint: swap {0} and {1} to match"},
            {"msg.move.available", "You can swap with {0}"},
            {"msg.move.available.dir", "You can swap {0} with {1}"},
            {"dir.up", "up"},
            {"dir.down", "down"},
            {"dir.left", "left"},
            {"dir.right", "right"},
            {"msg.hint.none", "No hint found"},
            {"msg.cascade", "Cascade {0}"},
            {"msg.special.created", "Created {0} candy!"},
            {"msg.bomb.warning", "Careful! The bomb at {0} explodes in {1} turns"},
            {"msg.bomb.explode", "Bomb exploded!"},
            {"msg.time5", "Time candy! +5 seconds"},
            {"msg.ingredient.arrive", "Ingredient at the bottom. Collect it!"},
            {"msg.board", "Board"},

            {"hint.nav", "Arrows or WASD to move. Enter selects. C repeats. R status. B board. T row. G column. H hint. L hammer. F1 Mr. Toffee tip. P pause"},

            {"tutorial.title", "Tutorial"},
            {"tutorial.page1", "Page 1 of 4. On the board, move the cursor with the arrows or WASD. As you move, you will hear each candy color and a sound that goes up or down depending on the column."},
            {"tutorial.page2", "Page 2 of 4. To move a candy, press W, A, S or D to swap it with the neighbor in that direction. Three or more of the same color must be in a line. Enter selects a candy and then you can swap it."},
            {"tutorial.page3", "Page 3 of 4. Four in a line creates a striped candy. Five in a T or L shape creates a wrapped candy. Five in a line creates a color bomb. A 2x2 square creates a fish. On this page, press 1 to listen to the striped horizontal/vertical sweep, 2 for the wrapped radial explosion, or 3 for the color bomb shockwave."},
            {"tutorial.page4", "Page 4 of 4. C repeats the current candy. R announces the score and objectives. B reads the whole board. T reads the row and G the column. H gives a hint. L uses the lollipop hammer. F1 asks Mr. Toffee for a tip. Esc or P open the pause menu. Before each level you can choose boosters. Mr. Toffee will guide you in each episode."},

            {"options.title", "Options"},
            {"options.language", "Language"},
            {"options.music", "Music volume"},
            {"options.sfx", "Sound effects volume"},
            {"options.voice", "Speech volume"},
            {"options.binaural", "Binaural ambient"},
            {"options.value", "Option {0}: {1}"},
            {"options.language.value", "Language: {0}"},
            {"options.binaural.value", "Binaural ambient: {0}"},

            {"update.available", "Update available: {0}. Release notes: {1}. Press Enter to download or Escape to ignore for now."},
            {"update.downloading", "Downloading update. Press 1 for downloaded megabytes, 2 for total size, 3 for speed, or Space for percentage."},
            {"update.complete", "Download complete. The game will restart to apply the update."},
            {"update.mb_downloaded", "{0} megabytes downloaded"},
            {"update.mb_total", "Total size {0} megabytes"},
            {"update.speed", "{0} megabytes per second"},
            {"update.percent", "{0} percent"},

            {"pause.title", "Pause"},
            {"pause.resume", "Resume"},
            {"pause.restart", "Restart level"},
            {"pause.quit", "Quit to menu"},

            {"complete.title", "Level completed"},
            {"complete.score", "Score: {0}"},
            {"complete.stars", "Stars: {0}"},
            {"lives.count", "Lives: {0}"},
            {"lives.lost", "You lost a life. You have {0} left."},
            {"msg.no.lives", "You have no lives. The next one arrives in {0} minutes."},
            {"msg.frosting.clear", "Frosting cleared!"},
            {"complete.next", "Press enter to continue"},
            {"failed.title", "Level failed"},
            {"failed.retry", "Retry"},
            {"failed.extramoves", "Buy 5 moves ({0} gold)"},
            {"failed.menu", "Menu"},

            {"yes", "Yes"},
            {"no", "No"},
            {"none", "none"},
            {"selected", "selected"},
            {"ok", "OK"},

            {"star.1", "one star"},
            {"star.2", "two stars"},
            {"star.3", "three stars"},
            {"stars.0", "zero stars"},
            {"stars.1", "one star"},
            {"stars.2", "two stars"},
            {"stars.3", "three stars"},

            {"voice.affirm.cascade", "Delicious!"},
            {"voice.affirm.combo", "Tasty!"},
            {"voice.affirm.great", "Great!"},
            {"voice.affirm.sweet", "Sweet!"},
            {"voice.affirm.awesome", "Awesome!"},

            {"lang.spanish", "Spanish"},
            {"lang.english", "English"},

            {"episode", "Episode"},
            {"episode.1", "Mouth Watering Meadows"},
            {"episode.2", "Dessert Diner"},
            {"episode.3", "Gummy Grove"},
            {"episode.4", "Lemonade Lake"},
            {"episode.5", "Minty Meadow"},
            {"episode.6", "Easter Bunne"},
            {"episode.7", "Bubblegum Bridge"},
            {"episode.intro.1", "Welcome to Mouth Watering Meadows. I am Mr. Toffee, your butler. Tiffi can't wait to watch you play. Good luck!"},
            {"episode.intro.2", "Amazing! You reached the Dessert Diner. Tiffi is thrilled. Keep matching candies."},
            {"episode.intro.3", "The Gummy Grove awaits. Watch out for the chocolate and the bombs. Tiffi trusts you."},
            {"episode.intro.4", "Welcome to Lemonade Lake! Refresh yourself but keep an eye on special candy orders."},
            {"episode.intro.5", "You are in Minty Meadow. Frosting and bombs will test your reflexes."},
            {"episode.intro.6", "Onward through Easter Bunne! Stay calm against expanding chocolate."},
            {"episode.intro.7", "You made it to Bubblegum Bridge! The peak of the original 2012 release. Prove your mastery."},
            {"episode.intro.new", "A new episode! The candies never end. Tiffi and I will stay with you."},
            {"ep.name.adj.1", "Sweet"},
            {"ep.name.adj.2", "Golden"},
            {"ep.name.adj.3", "Crimson"},
            {"ep.name.adj.4", "Emerald"},
            {"ep.name.adj.5", "Shimmering"},
            {"ep.name.adj.6", "Mystic"},
            {"ep.name.adj.7", "Royal"},
            {"ep.name.adj.8", "Floral"},
            {"ep.name.noun.1", "Forest"},
            {"ep.name.noun.2", "Garden"},
            {"ep.name.noun.3", "Mountain"},
            {"ep.name.noun.4", "Temple"},
            {"ep.name.noun.5", "City"},
            {"ep.name.noun.6", "Mansion"},
            {"ep.name.noun.7", "Tower"},
            {"ep.name.noun.8", "Valley"},
            {"ep.name.noun.9", "Bay"},
            {"ep.name.noun.10", "Cascade"},
            {"episode.complete", "Episode completed, Tiffi is proud! The next episode is {0}."},

            {"character.toffee", "Mr. Toffee"},
            {"character.tiffi", "Tiffi"},

            {"toffee.tip.1", "Mr. Toffee's tip: four in a line creates a striped candy."},
            {"toffee.tip.2", "Mr. Toffee's tip: five in a line creates a color bomb. Use it with another candy."},
            {"toffee.tip.3", "Mr. Toffee's tip: a 2x2 square creates a fish that destroys jelly."},
            {"toffee.tip.4", "Mr. Toffee's tip: combine two specials together for huge effects."},
            {"toffee.tip.5", "Mr. Toffee's tip: if there are no moves, press H and I will give you a hint."},
            {"toffee.tip.6", "Mr. Toffee's tip: the lollipop hammer destroys any candy or obstacle."},
            {"toffee.level.1", "Mr. Toffee says: Let's go!"},
            {"toffee.level.2", "Tiffi says: You can do it!"},
            {"toffee.level.3", "Mr. Toffee says: Good luck!"},
            {"toffee.level.4", "Tiffi says: Crush those candies!"},

            {"booster.hammer", "Lollipop hammer"},
            {"booster.moves", "Extra moves"},
            {"booster.time", "Extra time"},
            {"booster.colorbomb", "Color bomb"},
            {"booster.fish", "Jelly fish"},
            {"booster.shop", "Choose boosters for the level"},
            {"booster.play", "Play level"},
            {"booster.count", "You have {0}"},
            {"booster.selected", "Selected {0} of 3"},
            {"booster.max", "Maximum three boosters"},
            {"booster.used", "You used {0}. You have {1} left"},
            {"booster.awarded", "You earned boosters! {0}"},
            {"booster.applied", "Active boosters: {0}"},
            {"booster.none", "No {0} left"},
            {"booster.plus.moves", "Plus five moves!"},
            {"booster.plus.time", "Plus fifteen seconds!"},
            {"extra.moves.purchased", "Purchased {0} extra moves!"},
            {"hammer.used", "Hammer! You smashed the candy at {0}"},

            {"shop.title", "Shop"},
            {"shop.gold", "Gold Bars: {0} | Coins: {1}"},
            {"shop.lollipop", "Lollipop Hammer"},
            {"shop.extramoves", "Extra Moves (+5)"},
            {"shop.jellyfish", "Jelly Fish"},
            {"shop.colorbomb", "Color Bomb"},
            {"shop.extratime", "Extra Time (+15s)"},
            {"shop.goldpack1", "Small Pack: 10 Gold Bars (100 coins)"},
            {"shop.goldpack2", "Medium Pack: 30 Gold Bars (250 coins)"},
            {"shop.goldpack3", "Large Pack: 70 Gold Bars (500 coins)"},
            {"shop.daily", "Daily Bonus"},
            {"shop.price", "Price: {0} gold bars"},
            {"shop.locked", "Locked"},
            {"shop.unlock.at", "Unlocks at level {0}"},
            {"shop.owned", "Purchased"},
            {"shop.owned.count", "You have {0}"},
            {"shop.purchased", "Purchased!"},
            {"shop.notenough", "Not enough gold bars ({0} gold bars required)"},
            {"shop.coins.notenough", "Not enough coins. Earn more by playing levels"},
            {"shop.collect", "Collect"},
            {"shop.daily.wait", "Available in {0} min"},
            {"shop.daily.collected", "Daily bonus collected! +5 Gold Bars and +50 Coins"}
        };

        public static string LanguageName(Language lang)
        {
            return lang == Language.Spanish ? Get("lang.spanish") : Get("lang.english");
        }
    }
}