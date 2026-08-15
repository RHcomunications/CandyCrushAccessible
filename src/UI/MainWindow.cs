using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CandyCrushAccessible.Accessibility;
using CandyCrushAccessible.Audio;
using CandyCrushAccessible.Engine;

namespace CandyCrushAccessible.UI
{
    public enum GameScreen
    {
        Loading,
        MainMenu,
        LevelMap,
        Boosters,
        Shop,
        Playing,
        Options,
        Tutorial,
        Pause,
        Complete,
        Failed,
        UpdateAvailable
    }

    public class MainWindow : Form
    {
        private const int CellSize = 52;
        private const int BoardX = 40;
        private const int BoardY = 90;

        private GameScreen _screen = GameScreen.Loading;
        private GameProgress _progress;
        private Board _board;
        private int _levelNumber = 1;
        private int _cursorX = 3;
        private int _cursorY = 3;
        private int _selectedX = -1;
        private int _selectedY = -1;
        private int _menuIndex;
        private int _mapIndex;
        private int _optionsIndex;
        private int _pauseIndex;
        private int _tutorialPage;
        private int _boosterRow;
        private readonly List<BoosterType> _boosterSelection = new List<BoosterType>();
        private bool _timeWarningPlayed;
        private readonly System.Windows.Forms.Timer _timer;

        private bool _boosterPanelActive = false;
        private int _boosterPanelIndex = 0;
        private readonly List<BoosterType> _ownedBoosters = new List<BoosterType>();

        private readonly string[] MainMenuItems =
        {
            "mainmenu.play", "mainmenu.shop", "mainmenu.tutorial", "mainmenu.options", "mainmenu.quit"
        };

        private readonly string[] OptionsItems =
        {
            "options.language", "options.music", "options.sfx", "options.voice", "options.binaural", "options.update"
        };

        private readonly string[] PauseItems =
        {
            "pause.resume", "pause.restart", "pause.quit"
        };

        private readonly string[] FailItems =
        {
            "failed.retry", "failed.extramoves", "failed.menu"
        };

        private readonly string[] ShopItems =
        {
            "shop.lollipop", "shop.extramoves", "shop.jellyfish", "shop.colorbomb", "shop.extratime",
            "shop.goldpack1", "shop.goldpack2", "shop.goldpack3", "shop.daily"
        };

        private int _shopIndex = 0;

        public MainWindow()
        {
            Text = Localization.Get("game.title");
            ClientSize = new Size(560, 640);
            MinimumSize = new Size(560, 640);
            DoubleBuffered = true;
            KeyPreview = true;
            BackColor = Color.FromArgb(40, 36, 60);

            _timer = new System.Windows.Forms.Timer { Interval = 250 };
            _timer.Tick += TimerTick;

            _progress = GameProgress.Load();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
#if DEBUG
            GameProgress.IsDevMode = true;
            _progress.Lives = 99;
            if (_progress.GoldBars < 500) _progress.GoldBars = 999;
            if (_progress.Coins < 5000) _progress.Coins = 9999;
            if (_progress.CurrentLevel < Levels.TotalLevels) _progress.CurrentLevel = Levels.TotalLevels;
#endif
            ContentResolver.Initialize();
            SoundEngine.Init();
            SoundEngine.BinauralAmbientEnabled = _progress.BinauralAmbientEnabled;
            SoundEngine.MusicVolume = _progress.MusicVolume;
            SoundEngine.SfxVolume = _progress.SfxVolume;
            SoundEngine.VoiceVolume = _progress.VoiceVolume;
            Speech.Initialize();
            Localization.Current = _progress.LanguageSpanish ? Language.Spanish : Language.English;
            _timer.Start();

            CheckUpdateAvailableOnStartup();
        }

        private async void CheckUpdateAvailableOnStartup()
        {
            var update = await Updater.CheckForUpdatesAsync();
            if (update != null)
            {
                SwitchScreen(GameScreen.UpdateAvailable);
                Speech.SpeakInterrupt(string.Format(Localization.Get("update.available"), update.Version, update.ReleaseNotes));
            }
            else
            {
                SwitchScreen(GameScreen.MainMenu);
                SoundEngine.PlayMusic(MusicTrack.Menu);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timer.Stop();
            SoundEngine.Shutdown();
            base.OnFormClosed(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            switch (_screen)
            {
                case GameScreen.MainMenu: DrawMainMenu(g); break;
                case GameScreen.LevelMap: DrawLevelMap(g); break;
                case GameScreen.Boosters: DrawBoosters(g); break;
                case GameScreen.Shop: DrawShop(g); break;
                case GameScreen.Playing: DrawPlaying(g); break;
                case GameScreen.Options: DrawOptions(g); break;
                case GameScreen.Tutorial: DrawTutorial(g); break;
                case GameScreen.Pause: DrawPause(g); break;
                case GameScreen.Complete: DrawComplete(g); break;
                case GameScreen.Failed: DrawFailed(g); break;
                case GameScreen.UpdateAvailable: DrawUpdateAvailable(g); break;
                default: DrawLoading(g); break;
            }
        }

        private void SwitchScreen(GameScreen s)
        {
            _screen = s;
            _timeWarningPlayed = false;
            if (s == GameScreen.MainMenu)
            {
                SoundEngine.PlayMusic(MusicTrack.Menu);
                AnnounceMenu(MainMenuItems[_menuIndex]);
            }
            else if (s == GameScreen.LevelMap)
            {
                SoundEngine.PlayMusic(MusicTrack.Menu);
            }
            Invalidate();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            int livesBefore = _progress.Lives;
            _progress.UpdateLives();
            if (_progress.Lives != livesBefore)
            {
                _progress.Save();
                Speech.Speak(Localization.Get("lives.count") + " " + _progress.Lives);
            }
            if (_screen != GameScreen.Playing || _board == null || _board.Level.Type != LevelType.Timed) return;
            _board.UpdateTime(0.25);
            if (_board.TimeLeft <= 10 && _board.TimeLeft > 0 && !_timeWarningPlayed)
            {
                _timeWarningPlayed = true;
                SoundEngine.PlaySound("time_warning");
                Speech.Speak(string.Format(Localization.Get("time.count"), (int)Math.Ceiling(_board.TimeLeft)) + "...");
            }
            if (_board.TimeLeft <= 0 && !_board.Completed)
            {
                HandleLose();
            }
            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            bool handled = true;
            switch (_screen)
            {
                case GameScreen.MainMenu: HandleMainMenuKeys(e); break;
                case GameScreen.LevelMap: HandleLevelMapKeys(e); break;
                case GameScreen.Boosters: HandleBoostersKeys(e); break;
                case GameScreen.Shop: HandleShopKeys(e); break;
                case GameScreen.Playing: HandlePlayingKeys(e); break;
                case GameScreen.Options: HandleOptionsKeys(e); break;
                case GameScreen.Tutorial: HandleTutorialKeys(e); break;
                case GameScreen.Pause: HandlePauseKeys(e); break;
                case GameScreen.Complete: HandleCompleteKeys(e); break;
                case GameScreen.Failed: HandleFailedKeys(e); break;
                case GameScreen.UpdateAvailable: HandleUpdateKeys(e); break;
                default: handled = false; break;
            }
            if (handled) e.Handled = true;
            base.OnKeyDown(e);
        }

        private void HandleMainMenuKeys(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.W:
                    _menuIndex = (_menuIndex + MainMenuItems.Length - 1) % MainMenuItems.Length;
                    AnnounceMenu(MainMenuItems[_menuIndex]);
                    Invalidate();
                    break;
                case Keys.Down:
                case Keys.S:
                    _menuIndex = (_menuIndex + 1) % MainMenuItems.Length;
                    AnnounceMenu(MainMenuItems[_menuIndex]);
                    Invalidate();
                    break;
                case Keys.Enter:
                case Keys.Space:
                    SoundEngine.PlaySound("button");
                    switch (_menuIndex)
                    {
                        case 0:
                            _mapIndex = Math.Max(0, _progress.CurrentLevel - 1);
                            SwitchScreen(GameScreen.LevelMap);
                            AnnounceLevel(_mapIndex);
                            break;
                        case 1:
                            _shopIndex = 0;
                            SwitchScreen(GameScreen.Shop);
                            AnnounceShop();
                            break;
                        case 2:
                            _tutorialPage = 0;
                            SwitchScreen(GameScreen.Tutorial);
                            AnnounceTutorial();
                            break;
                        case 3:
                            _optionsIndex = 0;
                            SwitchScreen(GameScreen.Options);
                            AnnounceOptions();
                            break;
                        case 4:
                            Close();
                            break;
                    }
                    break;
                case Keys.Escape:
                    Close();
                    break;
            }
        }

        private void AnnounceMenu(string key)
        {
            if (key == "failed.extramoves")
            {
                int price = GameProgress.GetBoosterPrice(BoosterType.ExtraMoves);
                Speech.SpeakInterrupt(string.Format(Localization.Get(key), price));
            }
            else
            {
                Speech.SpeakInterrupt(Localization.Get(key));
            }
        }

        private void HandleLevelMapKeys(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.W:
                    if (_mapIndex > 0) { _mapIndex--; AnnounceLevel(_mapIndex); Invalidate(); }
                    break;
                case Keys.Down:
                case Keys.S:
                    if (_mapIndex < _progress.CurrentLevel) { _mapIndex++; AnnounceLevel(_mapIndex); Invalidate(); }
                    break;
                case Keys.Enter:
                case Keys.Space:
                    if (_progress.IsUnlocked(_mapIndex + 1))
                    {
                        SoundEngine.PlaySound("button");
                        _boosterRow = Boosters.All.Length;
                        SwitchScreen(GameScreen.Boosters);
                        AnnounceBoosterRow();
                    }
                    else
                    {
                        SoundEngine.PlaySound("invalid");
                        Speech.Speak(Localization.Get("menu.level") + " " + (_mapIndex + 1) + ". " + Localization.Get("msg.no"));
                    }
                    break;
                case Keys.V:
                    AnnounceLivesRealtime();
                    break;
                case Keys.Escape:
                case Keys.Back:
                    SwitchScreen(GameScreen.MainMenu);
                    break;
            }
        }

