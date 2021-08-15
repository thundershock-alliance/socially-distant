using System;
using SociallyDistant.Core;
using SociallyDistant.Core.Components;
using SociallyDistant.Core.Config;
using SociallyDistant.Core.Game;
using SociallyDistant.Core.Net;
using SociallyDistant.Core.SaveData;
using SociallyDistant.Core.Windowing;
using Thundershock;
using Thundershock.Core;
using Thundershock.Core.Input;
using Thundershock.Gui;
using Thundershock.Gui.Elements;
using Thundershock.Gui.Elements.Console;

namespace SociallyDistant
{
    public sealed class Workspace : Scene
    {
        #region APP REFERENCES

        private SaveManager _saveManager;
        private RedConfigManager _redConf;
        
        #endregion

        #region SCENE COMPONENTS

        private WindowManager _windowManager;
        private Shell _shell;

        #endregion
        
        #region USER INTERFACE

        private Stacker _infoLeft = new();
        private Stacker _master = new();
        private Panel _infoBanner = new();
        private Stacker _infoMaster = new();
        private Stacker _infoProfileCard = new();
        private Picture _playerAvatar = new();
        private TextBlock _playerName = new();
        private Stacker _playerInfoStacker = new();
        private Stacker _infoRight = new();
        private Button _settings = new();
        private ConsoleControl _console = new();
        
        #endregion
        
        #region STATE

        private TimeSpan _uptime;
        private TimeSpan _frameTime;
        private IRedTeamContext _context;
        private ColorPalette _palette;
        
        #endregion

        #region WINDOWS

        private SettingsWindow _settingsWindow;

        #endregion
        
        #region PROPERTIES

        public TimeSpan Uptime => _uptime;
        public TimeSpan FrameTime => _frameTime;

        #endregion
        
        protected override void OnLoad()
        {
            // Grab app references.
            _saveManager = Game.GetComponent<SaveManager>();
            _redConf = Game.GetComponent<RedConfigManager>();
            
            // Build the workspace GUI.
            BuildGui();

            // Load the redconf state.
            LoadConfig();
            
            // Style the GUI.
            StyleGui();
            
            // Start the command shell.
            StartShell();
            
            // Bind to configuration reloads.
            _redConf.ConfigUpdated += RedConfOnConfigUpdated;
            
            // Window manager.
            _windowManager = RegisterSystem<WindowManager>();
            
            base.OnLoad();

            _settings.MouseUp += SettingsOnMouseUp;
        }

        private void SettingsOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Primary)
            {
                if (_settingsWindow == null)
                {
                    _settingsWindow = _windowManager.OpenWindow<SettingsWindow>();
                    _settingsWindow.WindowClosed += SettingsWindowOnWindowClosed;
                }
            }
        }

        private void SettingsWindowOnWindowClosed(object sender, EventArgs e)
        {
            _settingsWindow = null;
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            _frameTime = gameTime.ElapsedGameTime;
            _uptime = gameTime.TotalGameTime;

            _playerName.Text = _saveManager.CurrentGame.PlayerName;
            
            base.OnUpdate(gameTime);
        }

        private void RedConfOnConfigUpdated(object sender, EventArgs e)
        {
            LoadConfig();
            StyleGui();
        }

        private void LoadConfig()
        {
            // console fonts.
            _redConf.SetConsoleFonts(_console);
            
            // Color palette.
            _palette = _redConf.GetPalette();
            _console.ColorPalette = _palette;
        }
        
        private void StartShell()
        {
            // Start the game's simulation.
            var simulation = RegisterSystem<Simulation>();
            
            // Register the shell as a system.
            _shell = RegisterSystem<Shell>();

            // Attach a shell to the player entity.
            var playerEntity = simulation.GetPlayerEntity();
            Registry.AddComponent(playerEntity, (IConsole) _console);
            Registry.AddComponent(playerEntity, new ShellStateComponent
            {
                UserId = 1 // uses the player's  normal user account instead of root.
            });
        }
        
        private void BuildGui()
        {
            _playerInfoStacker.Children.Add(_playerName);
            
            _infoProfileCard.Children.Add(_playerAvatar);
            _infoProfileCard.Children.Add(_playerInfoStacker);

            _infoRight.Children.Add(_settings);
            _infoRight.Children.Add(_infoProfileCard);

            _infoMaster.Children.Add(_infoLeft);
            _infoMaster.Children.Add(_infoRight);

            _infoBanner.Children.Add(_infoMaster);

            _master.Children.Add(_infoBanner);
            _master.Children.Add(_console);
            
            Gui.AddToViewport(_master);
        }

        private void StyleGui()
        {
            _settings.Text = "Settings";
            
            _playerAvatar.VerticalAlignment = VerticalAlignment.Center;
            _playerInfoStacker.VerticalAlignment = VerticalAlignment.Center;
            _settings.VerticalAlignment = VerticalAlignment.Center;

            _settings.Padding = new Padding(4, 0, 4, 0);
            
            _playerAvatar.FixedWidth = 24;
            _playerAvatar.FixedHeight = 24;
            _playerAvatar.ImageMode = ImageMode.Rounded;

            _infoMaster.Direction = StackDirection.Horizontal;
            _infoProfileCard.Direction = StackDirection.Horizontal;
            _infoRight.Direction = StackDirection.Horizontal;
            _infoLeft.Direction = StackDirection.Horizontal;

            _infoRight.HorizontalAlignment = HorizontalAlignment.Right;
            
            // Fills.
            _console.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            _infoRight.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            _infoLeft.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);

            _playerInfoStacker.Padding = new Padding(3, 0, 0, 0);
            _infoMaster.Padding = new Padding(4, 2, 4, 2);

            _console.DrawBackgroundImage = false;
        }

        private void TrySave(IConsole console)
        {
            _saveManager.Save();
            _console.WriteLine($"&b * save successful * &B");
        }
    }
}