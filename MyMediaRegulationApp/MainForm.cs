using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using NAudio.CoreAudioApi;

namespace MyMediaRegulationApp
{
    public partial class MainForm : Form
    {
        private System.Windows.Forms.Timer overlayTimer;
        private Label volumeLabel;
        private ProgressBar volumeBar;
        private NotifyIcon trayIcon;
        private KeyboardHook hook;

        private const string ProcessName = "spotify"; // Назва процесу для регулювання гучності
        private const float VolumeStep = 0.01f; // Крок зміни гучності

        public MainForm()
        {
            InitializeComponent();
            InitializeOverlay();
            InitializeTray();
            HookKeyboard();
        }
        private void InitializeOverlay()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Black;
            this.Opacity = 0.8;
            this.Size = new Size(300, 100);
            this.TopMost = true;
            this.ShowInTaskbar = false;

            volumeLabel = new Label
            {
                ForeColor = Color.White,
                Font = new Font("Arial", 14, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 30
            };

            volumeBar = new ProgressBar
            {
                Dock = DockStyle.Bottom,
                Height = 20
            };

            this.Controls.Add(volumeLabel);
            this.Controls.Add(volumeBar);

            overlayTimer = new System.Windows.Forms.Timer
            {
                Interval = 2000 // Закриваємо оверлей через 2 секунди
            };
            overlayTimer.Tick += (s, e) => this.Hide();

            this.Shown += (s, e) => PositionOverlay();
        }

        private void PositionOverlay()
        {
            var screen = Screen.PrimaryScreen.Bounds; // Використовуємо Bounds для точного розрахунку
            int x = (screen.Width - this.Width) / 2;
            int y = screen.Height - this.Height - 50; // Відступ від нижнього краю

            this.Location = new Point(x, y);
        }


        private void InitializeTray()
        {
            trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Information,
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip()
            };
            trayIcon.ContextMenuStrip.Items.Add("Вийти", null, (s, e) => Application.Exit());
        }

        private void HookKeyboard()
        {
            hook = new KeyboardHook();
            hook.KeyPressed += (s, e) =>
            {
                if (e.Key == Keys.F24) // Збільшити гучність
                {
                    ChangeAppVolume(ProcessName, +VolumeStep);
                }
                else if (e.Key == Keys.F23) // Зменшити гучність
                {
                    ChangeAppVolume(ProcessName, -VolumeStep);
                }
            };
            hook.Hook();
        }

        private void ChangeAppVolume(string appName, float step)
        {
            var deviceEnumerator = new MMDeviceEnumerator();
            var device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            var sessions = device.AudioSessionManager.Sessions;

            for (int i = 0; i < sessions.Count; i++)
            {
                var session = sessions[i];
                if (session.GetProcessID != 0)
                {
                    var process = Process.GetProcessById((int)session.GetProcessID);
                    if (process.ProcessName.ToLower() == appName.Replace(".exe", "").ToLower())
                    {
                        float currentVolume = session.SimpleAudioVolume.Volume;
                        float newVolume = Math.Max(0.0f, Math.Min(1.0f, currentVolume + step));
                        session.SimpleAudioVolume.Volume = newVolume;

                        ShowOverlay(newVolume);
                        return;
                    }
                }
            }
        }

        private void ShowOverlay(float volume)
        {
            volumeLabel.Text = $"Гучність: {volume * 100:0}%";
            volumeBar.Value = (int)(volume * 100);

            PositionOverlay(); // Оновлюємо позицію перед показом

            overlayTimer.Stop();  // Зупиняємо таймер, якщо він уже працює
            overlayTimer.Start(); // Запускаємо його заново

            this.Show();
        }


        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            trayIcon.Dispose();
            hook.Unhook();
        }
    }

    public class KeyPressedEventArgs : EventArgs
    {
        public Keys Key { get; }

        public KeyPressedEventArgs(Keys key)
        {
            Key = key;
        }
    }
}