        private void AnnounceLevel(int index)
        {
            int n = index + 1;
            LevelDefinition l = Levels.Get(n);
            string stars = "0";
            int best;
            if (_progress.BestStars.TryGetValue(n, out best)) stars = best.ToString();
            string epText = "";
            EpisodeDefinition ep = Episodes.GetForLevel(n);
            if (n == ep.StartLevel)
            {
                epText = Localization.Get("episode") + " " + ep.Number + ": " + ep.Name + ". ";
            }
            if (_progress.IsUnlocked(n))
            {
                Speech.SpeakInterrupt(epText + Localization.Get("menu.level") + " " + n + ". " + l.ObjectiveText + ". " + Localization.Get("complete.stars") + " " + stars);
            }
            else
            {
                Speech.SpeakInterrupt(epText + Localization.Get("menu.level") + " " + n + ". " + Localization.Get("no"));
            }
        }

        private List<BoosterType> GetAvailableBoosters()
        {
            List<BoosterType> available = new List<BoosterType>();
            foreach (BoosterType t in Boosters.All)
            {
                if (_progress.IsBoosterUnlocked(t))
                {
                    available.Add(t);
                }
            }
            return available;
        }

        private void HandleBoostersKeys(KeyEventArgs e)
        {
            List<BoosterType> available = GetAvailableBoosters();
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.W:
                    _boosterRow = (_boosterRow + available.Count) % (available.Count + 1);
                    AnnounceBoosterRow();
                    Invalidate();
                    break;
                case Keys.Down:
                case Keys.S:
                    _boosterRow = (_boosterRow + 1) % (available.Count + 1);
                    AnnounceBoosterRow();
                    Invalidate();
                    break;
                case Keys.Enter:
                case Keys.Space:
                    if (_boosterRow < available.Count)
                    {
                        ToggleBoosterSelection(available[_boosterRow]);
                    }
                    else
                    {
                        StartLevelFromBoosters();
                    }
                    break;
                case Keys.Escape:
                case Keys.Back:
                    SwitchScreen(GameScreen.LevelMap);
                    AnnounceLevel(_mapIndex);
                    break;
            }
        }

        private void AnnounceBoosterRow()
        {
            List<BoosterType> available = GetAvailableBoosters();
            if (_boosterRow < available.Count)
            {
                BoosterType t = available[_boosterRow];
                string sel = _boosterSelection.Contains(t) ? Localization.Get("selected") : Localization.Get("none");
                Speech.SpeakInterrupt(Boosters.Name(t) + ". " + string.Format(Localization.Get("booster.count"), _progress.GetBooster(t)) + ". " + sel);
            }
            else
            {
                Speech.SpeakInterrupt(Localization.Get("booster.play"));
            }
        }

        private void ToggleBoosterSelection(BoosterType type)
        {
            if (_progress.GetBooster(type) <= 0)
            {
                SoundEngine.PlaySound("invalid");
                Speech.Speak(string.Format(Localization.Get("booster.none"), Boosters.Name(type)));
                return;
            }
            if (_boosterSelection.Contains(type))
            {
                _boosterSelection.Remove(type);
                SoundEngine.PlaySound("button_release");
                Speech.Speak(string.Format(Localization.Get("booster.selected"), _boosterSelection.Count));
            }
            else
            {
                if (_boosterSelection.Count >= 3)
                {
                    SoundEngine.PlaySound("invalid");
                    Speech.Speak(Localization.Get("booster.max"));
                    return;
                }
                _boosterSelection.Add(type);
                SoundEngine.PlaySound("button");
                Speech.Speak(Boosters.Name(type) + ". " + string.Format(Localization.Get("booster.selected"), _boosterSelection.Count));
            }
            Invalidate();
        }

        private bool CanStartAttempt()
        {
            _progress.UpdateLives();
            if (_progress.Lives <= 0)
            {
                double secs = _progress.NextLifeInSeconds();
                SoundEngine.PlaySound("invalid");
                Speech.Speak(string.Format(Localization.Get("msg.no.lives"), (int)Math.Ceiling(secs / 60.0)));
                return false;
            }
            return true;
        }

        private void StartLevelFromBoosters()
        {
            _progress.UpdateLives();
            if (_progress.Lives <= 0)
            {
                double secs = _progress.NextLifeInSeconds();
                SoundEngine.PlaySound("invalid");
                Speech.Speak(string.Format(Localization.Get("msg.no.lives"), (int)Math.Ceiling(secs / 60.0)));
                return;
            }
            SoundEngine.PlaySound("button");
            foreach (BoosterType t in _boosterSelection)
            {
                _progress.UseBooster(t);
            }
            _progress.Save();
            List<BoosterType> selected = new List<BoosterType>(_boosterSelection);
            _boosterSelection.Clear();
            StartLevel(_mapIndex + 1, selected);
        }

        private void AnnounceShop()
        {
            string balances = string.Format(Localization.Get("shop.gold"), _progress.GoldBars, _progress.Coins);
            if (_shopIndex < 5)
            {
                string itemKey = ShopItems[_shopIndex];
                BoosterType type = GetBoosterTypeFromShopIndex(_shopIndex);
                bool unlocked = _progress.IsBoosterUnlocked(type);
                int price = GameProgress.GetBoosterPrice(type);
                int count = _progress.GetBooster(type);
                string status = unlocked ? (count > 0 ? string.Format(Localization.Get("shop.owned.count"), count) + ". " + string.Format(Localization.Get("shop.price"), price) : string.Format(Localization.Get("shop.price"), price)) : Localization.Get("shop.locked");
                Speech.SpeakInterrupt(Localization.Get(itemKey) + ". " + status + ". " + balances);
            }
            else if (_shopIndex >= 5 && _shopIndex <= 7)
            {
                string itemKey = ShopItems[_shopIndex];
                Speech.SpeakInterrupt(Localization.Get(itemKey) + ". " + balances);
            }
            else
            {
                double remaining = _progress.DailyBonusTimeRemaining();
                if (remaining <= 0)
                {
                    Speech.SpeakInterrupt(Localization.Get("shop.daily") + ". " + Localization.Get("shop.collect") + ". " + balances);
                }
                else
                {
                    TimeSpan t = TimeSpan.FromSeconds(remaining);
                    string waitText = string.Format(Localization.Get("shop.daily.wait"), (int)t.TotalHours, t.Minutes);
                    Speech.SpeakInterrupt(Localization.Get("shop.daily") + ". " + waitText + ". " + balances);
                }
            }
        }

        private BoosterType GetBoosterTypeFromShopIndex(int index)
        {
            switch (index)
            {
                case 0: return BoosterType.LollipopHammer;
                case 1: return BoosterType.ExtraMoves;
                case 2: return BoosterType.JellyFish;
                case 3: return BoosterType.ColorBomb;
                case 4: return BoosterType.ExtraTime;
                default: return BoosterType.LollipopHammer;
            }
        }

