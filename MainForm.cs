using System.Diagnostics;

namespace TitanSoulsESP;

public partial class MainForm : Form
{
    private readonly MemoryManager _memory;
    private readonly GameReader _gameReader;
    private System.Windows.Forms.Timer? _updateTimer;
    private Process? _targetProcess;
    private IntPtr _gameWindowHandle;

    public MainForm()
    {
        _memory = new MemoryManager();
        _gameReader = new GameReader(_memory);
        
        InitializeComponent();
        SetupOverlay();
        SetupTimer();
    }

    private void InitializeComponent()
    {
        Text = "Titan Souls ESP";
        Size = new Size(300, 400);
        FormBorderStyle = FormBorderStyle.None;
        BackColor = Color.Magenta;
        TransparencyKey = Color.Magenta;
        TopMost = true;
        ShowInTaskbar = false;
        
        var statusLabel = new Label
        {
            Name = "statusLabel",
            Text = "状态: 等待游戏...",
            ForeColor = Color.White,
            Location = new Point(10, 10),
            AutoSize = true,
            BackColor = Color.Transparent
        };
        Controls.Add(statusLabel);
    }

    private void SetupOverlay()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer, true);
    }

    private void SetupTimer()
    {
        _updateTimer = new System.Windows.Forms.Timer
        {
            Interval = 16 // ~60 FPS
        };
        _updateTimer.Tick += UpdateTimer_Tick;
        _updateTimer.Start();
    }

    private void UpdateTimer_Tick(object? sender, EventArgs e)
    {
        if (!_memory.IsAttached)
        {
            TryAttachToGame();
        }
        else
        {
            UpdateOverlayPosition();
            _gameReader.Update();
        }
        
        Invalidate();
    }

    private void TryAttachToGame()
    {
        if (_memory.Attach())
        {
            _gameReader.Initialize();
            FindGameWindow();
            UpdateStatusLabel("已连接到游戏");
        }
    }

    private void FindGameWindow()
    {
        var processes = Process.GetProcessesByName("TITAN");
        if (processes.Length > 0)
        {
            _targetProcess = processes[0];
            _gameWindowHandle = _targetProcess.MainWindowHandle;
        }
    }

    private void UpdateOverlayPosition()
    {
        if (_targetProcess == null || _targetProcess.HasExited)
        {
            _memory.Detach();
            UpdateStatusLabel("游戏已关闭");
            return;
        }

        if (_gameWindowHandle != IntPtr.Zero)
        {
            if (NativeMethods.GetWindowRect(_gameWindowHandle, out var rect))
            {
                Bounds = Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
            }
        }
    }

    private void UpdateStatusLabel(string text)
    {
        if (Controls.Find("statusLabel", false).FirstOrDefault() is Label label)
        {
            label.Text = $"状态: {text}";
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        if (!_gameReader.IsReady) return;

        DrawPlayer(g);
        DrawBosses(g);
        DrawArrow(g);
    }

    private void DrawPlayer(Graphics g)
    {
        var player = _gameReader.Player;
        if (!player.IsActive) return;

        var screenPos = WorldToScreen(player.Position);
        using var pen = new Pen(Color.Lime, 2);
        using var brush = new SolidBrush(Color.Lime);
        using var font = new Font("Arial", 10);
        
        // 绘制玩家标记
        g.DrawEllipse(pen, screenPos.X - 10, screenPos.Y - 10, 20, 20);
        g.DrawString($"Player [{player.Health:F0}]", font, brush, screenPos.X + 15, screenPos.Y - 8);
    }

    private void DrawBosses(Graphics g)
    {
        using var pen = new Pen(Color.Red, 2);
        using var brush = new SolidBrush(Color.Red);
        using var font = new Font("Arial", 10);
        
        foreach (var boss in _gameReader.Bosses)
        {
            var screenPos = WorldToScreen(boss.Position);
            
            // 绘制 boss 方框
            g.DrawRectangle(pen, screenPos.X - 15, screenPos.Y - 15, 30, 30);
            g.DrawString($"{boss.Name} [{boss.Health:F0}]", font, brush, screenPos.X + 20, screenPos.Y - 8);
        }
    }

    private void DrawArrow(Graphics g)
    {
        if (_gameReader.Player.HasArrow) return;
        
        var arrowPos = _gameReader.Player.ArrowPosition;
        var screenPos = WorldToScreen(arrowPos);
        
        using var pen = new Pen(Color.Yellow, 2);
        using var brush = new SolidBrush(Color.Yellow);
        
        // 绘制箭头标记
        g.DrawLine(pen, screenPos.X - 8, screenPos.Y, screenPos.X + 8, screenPos.Y);
        g.DrawLine(pen, screenPos.X, screenPos.Y - 8, screenPos.X, screenPos.Y + 8);
    }

    private Point WorldToScreen(Vector2 worldPos)
    {
        // TODO: 需要找到游戏的相机/视图矩阵
        // 目前返回屏幕中心偏移
        return new Point(
            (int)(worldPos.X + Width / 2),
            (int)(worldPos.Y + Height / 2)
        );
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _updateTimer?.Dispose();
            _memory.Dispose();
        }
        base.Dispose(disposing);
    }
}

internal static class NativeMethods
{
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