        private void HandleShopKeys(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.W:
                    _shopIndex = (_shopIndex + ShopItems.Length - 1) % ShopItems.Length;
                    AnnounceShop();
                    Invalidate();
                    break;
                case Keys.Down:
                case Keys.S:
                    _shopIndex = (_shopIndex + 1) % ShopItems.Length;
                    AnnounceShop();
                    Invalidate();
                    break;
                case Keys.Enter:
                case Keys.Space:
                    if (_shopIndex < 5)
                    {
                        BoosterType type = GetBoosterTypeFromShopIndex(_shopIndex);
                        if (!_progress.IsBoosterUnlocked(type))
                        {
                            SoundEngine.PlaySound("shop_error");
                            Speech.Speak(Localization.Get("shop.locked"));
                            return;
                        }
                        int price = GameProgress.GetBoosterPrice(type);
                        if (_progress.SpendGoldBars(price))
                        {
                            _progress.AddBooster(type, 1);
                            _progress.Save();
                            SoundEngine.PlaySound("shop_buy");
                            Speech.Speak(Localization.Get(ShopItems[_shopIndex]) + ". " + Localization.Get("shop.purchased") + ". " + string.Format(Localization.Get("shop.gold"), _progress.GoldBars, _progress.Coins));
                        }
                        else
                        {
                            SoundEngine.PlaySound("shop_error");
                            Speech.Speak(string.Format(Localization.Get("shop.notenough"), price));
                        }
                    }
                    else if (_shopIndex == 5)
                    {
                        if (_progress.BuyGoldPackage(10, 100))
                        {
                            SoundEngine.PlaySound("shop_buy");
                            Speech.Speak(Localization.Get("shop.purchased") + ". +10 " + string.Format(Localization.Get("shop.gold"), _progress.GoldBars, _progress.Coins));
                        }
                        else
                        {
                            SoundEngine.PlaySound("shop_error");
                            Speech.Speak(Localization.Get("shop.coins.notenough"));
                        }
                    }
                    else if (_shopIndex == 6)
                    {
                        if (_progress.BuyGoldPackage(30, 250))
                        {
                            SoundEngine.PlaySound("shop_buy");
                            Speech.Speak(Localization.Get("shop.purchased") + ". +30 " + string.Format(Localization.Get("shop.gold"), _progress.GoldBars, _progress.Coins));
                        }
                        else
                        {
                            SoundEngine.PlaySound("shop_error");
                            Speech.Speak(Localization.Get("shop.coins.notenough"));
                        }
                    }
                    else if (_shopIndex == 7)
                    {
                        if (_progress.BuyGoldPackage(70, 500))
                        {
                            SoundEngine.PlaySound("shop_buy");
                            Speech.Speak(Localization.Get("shop.purchased") + ". +70 " + string.Format(Localization.Get("shop.gold"), _progress.GoldBars, _progress.Coins));
                        }
                        else
                        {
                            SoundEngine.PlaySound("shop_error");
                            Speech.Speak(Localization.Get("shop.coins.notenough"));
                        }
                    }
                    else
                    {
                        if (_progress.TryCollectDailyBonus())
                        {
                            SoundEngine.PlaySound("daily_bonus");
                            Speech.Speak(Localization.Get("shop.daily.collected") + ". " + string.Format(Localization.Get("shop.gold"), _progress.GoldBars, _progress.Coins));
                        }
                        else
                        {
                            SoundEngine.PlaySound("shop_error");
                            double remaining = _progress.DailyBonusTimeRemaining();
                            TimeSpan t = TimeSpan.FromSeconds(remaining);
                            string waitText = string.Format(Localization.Get("shop.daily.wait"), (int)t.TotalHours, t.Minutes);
                            Speech.Speak(waitText);
                        }
                    }
                    Invalidate();
                    break;
                case Keys.Escape:
                case Keys.Back:
                    SwitchScreen(GameScreen.MainMenu);
                    AnnounceMenu(MainMenuItems[_menuIndex]);
                    break;
            }
        }

        private void DrawShop(Graphics g)
        {
            g.DrawString(Localization.Get("shop.title"), new Font(Font.FontFamily, 18), Brushes.Gold, 40, 20);
            g.DrawString(string.Format(Localization.Get("shop.gold"), _progress.GoldBars, _progress.Coins), new Font(Font.FontFamily, 13, FontStyle.Bold), Brushes.Gold, 40, 48);
            int y = 78;
            for (int i = 0; i < ShopItems.Length; i++)
            {
                string text = (i == _shopIndex ? "> " : "  ") + Localization.Get(ShopItems[i]);
                if (i < 5)
                {
                    BoosterType type = GetBoosterTypeFromShopIndex(i);
                    bool unlocked = _progress.IsBoosterUnlocked(type);
                    int price = GameProgress.GetBoosterPrice(type);
                    int count = _progress.GetBooster(type);
                    if (unlocked)
                    {
                        text += "  " + string.Format(Localization.Get("shop.price"), price);
                        if (count > 0) text += "  (" + string.Format(Localization.Get("shop.owned.count"), count) + ")";
                    }
                    else
                    {
                        text += "  " + Localization.Get("shop.locked") + " (" + string.Format(Localization.Get("shop.unlock.at"), GameProgress.GetBoosterUnlockLevel(type)) + ")";
                    }
                }
                else if (i == 8)
                {
                    double remaining = _progress.DailyBonusTimeRemaining();
                    if (remaining <= 0)
                    {
                        text += "  " + Localization.Get("shop.collect");
                    }
                    else
                    {
                        TimeSpan t = TimeSpan.FromSeconds(remaining);
                        string waitText = string.Format(Localization.Get("shop.daily.wait"), (int)t.TotalHours, t.Minutes);
                        text += "  " + waitText;
                    }
                }
                Brush b = i == _shopIndex ? Brushes.Gold : Brushes.White;
                Font f = i == _shopIndex ? new Font(Font.FontFamily, 12, FontStyle.Bold) : new Font(Font.FontFamily, 12);
                g.DrawString(text, f, b, 40, y);
                y += 32;
            }
        }

        private void AnnounceLivesRealtime()
        {
            _progress.UpdateLives();
            if (_progress.Lives >= 5)
            {
                Speech.SpeakInterrupt(string.Format(Localization.Get("lives.max"), _progress.Lives));
            }
            else
            {
                TimeSpan t = TimeSpan.FromSeconds(_progress.NextLifeInSeconds());
                Speech.SpeakInterrupt(string.Format(Localization.Get("lives.next"), _progress.Lives, t.Minutes, t.Seconds));
            }
        }

        private void HandlePlayingKeys(KeyEventArgs e)
        {
            if (_board == null || _board.Completed || _board.Failed) return;

            if (_boosterPanelActive)
            {
                switch (e.KeyCode)
                {
                    case Keys.Up:
                    case Keys.W:
                        _boosterPanelIndex = (_boosterPanelIndex + _ownedBoosters.Count - 1) % _ownedBoosters.Count;
                        AnnounceBoosterPanelItem();
                        Invalidate();
                        return;
                    case Keys.Down:
                    case Keys.S:
                        _boosterPanelIndex = (_boosterPanelIndex + 1) % _ownedBoosters.Count;
                        AnnounceBoosterPanelItem();
                        Invalidate();
                        return;
                    case Keys.Tab:
                    case Keys.Escape:
                        _boosterPanelActive = false;
                        SoundEngine.PlaySound("button_release");
                        Speech.SpeakInterrupt(Localization.Get("booster.panel.closed"));
                        Invalidate();
                        return;
                    case Keys.Enter:
                    case Keys.Space:
                        ExecuteTacticalBooster();
                        return;
                }
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.Tab:
                    OpenTacticalBoosterPanel();
                    break;
                case Keys.V:
                    AnnounceLivesRealtime();
                    break;
                case Keys.Left:
                    MoveCursor(-1, 0);
                    break;
                case Keys.Right:
                    MoveCursor(1, 0);
                    break;
                case Keys.Up:
                    MoveCursor(0, -1);
                    break;
                case Keys.Down:
                    MoveCursor(0, 1);
                    break;
                case Keys.A:
                    DoSwap(-1, 0);
                    break;
                case Keys.D:
                    DoSwap(1, 0);
                    break;
                case Keys.W:
                    DoSwap(0, -1);
                    break;
                case Keys.S:
                    DoSwap(0, 1);
                    break;
                case Keys.Enter:
                case Keys.Space:
                    ToggleSelect();
                    break;
                case Keys.C:
                    AnnounceCursorCell();
                    break;
                case Keys.R:
                    Speech.Speak(_board.StatusText());
                    break;
                case Keys.B:
                    Speech.SpeakInterrupt(_board.DescribeBoard());
                    break;
                case Keys.T:
                    Speech.SpeakInterrupt(string.Format(Localization.Get("row.read"), _cursorY + 1, _board.DescribeRow(_cursorY)));
                    break;
                case Keys.G:
                    Speech.SpeakInterrupt(string.Format(Localization.Get("column.read"), Board.ColLetter(_cursorX), _board.DescribeColumn(_cursorX)));
                    break;
                case Keys.H:
                    ShowHint();
                    break;
                case Keys.L:
                    UseHammer();
                    break;
                case Keys.F1:
                    ShowToffeeTip();
                    break;
                case Keys.D1:
                    Speech.Speak(_board.StatusText());
                    break;
                case Keys.D2:
                    AnnounceSpecials();
                    break;
                case Keys.P:
                case Keys.Escape:
                    _pauseIndex = 0;
                    SwitchScreen(GameScreen.Pause);
                    AnnounceMenu(PauseItems[0]);
                    break;
            }
        }

        private void OpenTacticalBoosterPanel()
        {
            _ownedBoosters.Clear();
            foreach (BoosterType t in Boosters.All)
            {
                if (_progress.GetBooster(t) > 0)
                {
                    _ownedBoosters.Add(t);
                }
            }
            if (_ownedBoosters.Count > 0)
            {
                _boosterPanelActive = true;
                _boosterPanelIndex = 0;
                SoundEngine.PlaySound("swoop_in");
                Speech.SpeakInterrupt(string.Format(Localization.Get("booster.panel.open"), Board.CellName(_cursorX, _cursorY)) + ". " + GetBoosterPanelItemText(_boosterPanelIndex));
                Invalidate();
            }
            else
            {
                SoundEngine.PlaySound("invalid");
                Speech.Speak(Localization.Get("booster.panel.empty"));
            }
        }

        private string GetBoosterPanelItemText(int index)
        {
            if (index < 0 || index >= _ownedBoosters.Count) return "";
            BoosterType t = _ownedBoosters[index];
            int count = _progress.GetBooster(t);
            return Boosters.Name(t) + " (" + string.Format(Localization.Get("booster.count"), count) + "). " + Boosters.Description(t);
        }

        private void AnnounceBoosterPanelItem()
        {
            SoundEngine.PlaySound("button");
            Speech.SpeakInterrupt(GetBoosterPanelItemText(_boosterPanelIndex));
        }

        private void ExecuteTacticalBooster()
        {
            if (_boosterPanelIndex < 0 || _boosterPanelIndex >= _ownedBoosters.Count) return;
            BoosterType t = _ownedBoosters[_boosterPanelIndex];
            if (_progress.GetBooster(t) <= 0)
            {
                SoundEngine.PlaySound("invalid");
                Speech.Speak(Localization.Get("booster.panel.empty"));
                _boosterPanelActive = false;
                Invalidate();
                return;
            }

            switch (t)
            {
                case BoosterType.LollipopHammer:
                    _boosterPanelActive = false;
                    UseHammer();
                    break;
                case BoosterType.ColorBomb:
                    if (_board.PlaceSpecialAt(_cursorX, _cursorY, SpecialType.ColorBomb))
                    {
                        _progress.UseBooster(t);
                        _progress.Save();
                        _boosterPanelActive = false;
                        SoundEngine.PlaySound("colorbomb_created");
                        Speech.SpeakInterrupt(string.Format(Localization.Get("booster.colorbomb.placed"), Board.CellName(_cursorX, _cursorY)) + ". " + _board.DescribeCell(_cursorX, _cursorY));
                        CheckGameOver();
                        Invalidate();
                    }
                    else
                    {
                        SoundEngine.PlaySound("invalid");
                        Speech.Speak(Localization.Get("msg.invalid"));
                    }
                    break;
                case BoosterType.JellyFish:
                    if (_board.PlaceSpecialAt(_cursorX, _cursorY, SpecialType.Fish))
                    {
                        _progress.UseBooster(t);
                        _progress.Save();
                        _boosterPanelActive = false;
                        SoundEngine.PlaySound("fish");
                        Speech.SpeakInterrupt(string.Format(Localization.Get("booster.jellyfish.placed"), Board.CellName(_cursorX, _cursorY)) + ". " + _board.DescribeCell(_cursorX, _cursorY));
                        CheckGameOver();
                        Invalidate();
                    }
                    else
                    {
                        SoundEngine.PlaySound("invalid");
                        Speech.Speak(Localization.Get("msg.invalid"));
                    }
                    break;
                case BoosterType.ExtraMoves:
                    _progress.UseBooster(t);
                    _progress.Save();
                    _boosterPanelActive = false;
                    _board.AddMoves(5);
                    SoundEngine.PlaySound("klubb");
                    Speech.SpeakInterrupt(Localization.Get("booster.plus.moves") + ". " + string.Format(Localization.Get("moves.count"), _board.MovesLeft));
                    Invalidate();
                    break;
                case BoosterType.ExtraTime:
                    if (_board.Level.Type == LevelType.Timed)
                    {
                        _progress.UseBooster(t);
                        _progress.Save();
                        _boosterPanelActive = false;
                        _board.AddTime(15);
                        SoundEngine.PlaySound("time_warning");
                        Speech.SpeakInterrupt(Localization.Get("booster.plus.time") + ". " + string.Format(Localization.Get("time.count"), (int)Math.Ceiling(_board.TimeLeft)) + "s");
                        Invalidate();
                    }
                    else
                    {
                        SoundEngine.PlaySound("invalid");
                        Speech.Speak(Localization.Get("msg.invalid"));
                    }
                    break;
            }
        }

        private void MoveCursor(int dx, int dy)
        {
            int nx = _cursorX + dx;
            int ny = _cursorY + dy;
            if (nx < 0 || nx >= Board.Cols || ny < 0 || ny >= Board.Rows) return;
            _cursorX = nx;
            _cursorY = ny;
            AnnounceCursorCell();
            Invalidate();
        }

        private void AnnounceCursorCell()
        {
            Candy c = _board.GetCandy(_cursorX, _cursorY);
            if (c != null && !c.IsIngredient && c.Special == SpecialType.None)
            {
                SoundEngine.PlayCandySound(_cursorX, _cursorY, c.Color);
            }
            string cell = _board.DescribeCell(_cursorX, _cursorY);
            string move = FindValidMoveFrom(_cursorX, _cursorY);
            if (!string.IsNullOrEmpty(move)) cell += ". " + move;
            Speech.SpeakInterrupt(cell);
        }

        private string FindValidMoveFrom(int x, int y)
        {
            int[,] dirs = { { 0, -1 }, { 0, 1 }, { -1, 0 }, { 1, 0 } };
            string[] dirKeys = { "dir.up", "dir.down", "dir.left", "dir.right" };
            List<string> moves = new List<string>();

            for (int i = 0; i < 4; i++)
            {
                int nx = x + dirs[i, 0];
                int ny = y + dirs[i, 1];
                if (nx < 0 || nx >= Board.Cols || ny < 0 || ny >= Board.Rows) continue;
                if (_board.IsValidMove(x, y, nx, ny))
                {
                    string dirStr = Localization.Get(dirKeys[i]);
                    string targetCell = Board.CellName(nx, ny);
                    moves.Add(string.Format(Localization.Get("msg.move.available.dir"), dirStr, targetCell));
                }
            }
            if (moves.Count == 0) return "";
            return string.Join(", ", moves);
        }

        private void ToggleSelect()
        {
            Candy c = _board.GetCandy(_cursorX, _cursorY);
            if (c == null || c.IsIngredient || c.IsLicorice)
            {
                SoundEngine.PlaySound("invalid");
                Speech.Speak(Localization.Get("msg.invalid"));
                return;
            }
            if (_selectedX == -1)
            {
                _selectedX = _cursorX;
                _selectedY = _cursorY;
                SoundEngine.PlayCandySound(_cursorX, _cursorY, c.Color);
                Speech.Speak(string.Format(Localization.Get("msg.selected"), _board.DescribeCell(_cursorX, _cursorY)));
            }
            else
            {
                if (Math.Abs(_selectedX - _cursorX) + Math.Abs(_selectedY - _cursorY) == 1)
                {
                    int sx = _selectedX;
                    int sy = _selectedY;
                    ClearSelection();
                    DoSwapAt(sx, sy, _cursorX, _cursorY);
                }
                else
                {
                    _selectedX = _cursorX;
                    _selectedY = _cursorY;
                    SoundEngine.PlayCandySound(_cursorX, _cursorY, c.Color);
                    Speech.Speak(string.Format(Localization.Get("msg.selected"), _board.DescribeCell(_cursorX, _cursorY)));
                }
            }
            Invalidate();
        }

        private void ClearSelection()
        {
            _selectedX = -1;
            _selectedY = -1;
        }

        private void DoSwap(int dx, int dy)
        {
            int nx = _cursorX + dx;
            int ny = _cursorY + dy;
            if (nx < 0 || nx >= Board.Cols || ny < 0 || ny >= Board.Rows) return;
            DoSwapAt(_cursorX, _cursorY, nx, ny);
        }

        private void DoSwapAt(int x1, int y1, int x2, int y2)
        {
            ClearSelection();
            TurnResult r = _board.ProcessTurn(x1, y1, x2, y2);
            _cursorX = x2;
            _cursorY = y2;
            HandleTurnResult(r, x1);
            Invalidate();
        }

        private void HandleTurnResult(TurnResult r, int panCol = -1)
        {
            if (!r.Valid)
            {
                SoundEngine.PlaySound("invalid");
                Speech.Speak(Localization.Get("msg.invalid"));
                return;
            }

            SoundEngine.PlaySound("switch", panCol);
            if (r.CascadeLevels >= 1)
            {
                if (r.CascadeLevels > 1) SoundEngine.PlayMatchSequence(r.CascadeLevels, panCol);
                else SoundEngine.PlayMatchSound(1, panCol);
            }

            foreach (SpecialType sp in r.SpecialsCreated)
            {
                switch (sp)
                {
                    case SpecialType.Striped: SoundEngine.PlaySound("striped_created", panCol, _cursorY); break;
                    case SpecialType.Wrapped: SoundEngine.PlaySound("wrapped_created", panCol, _cursorY); break;
                    case SpecialType.ColorBomb: SoundEngine.PlaySound("colorbomb_created", panCol, _cursorY); break;
                    case SpecialType.Fish: SoundEngine.PlaySound("fish", panCol, _cursorY); break;
                }
            }
            foreach (SpecialType sp in r.SpecialsActivated)
            {
                switch (sp)
                {
                    case SpecialType.Striped: SoundEngine.PlayLineBlastSweep(panCol >= 0 ? panCol : _cursorX, _cursorY, true); break;
                    case SpecialType.Wrapped: SoundEngine.PlayWrappedExplosion(panCol >= 0 ? panCol : _cursorX, _cursorY); break;
                    case SpecialType.ColorBomb: SoundEngine.PlayColorBombSweep(panCol >= 0 ? panCol : _cursorX, _cursorY); break;
                    case SpecialType.Fish: 
                        SoundEngine.PlaySound("fish_eating", panCol >= 0 ? panCol : _cursorX, _cursorY);
                        SoundEngine.PlaySound("fish_bite", panCol >= 0 ? panCol : _cursorX, _cursorY);
                        break;
                }
            }
            if (r.LicoriceBroken > 0) SoundEngine.PlaySound("licorice", panCol, _cursorY);
            if (r.JellyCleared > 0) SoundEngine.PlaySound("jelly", panCol, _cursorY);
            if (r.ChocolateDestroyed > 0) SoundEngine.PlaySound("chocolate_removed", panCol, _cursorY);
            if (r.FrostingBroken > 0) SoundEngine.PlaySound(_board.IsDoubleFrosting(_cursorX, _cursorY) ? "frosting2" : "frosting1", panCol, _cursorY);
            if (r.IngredientsCollected > 0) SoundEngine.PlaySound("ingredient", panCol, _cursorY);
            if (r.BombExploded > 0) SoundEngine.PlaySound("bomb", panCol, _cursorY);
            if (r.TimeGained > 0)
            {
                _board.AddTime(r.TimeGained);
                SoundEngine.PlaySound("time_warning");
            }

            List<string> parts = new List<string>();
            if (r.Events.Count > 0)
            {
                parts.Add(string.Join(". ", r.Events));
            }
            if (r.ScoreGained > 0)
            {
                parts.Add(string.Format("{0}: {1}", Localization.Get("score"), r.ScoreGained));
            }
            if (r.CascadeLevels > 1)
            {
                parts.Add(string.Format(Localization.Get("msg.cascade"), r.CascadeLevels));
                string voice = PickAffirmation(r.CascadeLevels);
                SoundEngine.PlayVoice(voice);
            }
            if (r.TimeGained > 0)
            {
                parts.Add(Localization.Get("msg.time5"));
            }
            if (_board.Level.Type == LevelType.Jelly)
            {
                parts.Add(string.Format(Localization.Get("jelly.remaining"), _board.RemainingJelly));
            }
            if (_board.Level.Type == LevelType.Order)
            {
                parts.Add(string.Format(Localization.Get("order.remaining"), _board.OrdersRemaining));
            }
            if (_board.Level.Type == LevelType.Ingredient && r.IngredientsCollected > 0)
            {
                parts.Add(string.Format(Localization.Get("ingredients"), _board.IngredientsRemaining));
            }

            Speech.Speak(string.Join(". ", parts));
            CheckFrostingCleared();
            CheckGameOver();
        }

        private void CheckFrostingCleared()
        {
            if (_board == null) return;
            if (_board.Level.TargetFrosting <= 0) return;
            if (_frostingClearedAnnounced) return;
            if (_board.FrostingRemaining == 0)
            {
                _frostingClearedAnnounced = true;
                SoundEngine.PlaySound("fanfare");
                Speech.Speak(Localization.Get("msg.frosting.clear"));
            }
        }

        private string PickAffirmation(int cascade)
        {
            string[] keys = { "sweet", "tasty", "delicious", "divine" };
            int idx = Math.Min(cascade - 2, keys.Length - 1);
            return keys[idx];
        }

        private void CheckGameOver()
        {
            if (_board.Completed)
            {
                HandleWin();
                return;
            }
            if (_board.Failed)
            {
                HandleLose();
                return;
            }
            if (!_board.HasValidMoves())
            {
                _board.Reshuffle();
                SoundEngine.PlaySound("swoosh");
                Speech.Speak(Localization.Get("msg.no.move"));
                SoundEngine.PlaySound("tasty");
                Speech.Speak(Localization.Get("toffee.tip.5"));
            }
        }

        private void HandleWin()
        {
            SoundEngine.PlaySound("win");
            SoundEngine.PlayMusic(MusicTrack.Win);
            int moves = _board.MovesLeft;
            if (moves > 0)
            {
                TurnResult sc = _board.SugarCrush(moves);
                if (sc.SugarCrushMoves > 0 || sc.ActivationsDetailed.Count > 0)
                {
                    SoundEngine.PlaySugarCrushSequence(sc);
                }
            }
            int stars = _board.StarsEarned;
            _progress.RecordResult(_levelNumber, _board.Score, stars);
            _progress.AwardLevelCompletion(stars);
            if (stars > 0) SoundEngine.PlayStarSequence(stars);

            string awarded = _progress.AwardBoosters(stars);
            if (!string.IsNullOrEmpty(awarded))
            {
                _progress.Save();
                Speech.Speak(Localization.Get("booster.awarded") + " " + awarded);
            }

            Speech.SpeakInterrupt(Localization.Get("msg.win") + ". " +
                string.Format(Localization.Get("complete.score"), _board.Score) + ". " +
                string.Format(Localization.Get("complete.stars"), Localization.StarLabel(stars)) + ". " +
                string.Format(Localization.Get("shop.gold"), _progress.GoldBars, _progress.Coins));

            EpisodeDefinition ep = Episodes.GetForLevel(_levelNumber);
            if (Episodes.IsEndLevel(_levelNumber))
            {
                EpisodeDefinition next = Episodes.Next(ep);
                if (next != null)
                {
                    SoundEngine.PlaySound("fanfare");
                    Speech.Speak(string.Format(Localization.Get("episode.complete"), next.Name));
                }
            }
            if (!_progress.IsUnlocked(_levelNumber + 1))
            {
                SoundEngine.PlaySound("unlock");
            }
            SwitchScreen(GameScreen.Complete);
        }

        private void HandleLose()
        {
            SoundEngine.PlaySound("lose");
            SoundEngine.PlayMusic(MusicTrack.Lose);
            _pauseIndex = 0;
            string msg = _board.Level.Type == LevelType.Timed
                ? Localization.Get("msg.lose.time")
                : Localization.Get("msg.lose");
            _progress.UpdateLives();
            _progress.TryConsumeLife();
            _progress.Save();
            if (_progress.Lives <= 0)
            {
                double secs = _progress.NextLifeInSeconds();
                msg += ". " + string.Format(Localization.Get("msg.no.lives"), (int)Math.Ceiling(secs / 60.0));
            }
            else
            {
                msg += ". " + string.Format(Localization.Get("lives.lost"), _progress.Lives);
            }
            Speech.SpeakInterrupt(msg);
            SwitchScreen(GameScreen.Failed);
        }

        private int _tipIndex = -1;
        private int _levelVoiceIndex;
        private bool _frostingClearedAnnounced;
        private readonly string[] _tipClips = { "sweet", "tasty", "delicious", "divine" };

        private void ShowToffeeTip()
        {
            _tipIndex = (_tipIndex + 1) % 6;
            SoundEngine.PlaySound(_tipClips[_tipIndex % _tipClips.Length]);
            Speech.SpeakInterrupt(Localization.Get("toffee.tip." + (_tipIndex + 1)));
        }

        private void UseHammer()
        {
            if (_board == null) return;
            if (!_progress.UseBooster(BoosterType.LollipopHammer))
            {
                SoundEngine.PlaySound("invalid");
                Speech.Speak(string.Format(Localization.Get("booster.none"), Boosters.Name(BoosterType.LollipopHammer)));
                return;
            }
            _progress.Save();
            TurnResult r = _board.SmashCell(_cursorX, _cursorY);
            if (!r.Valid)
            {
                _progress.AddBooster(BoosterType.LollipopHammer, 1);
                _progress.Save();
                SoundEngine.PlaySound("invalid");
                Speech.Speak(Localization.Get("msg.invalid"));
                return;
            }
            SoundEngine.PlaySound("klubb");
            Speech.Speak(string.Format(Localization.Get("hammer.used"), Board.CellName(_cursorX, _cursorY)) + ". " +
                string.Format(Localization.Get("score"), r.ScoreGained));
            CheckFrostingCleared();
            CheckGameOver();
            Invalidate();
        }

        private void ShowHint()
        {
            int x1, y1, x2, y2;
            if (_board.GetHint(out x1, out y1, out x2, out y2))
            {
                SoundEngine.PlaySound("swoop_in");
                Speech.SpeakInterrupt(string.Format(Localization.Get("msg.hint"),
                    Board.CellName(x1, y1), Board.CellName(x2, y2)));
            }
            else
            {
                Speech.Speak(Localization.Get("msg.hint.none"));
            }
        }

        private void AnnounceSpecials()
        {
            List<string> list = _board.GetSpecialPositions();
            if (list.Count == 0)
            {
                Speech.Speak(Localization.Get("none"));
            }
            else
            {
                Speech.Speak(string.Join(". ", list));
            }
        }

        private void StartLevel(int levelNumber, List<BoosterType> boosters = null, int extraMoves = 0)
        {
            if (!CanStartAttempt())
            {
                _mapIndex = Math.Max(0, levelNumber - 1);
                SwitchScreen(GameScreen.LevelMap);
                AnnounceLevel(_mapIndex);
                return;
            }
            _levelNumber = levelNumber;
            _board = new Board(Levels.Get(levelNumber));
            if (extraMoves > 0)
            {
                _board.AddMoves(extraMoves);
            }
            _cursorX = Board.Cols / 2 - 1;
            _cursorY = Board.Rows / 2 - 1;
            _frostingClearedAnnounced = false;
            ClearSelection();
            SwitchScreen(GameScreen.Playing);
            SoundEngine.PlayEpisodeMusic(Episodes.GetForLevel(levelNumber).Number);
            SoundEngine.PlayBinauralAmbientShimmer();

            List<string> applied = new List<string>();
            if (boosters != null && boosters.Count > 0)
            {
                string msg = _board.ApplyStartBoosters(boosters);
                if (!string.IsNullOrEmpty(msg)) applied.Add(msg);
                SoundEngine.PlaySound("klubb");
            }

            EpisodeDefinition ep = Episodes.GetForLevel(levelNumber);
            if (ep.Number > _progress.CurrentEpisode)
            {
                _progress.CurrentEpisode = ep.Number;
                _progress.Save();
                SoundEngine.PlaySound("fanfare");
                string introKey = ep.Number <= 3 ? "episode.intro." + ep.Number : "episode.intro.new";
                Speech.SpeakInterrupt(Localization.Get(introKey) + " " + Localization.Get("episode") + " " + ep.Number + ": " + ep.Name);
            }
            else
            {
                string msg = Localization.Get("menu.level") + " " + levelNumber + ". " + _board.Level.ObjectiveText + ". " +
                    string.Format(Localization.Get("lives.count"), _progress.Lives);
                if (!_progress.BestStars.ContainsKey(levelNumber))
                {
                    _levelVoiceIndex++;
                    SoundEngine.PlaySound(_tipClips[_levelVoiceIndex % _tipClips.Length]);
                    msg = Localization.Get("toffee.level." + ((_levelVoiceIndex % 4) + 1)) + " " + msg;
                }
                if (applied.Count > 0)
                {
                    msg += ". " + string.Format(Localization.Get("booster.applied"), string.Join(", ", applied));
                }
                if (extraMoves > 0)
                {
                    msg += ". " + string.Format(Localization.Get("extra.moves.purchased"), extraMoves);
                }
                Speech.SpeakInterrupt(msg);
            }
        }

        private void ContinueLevelWithExtraMoves(int extraMoves)
        {
            if (_board != null)
            {
                _board.AddMoves(extraMoves);
            }
            SwitchScreen(GameScreen.Playing);
            SoundEngine.PlayEpisodeMusic(Episodes.GetForLevel(_levelNumber).Number);
            SoundEngine.PlayBinauralAmbientShimmer();
            string msg = string.Format(Localization.Get("extra.moves.purchased"), extraMoves) + ". " +
                string.Format(Localization.Get("moves.count"), _board != null ? _board.MovesLeft : extraMoves);
            Speech.SpeakInterrupt(msg);
            AnnounceCursorCell();
        }

        private void HandleOptionsKeys(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.W:
                    _optionsIndex = (_optionsIndex + OptionsItems.Length - 1) % OptionsItems.Length;
                    AnnounceOptions();
                    Invalidate();
                    break;
                case Keys.Down:
                case Keys.S:
                    _optionsIndex = (_optionsIndex + 1) % OptionsItems.Length;
                    AnnounceOptions();
                    Invalidate();
                    break;
                case Keys.Left:
                    AdjustOption(-1);
                    break;
                case Keys.Right:
                    AdjustOption(1);
                    break;
                case Keys.Enter:
                case Keys.Space:
                    if (_optionsIndex == 0)
                    {
                        ToggleLanguage();
                    }
                    else if (_optionsIndex == 4)
                    {
                        ToggleBinauralAmbient();
                    }
                    else if (_optionsIndex == 5)
                    {
                        CheckForUpdatesManual();
                    }
                    break;
                case Keys.Escape:
                case Keys.Back:
                    _progress.Save();
                    SwitchScreen(GameScreen.MainMenu);
                    break;
            }
        }

        private async void CheckForUpdatesManual()
        {
            SoundEngine.PlaySound("button");
            Speech.Speak(Localization.Get("update.checking"));
            var update = await Updater.CheckForUpdatesAsync();
            if (update != null)
            {
                SwitchScreen(GameScreen.UpdateAvailable);
                Speech.SpeakInterrupt(string.Format(Localization.Get("update.available"), update.Version, update.ReleaseNotes));
            }
            else
            {
                Speech.Speak(Localization.Get("update.none"));
            }
        }

        private void ToggleLanguage()
        {
            Localization.Current = Localization.Current == Language.Spanish ? Language.English : Language.Spanish;
            _progress.LanguageSpanish = Localization.Current == Language.Spanish;
            _progress.Save();
            Text = Localization.Get("game.title");
            SoundEngine.PlaySound("button");
            Speech.Speak(string.Format(Localization.Get("options.language.value"), Localization.LanguageName(Localization.Current)));
            Invalidate();
        }

        private void ToggleBinauralAmbient()
        {
            _progress.BinauralAmbientEnabled = !_progress.BinauralAmbientEnabled;
            SoundEngine.BinauralAmbientEnabled = _progress.BinauralAmbientEnabled;
            _progress.Save();
            SoundEngine.PlaySound("button");
            string state = _progress.BinauralAmbientEnabled ? Localization.Get("yes") : Localization.Get("no");
            Speech.Speak(string.Format(Localization.Get("options.binaural.value"), state));
            Invalidate();
        }

        private void AdjustOption(int delta)
        {
            switch (_optionsIndex)
            {
                case 1:
                    float m = SoundEngine.MusicVolume;
                    m = Math.Max(0, Math.Min(1f, (float)Math.Round(m + delta * 0.05f, 2)));
                    SoundEngine.MusicVolume = m;
                    _progress.MusicVolume = m;
                    _progress.Save();
                    Speech.Speak(string.Format(Localization.Get("options.value"), Localization.Get(OptionsItems[1]), (int)Math.Round(m * 100)));
                    break;
                case 2:
                    float s = SoundEngine.SfxVolume;
                    s = Math.Max(0, Math.Min(1f, (float)Math.Round(s + delta * 0.05f, 2)));
                    SoundEngine.SfxVolume = s;
                    _progress.SfxVolume = s;
                    _progress.Save();
                    Speech.Speak(string.Format(Localization.Get("options.value"), Localization.Get(OptionsItems[2]), (int)Math.Round(s * 100)));
                    break;
                case 3:
                    float v = SoundEngine.VoiceVolume;
                    v = Math.Max(0, Math.Min(1f, (float)Math.Round(v + delta * 0.05f, 2)));
                    SoundEngine.VoiceVolume = v;
                    _progress.VoiceVolume = v;
                    _progress.Save();
                    Speech.Speak(string.Format(Localization.Get("options.value"), Localization.Get(OptionsItems[3]), (int)Math.Round(v * 100)));
                    break;
                case 4:
                    ToggleBinauralAmbient();
                    break;
            }
            Invalidate();
        }

        private void AnnounceOptions()
        {
            switch (_optionsIndex)
            {
                case 0:
                    Speech.SpeakInterrupt(string.Format(Localization.Get("options.language.value"), Localization.LanguageName(Localization.Current)));
                    break;
                case 1:
                    Speech.SpeakInterrupt(string.Format(Localization.Get("options.value"), Localization.Get(OptionsItems[1]), (int)(SoundEngine.MusicVolume * 100)));
                    break;
                case 2:
                    Speech.SpeakInterrupt(string.Format(Localization.Get("options.value"), Localization.Get(OptionsItems[2]), (int)(SoundEngine.SfxVolume * 100)));
                    break;
                case 3:
                    Speech.SpeakInterrupt(string.Format(Localization.Get("options.value"), Localization.Get(OptionsItems[3]), (int)(SoundEngine.VoiceVolume * 100)));
                    break;
                case 4:
                    string state = _progress.BinauralAmbientEnabled ? Localization.Get("yes") : Localization.Get("no");
                    Speech.SpeakInterrupt(string.Format(Localization.Get("options.binaural.value"), state));
                    break;
                case 5:
                    Speech.SpeakInterrupt(Localization.Get("options.update"));
                    break;
            }
        }

        private void HandleTutorialKeys(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Right:
                case Keys.Down:
                    if (_tutorialPage < 3) { _tutorialPage++; AnnounceTutorial(); Invalidate(); }
                    break;
                case Keys.Left:
                case Keys.Up:
                    if (_tutorialPage > 0) { _tutorialPage--; AnnounceTutorial(); Invalidate(); }
                    break;
                case Keys.D1:
                case Keys.NumPad1:
                    if (_tutorialPage == 2)
                    {
                        Speech.SpeakInterrupt(Localization.Get("special.striped"));
                        SoundEngine.PlayLineBlastSweep(3, 4, true);
                    }
                    break;
                case Keys.D2:
                case Keys.NumPad2:
                    if (_tutorialPage == 2)
                    {
                        Speech.SpeakInterrupt(Localization.Get("special.wrapped"));
                        SoundEngine.PlayWrappedExplosion(3, 4);
                    }
                    break;
                case Keys.D3:
                case Keys.NumPad3:
                    if (_tutorialPage == 2)
                    {
                        Speech.SpeakInterrupt(Localization.Get("special.colorbomb"));
                        SoundEngine.PlayColorBombSweep(3, 4);
                    }
                    break;
                case Keys.Escape:
                case Keys.Back:
                case Keys.Enter:
                    SwitchScreen(GameScreen.MainMenu);
                    break;
            }
        }

        private void AnnounceTutorial()
        {
            Speech.SpeakInterrupt(Localization.Get("tutorial.page" + (_tutorialPage + 1)));
        }

        private void HandlePauseKeys(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.W:
                    _pauseIndex = (_pauseIndex + PauseItems.Length - 1) % PauseItems.Length;
                    AnnounceMenu(PauseItems[_pauseIndex]);
                    Invalidate();
                    break;
                case Keys.Down:
                case Keys.S:
                    _pauseIndex = (_pauseIndex + 1) % PauseItems.Length;
                    AnnounceMenu(PauseItems[_pauseIndex]);
                    Invalidate();
                    break;
                case Keys.Enter:
                case Keys.Space:
                    SoundEngine.PlaySound("button");
                    switch (_pauseIndex)
                    {
                        case 0:
                            SwitchScreen(GameScreen.Playing);
                            AnnounceCursorCell();
                            break;
                        case 1:
                            StartLevel(_levelNumber);
                            break;
                        case 2:
                            _menuIndex = 0;
                            SwitchScreen(GameScreen.MainMenu);
                            SoundEngine.PlayMusic(MusicTrack.Menu);
                            break;
                    }
                    break;
                case Keys.Escape:
                    SwitchScreen(GameScreen.Playing);
                    AnnounceCursorCell();
                    break;
            }
        }

        private void HandleCompleteKeys(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                case Keys.Space:
                    SoundEngine.PlaySound("button");
                    StartLevel(_levelNumber + 1);
                    break;
                case Keys.Escape:
                case Keys.Back:
                    _mapIndex = Math.Max(0, _levelNumber - 1);
                    SwitchScreen(GameScreen.LevelMap);
                    SoundEngine.PlayMusic(MusicTrack.Menu);
                    AnnounceLevel(_mapIndex);
                    break;
            }
        }

        private void HandleFailedKeys(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.W:
                    _pauseIndex = (_pauseIndex + FailItems.Length - 1) % FailItems.Length;
                    AnnounceMenu(FailItems[_pauseIndex]);
                    Invalidate();
                    break;
                case Keys.Down:
                case Keys.S:
                    _pauseIndex = (_pauseIndex + 1) % FailItems.Length;
                    AnnounceMenu(FailItems[_pauseIndex]);
                    Invalidate();
                    break;
                case Keys.Enter:
                case Keys.Space:
                    SoundEngine.PlaySound("button");
                    if (_pauseIndex == 0)
                    {
                        StartLevel(_levelNumber);
                    }
                    else if (_pauseIndex == 1)
                    {
                        int price = GameProgress.GetBoosterPrice(BoosterType.ExtraMoves);
                        if (_progress.SpendGoldBars(price))
                        {
                            _progress.Save();
                            SoundEngine.PlaySound("klubb");
                            ContinueLevelWithExtraMoves(5);
                        }
                        else
                        {
                            SoundEngine.PlaySound("invalid");
                            Speech.Speak(string.Format(Localization.Get("shop.notenough"), price) + ". " + string.Format(Localization.Get("shop.gold"), _progress.GoldBars, _progress.Coins));
                        }
                    }
                    else
                    {
                        _menuIndex = 0;
                        SwitchScreen(GameScreen.MainMenu);
                        SoundEngine.PlayMusic(MusicTrack.Menu);
                    }
                    break;
                case Keys.Escape:
                    SwitchScreen(GameScreen.MainMenu);
                    SoundEngine.PlayMusic(MusicTrack.Menu);
                    break;
            }
        }

        private bool _isDownloadingUpdate = false;
        private long _bytesDownloaded = 0;
        private long _totalBytesToDownload = 0;
        private double _downloadSpeedMbps = 0;
        private DateTime _lastDownloadMeasureTime = DateTime.MinValue;
        private long _lastDownloadBytesMeasure = 0;

        private void HandleUpdateKeys(KeyEventArgs e)
        {
            if (!_isDownloadingUpdate)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    SoundEngine.PlaySound("button");
                    StartUpdateDownload();
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    SwitchScreen(GameScreen.MainMenu);
                    SoundEngine.PlayMusic(MusicTrack.Menu);
                }
            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.D1:
                    case Keys.NumPad1:
                        double mbDownloaded = _bytesDownloaded / (1024.0 * 1024.0);
                        Speech.SpeakInterrupt(string.Format(Localization.Get("update.mb_downloaded"), Math.Round(mbDownloaded, 1)));
                        break;
                    case Keys.D2:
                    case Keys.NumPad2:
                        double mbTotal = _totalBytesToDownload / (1024.0 * 1024.0);
                        Speech.SpeakInterrupt(string.Format(Localization.Get("update.mb_total"), Math.Round(mbTotal, 1)));
                        break;
                    case Keys.D3:
                    case Keys.NumPad3:
                        Speech.SpeakInterrupt(string.Format(Localization.Get("update.speed"), Math.Round(_downloadSpeedMbps, 2)));
                        break;
                    case Keys.Space:
                        int pct = _totalBytesToDownload > 0 ? (int)(_bytesDownloaded * 100 / _totalBytesToDownload) : 0;
                        Speech.SpeakInterrupt(string.Format(Localization.Get("update.percent"), pct));
                        break;
                }
            }
        }

        private async void StartUpdateDownload()
        {
            if (Updater.AvailableUpdate == null) return;
            _isDownloadingUpdate = true;
            Speech.SpeakInterrupt(Localization.Get("update.downloading"));
            string zipPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "CandyCrushAccessible_update.zip");

            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "CandyCrushAccessible-Updater");
                    using (var response = await client.GetAsync(Updater.AvailableUpdate.DownloadUrl, System.Net.Http.HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();
                        _totalBytesToDownload = response.Content.Headers.ContentLength ?? 0;
                        _bytesDownloaded = 0;
                        _lastDownloadMeasureTime = DateTime.UtcNow;
                        _lastDownloadBytesMeasure = 0;

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = new System.IO.FileStream(zipPath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None, 8192, true))
                        {
                            byte[] buffer = new byte[8192];
                            int read;
                            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, read);
                                _bytesDownloaded += read;

                                var now = DateTime.UtcNow;
                                double elapsed = (now - _lastDownloadMeasureTime).TotalSeconds;
                                if (elapsed >= 0.5)
                                {
                                    long diff = _bytesDownloaded - _lastDownloadBytesMeasure;
                                    _downloadSpeedMbps = (diff / (1024.0 * 1024.0)) / elapsed;
                                    _lastDownloadMeasureTime = now;
                                    _lastDownloadBytesMeasure = _bytesDownloaded;
                                }
                            }
                        }
                    }
                }

                Speech.SpeakInterrupt(Localization.Get("update.complete"));
                ApplyUpdateAndRestart(zipPath);
            }
            catch (Exception ex)
            {
                _isDownloadingUpdate = false;
                Speech.SpeakInterrupt("Error: " + ex.Message);
                SwitchScreen(GameScreen.MainMenu);
                SoundEngine.PlayMusic(MusicTrack.Menu);
            }
        }

        private void ApplyUpdateAndRestart(string zipPath)
        {
            try { SoundEngine.Shutdown(); } catch { }
            string appDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');
            string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string exeName = System.IO.Path.GetFileName(exePath);
            string batPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "apply_update.cmd");
            string logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "update_log.txt");

            string script = $@"@echo off
title Actualizador de Candy Crush Accesible
echo Esperando a que el motor principal se cierre por completo...

:wait_loop
tasklist /fi ""imagename eq {exeName}"" 2>NUL | find /i ""{exeName}"" >NUL
if ""%ERRORLEVEL%""==""0"" (
    timeout /t 1 /nobreak > nul
    goto wait_loop
)
timeout /t 2 /nobreak > nul

echo.
echo Extrayendo nueva version...
powershell -ExecutionPolicy Bypass -Command ""$ProgressPreference = 'SilentlyContinue'; Expand-Archive -Path '{zipPath}' -DestinationPath '{appDir}' -Force"" > ""{logPath}"" 2>&1

IF %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR CRITICO] Ocurrio un problema al extraer los archivos.
    echo Revisa el archivo update_log.txt para mas detalles.
    echo.
    type ""{logPath}""
    echo.
    echo Presiona cualquier tecla para cerrar esta ventana...
    pause > nul
    exit /b %ERRORLEVEL%
)

echo.
echo Limpiando archivos temporales...
if exist ""{zipPath}"" del ""{zipPath}""
if exist ""{logPath}"" del ""{logPath}""

echo.
echo Reiniciando el juego...
start """" ""{exePath}""
del ""%~f0""
";
            System.IO.File.WriteAllText(batPath, script);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = batPath,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal,
                UseShellExecute = true
            });
            Application.Exit();
        }

        private void DrawLoading(Graphics g)
        {
            string text = Localization.Get("game.title");
            SizeF s = g.MeasureString(text, Font);
            g.DrawString(text, new Font(Font.FontFamily, 24), Brushes.White,
                (ClientSize.Width - s.Width) / 2, ClientSize.Height / 2 - 20);
        }

        private void DrawUpdateAvailable(Graphics g)
        {
            g.DrawString(Localization.Get("game.title"), new Font(Font.FontFamily, 22), Brushes.Gold, 40, 30);
            if (Updater.AvailableUpdate != null)
            {
                string info = string.Format(Localization.Get("update.available"), Updater.AvailableUpdate.Version, Updater.AvailableUpdate.ReleaseNotes);
                g.DrawString(info, new Font(Font.FontFamily, 12), Brushes.White, new RectangleF(40, 80, ClientSize.Width - 80, ClientSize.Height - 160));
            }
        }

        private void DrawMainMenu(Graphics g)
        {
            string title = Localization.Get("game.title");
            g.DrawString(title, new Font(Font.FontFamily, 26), Brushes.Gold, 60, 60);
            g.DrawString(Localization.Get("game.subtitle"), Font, Brushes.LightGray, 60, 100);
            int y = 180;
            for (int i = 0; i < MainMenuItems.Length; i++)
            {
                string text = Localization.Get(MainMenuItems[i]);
                Brush b = i == _menuIndex ? Brushes.Gold : Brushes.White;
                Font f = i == _menuIndex ? new Font(Font.FontFamily, 16, FontStyle.Bold) : new Font(Font.FontFamily, 16);
                g.DrawString(text, f, b, 100, y);
                y += 50;
            }
            g.DrawString(string.Format(Localization.Get("lives.count"), _progress.Lives), Font, Brushes.LightGray, 100, y + 10);
        }

        private void DrawLevelMap(Graphics g)
        {
            g.DrawString(Localization.Get("menu.levelmap"), new Font(Font.FontFamily, 20), Brushes.Gold, 40, 30);
            g.DrawString(string.Format(Localization.Get("lives.count"), _progress.Lives), Font, Brushes.LightGray, 300, 36);
            int y = 80;
            for (int i = _mapIndex - 3; i <= _mapIndex + 3; i++)
            {
                if (i < 0 || i > _progress.CurrentLevel) continue;
                int n = i + 1;
                LevelDefinition l = Levels.Get(n);
                bool unlocked = _progress.IsUnlocked(n);
                string stars = "0";
                int best;
                if (_progress.BestStars.TryGetValue(n, out best)) stars = best.ToString();
                string text = (i == _mapIndex ? "> " : "  ") + Localization.Get("menu.level") + " " + n + "  " + l.ObjectiveText + "  " + Localization.Get("complete.stars") + " " + stars;
                Brush b = !unlocked ? Brushes.Gray : (i == _mapIndex ? Brushes.Gold : Brushes.White);
                Font f = i == _mapIndex ? new Font(Font.FontFamily, 13, FontStyle.Bold) : new Font(Font.FontFamily, 13);
                g.DrawString(text, f, b, 40, y);
                y += 34;
            }
        }

        private void DrawBoosters(Graphics g)
        {
            List<BoosterType> available = GetAvailableBoosters();
            g.DrawString(Localization.Get("booster.shop"), new Font(Font.FontFamily, 18), Brushes.Gold, 40, 25);
            g.DrawString(string.Format(Localization.Get("booster.selected"), _boosterSelection.Count), Font, Brushes.LightGray, 40, 55);
            int y = 90;
            for (int i = 0; i < available.Count; i++)
            {
                BoosterType t = available[i];
                string sel = _boosterSelection.Contains(t) ? "[" + Localization.Get("selected") + "]" : "";
                string text = (i == _boosterRow ? "> " : "  ") + Boosters.Name(t) + "  " +
                    string.Format(Localization.Get("booster.count"), _progress.GetBooster(t)) + " " + sel;
                Brush b = i == _boosterRow ? Brushes.Gold : Brushes.White;
                Font f = i == _boosterRow ? new Font(Font.FontFamily, 13, FontStyle.Bold) : new Font(Font.FontFamily, 13);
                g.DrawString(text, f, b, 40, y);
                y += 36;
            }
            string play = (_boosterRow == available.Count ? "> " : "  ") + Localization.Get("booster.play");
            Brush pb = _boosterRow == available.Count ? Brushes.Gold : Brushes.White;
            Font pf = _boosterRow == available.Count ? new Font(Font.FontFamily, 15, FontStyle.Bold) : new Font(Font.FontFamily, 15);
            g.DrawString(play, pf, pb, 40, y);
        }

        private void DrawPlaying(Graphics g)
        {
            if (_board == null) return;
            string header = Localization.Get("menu.level") + " " + _levelNumber + "   " +
                Localization.Get("score") + ": " + _board.Score;
            g.DrawString(header, new Font(Font.FontFamily, 12), Brushes.White, BoardX, 20);

            string movesLabel = _board.Level.Type == LevelType.Timed
                ? Localization.Get("time") + ": " + (int)Math.Ceiling(_board.TimeLeft)
                : Localization.Get("moves") + ": " + _board.MovesLeft;
            Font movesFont = new Font(Font.FontFamily, 14, FontStyle.Bold);
            SizeF sz = g.MeasureString(movesLabel, movesFont);
            int bw = (int)sz.Width + 24;
            int bh = (int)sz.Height + 14;
            Rectangle badge = new Rectangle(BoardX + Board.Cols * CellSize - bw, 16, bw, bh);
            g.FillRectangle(new SolidBrush(Color.FromArgb(90, 70, 110, 160)), badge);
            g.DrawRectangle(Pens.Gold, badge);
            g.DrawString(movesLabel, movesFont, Brushes.Gold, badge.X + 12, badge.Y + 7);

            string objective = _board.Level.ObjectiveText;
            g.DrawString(objective, Font, Brushes.LightGray, BoardX, 48);

            for (int y = 0; y < Board.Rows; y++)
            {
                for (int x = 0; x < Board.Cols; x++)
                {
                    Rectangle r = new Rectangle(BoardX + x * CellSize, BoardY + y * CellSize, CellSize - 2, CellSize - 2);
                    DrawCell(g, r, x, y);
                }
            }

            for (int x = 0; x < Board.Cols; x++)
            {
                g.DrawString(Board.ColLetter(x).ToString(), Font, Brushes.Gray, BoardX + x * CellSize + CellSize / 2 - 6, BoardY + Board.Rows * CellSize + 6);
            }
            for (int y = 0; y < Board.Rows; y++)
            {
                g.DrawString((y + 1).ToString(), Font, Brushes.Gray, BoardX - 20, BoardY + y * CellSize + CellSize / 2 - 8);
            }

            if (_boosterPanelActive && _ownedBoosters.Count > 0)
            {
                Rectangle panelRect = new Rectangle(BoardX + 15, BoardY + 30, Board.Cols * CellSize - 30, Math.Min(340, 60 + _ownedBoosters.Count * 48));
                using (SolidBrush bg = new SolidBrush(Color.FromArgb(245, 25, 20, 45)))
                {
                    g.FillRectangle(bg, panelRect);
                }
                g.DrawRectangle(Pens.Gold, panelRect);
                g.DrawString(string.Format(Localization.Get("booster.panel.open"), Board.CellName(_cursorX, _cursorY)), new Font(Font.FontFamily, 11, FontStyle.Bold), Brushes.Gold, panelRect.X + 15, panelRect.Y + 12);
                int py = panelRect.Y + 38;
                for (int i = 0; i < _ownedBoosters.Count; i++)
                {
                    BoosterType bt = _ownedBoosters[i];
                    string itemText = (i == _boosterPanelIndex ? "> " : "  ") + Boosters.Name(bt) + " (" + _progress.GetBooster(bt) + ")";
                    Brush b = i == _boosterPanelIndex ? Brushes.Gold : Brushes.White;
                    Font f = i == _boosterPanelIndex ? new Font(Font.FontFamily, 11, FontStyle.Bold) : new Font(Font.FontFamily, 11);
                    g.DrawString(itemText, f, b, panelRect.X + 15, py);
                    if (i == _boosterPanelIndex)
                    {
                        g.DrawString(Boosters.Description(bt), new Font(Font.FontFamily, 9), Brushes.LightGray, panelRect.X + 35, py + 18);
                    }
                    py += 44;
                }
            }
        }

        private void DrawCell(Graphics g, Rectangle r, int x, int y)
        {
            if (_board.IsDoubleJelly(x, y))
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(200, 210, 235, 240)), r);
            }
            else if (_board.HasJelly(x, y))
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(140, 210, 235, 240)), r);
            }

            if (_board.IsChocolate(x, y))
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(96, 58, 38)), r);
                g.DrawRectangle(Pens.SaddleBrown, r);
                return;
            }

            Candy c = _board.GetCandy(x, y);
            if (c == null) return;

            Color col = ColorFor(c.Color);
            Brush fill = new SolidBrush(col);
            g.FillEllipse(fill, r);
            g.DrawEllipse(Pens.White, r);

            string symbol = "";
            Font symFont = new Font(Font.FontFamily, 18, FontStyle.Bold);
            switch (c.Special)
            {
                case SpecialType.Striped: symbol = "S"; break;
                case SpecialType.Wrapped: symbol = "W"; break;
                case SpecialType.ColorBomb: symbol = "x"; break;
                case SpecialType.Fish: symbol = "F"; break;
            }
            if (c.IsTimeCandy) symbol = "+";
            if (c.BombTimer > 0) symbol = c.BombTimer.ToString();
            if (c.IsIngredient)
            {
                symbol = c.Ingredient == IngredientType.Cherry ? "C" : "N";
            }
            if (string.IsNullOrEmpty(symbol))
            {
                symbol = c.Color.ToString()[0].ToString();
            }
            SizeF s = g.MeasureString(symbol, symFont);
            g.DrawString(symbol, symFont, Brushes.White, r.X + (r.Width - s.Width) / 2, r.Y + (r.Height - s.Height) / 2 - 2);

            if (c.IsLicorice)
            {
                using (Pen p = new Pen(Color.DarkSlateGray, 3))
                {
                    g.DrawEllipse(p, r.X + 3, r.Y + 3, r.Width - 6, r.Height - 6);
                }
            }

            if (c.IsTimeCandy || c.Special == SpecialType.ColorBomb)
            {
                g.DrawEllipse(new Pen(Color.Gold, 2), r.X + 2, r.Y + 2, r.Width - 4, r.Height - 4);
            }
        }

        private static Color ColorFor(CandyColor color)
        {
            switch (color)
            {
                case CandyColor.Red: return Color.FromArgb(226, 80, 80);
                case CandyColor.Blue: return Color.FromArgb(76, 134, 226);
                case CandyColor.Green: return Color.FromArgb(86, 190, 96);
                case CandyColor.Yellow: return Color.FromArgb(232, 204, 66);
                case CandyColor.Orange: return Color.FromArgb(240, 146, 54);
                case CandyColor.Purple: return Color.FromArgb(164, 94, 226);
            }
            return Color.Gray;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        private void DrawOptions(Graphics g)
        {
            g.DrawString(Localization.Get("options.title"), new Font(Font.FontFamily, 20), Brushes.Gold, 40, 30);
            string binState = _progress.BinauralAmbientEnabled ? Localization.Get("yes") : Localization.Get("no");
            string[] values = {
                string.Format(Localization.Get("options.language.value"), Localization.LanguageName(Localization.Current)),
                string.Format(Localization.Get("options.value"), Localization.Get(OptionsItems[1]), (int)(SoundEngine.MusicVolume * 100)),
                string.Format(Localization.Get("options.value"), Localization.Get(OptionsItems[2]), (int)(SoundEngine.SfxVolume * 100)),
                string.Format(Localization.Get("options.value"), Localization.Get(OptionsItems[3]), (int)(SoundEngine.VoiceVolume * 100)),
                string.Format(Localization.Get("options.binaural.value"), binState),
                Localization.Get("options.update")
            };
            int y = 90;
            for (int i = 0; i < OptionsItems.Length; i++)
            {
                string text = (i == _optionsIndex ? "> " : "  ") + values[i];
                Brush b = i == _optionsIndex ? Brushes.Gold : Brushes.White;
                Font f = i == _optionsIndex ? new Font(Font.FontFamily, 14, FontStyle.Bold) : new Font(Font.FontFamily, 14);
                g.DrawString(text, f, b, 40, y);
                y += 45;
            }
        }

        private void DrawTutorial(Graphics g)
        {
            g.DrawString(Localization.Get("tutorial.title") + " " + (_tutorialPage + 1) + "/4",
                new Font(Font.FontFamily, 20), Brushes.Gold, 40, 30);
            string text = Localization.Get("tutorial.page" + (_tutorialPage + 1));
            g.DrawString(text, new Font(Font.FontFamily, 12), Brushes.White,
                new RectangleF(40, 80, ClientSize.Width - 80, ClientSize.Height - 160));
        }

        private void DrawPause(Graphics g)
        {
            g.DrawString(Localization.Get("pause.title"), new Font(Font.FontFamily, 22), Brushes.Gold, 60, 60);
            int y = 140;
            for (int i = 0; i < PauseItems.Length; i++)
            {
                string text = (i == _pauseIndex ? "> " : "  ") + Localization.Get(PauseItems[i]);
                Brush b = i == _pauseIndex ? Brushes.Gold : Brushes.White;
                Font f = i == _pauseIndex ? new Font(Font.FontFamily, 15, FontStyle.Bold) : new Font(Font.FontFamily, 15);
                g.DrawString(text, f, b, 100, y);
                y += 45;
            }
        }

        private void DrawComplete(Graphics g)
        {
            string title = Localization.Get("complete.title");
            g.DrawString(title, new Font(Font.FontFamily, 24), Brushes.Gold, 60, 60);
            g.DrawString(string.Format(Localization.Get("complete.score"), _board != null ? _board.Score : 0),
                new Font(Font.FontFamily, 15), Brushes.White, 60, 120);
            string stars = Localization.StarLabel(_board != null ? _board.StarsEarned : 0);
            g.DrawString(string.Format(Localization.Get("complete.stars"), stars),
                new Font(Font.FontFamily, 15), Brushes.Gold, 60, 160);
            g.DrawString(Localization.Get("complete.next"), Font, Brushes.LightGray, 60, 220);
        }

        private void DrawFailed(Graphics g)
        {
            g.DrawString(Localization.Get("failed.title"), new Font(Font.FontFamily, 24), Brushes.IndianRed, 60, 60);
            int y = 140;
            for (int i = 0; i < FailItems.Length; i++)
            {
                string text = FailItems[i] == "failed.extramoves"
                    ? string.Format(Localization.Get(FailItems[i]), GameProgress.GetBoosterPrice(BoosterType.ExtraMoves))
                    : Localization.Get(FailItems[i]);
                text = (i == _pauseIndex ? "> " : "  ") + text;
                Brush b = i == _pauseIndex ? Brushes.Gold : Brushes.White;
                Font f = i == _pauseIndex ? new Font(Font.FontFamily, 15, FontStyle.Bold) : new Font(Font.FontFamily, 15);
                g.DrawString(text, f, b, 100, y);
                y += 45;
            }
        }
    }
}