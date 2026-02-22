namespace TitanSoulsESP;

public class GameReader
{
    private readonly MemoryManager _memory;
    
    public Player Player { get; } = new();
    public List<Boss> Bosses { get; } = new();
    
    public IntPtr BaseAddress { get; private set; }
    public bool IsReady => _memory.IsAttached && BaseAddress != IntPtr.Zero;

    public GameReader(MemoryManager memory)
    {
        _memory = memory;
    }

    public void Initialize()
    {
        if (!_memory.IsAttached) return;
        
        BaseAddress = _memory.GetModuleBase("TITAN.exe");
    }

    public void Update()
    {
        if (!IsReady) return;

        ReadPlayer();
        ReadBosses();
    }

    private void ReadPlayer()
    {
        // TODO: 用 Cheat Engine 找到实际偏移
        // 示例偏移 (需要替换为实际值)
        IntPtr playerBase = BaseAddress + 0x123456;
        
        float posX = _memory.ReadFloat(playerBase + 0x10);
        float posY = _memory.ReadFloat(playerBase + 0x14);
        
        Player.Name = "Player";
        Player.Position = new Vector2(posX, posY);
        Player.Health = _memory.ReadFloat(playerBase + 0x20);
        Player.HasArrow = _memory.ReadByte(playerBase + 0x30) == 1;
        
        if (!Player.HasArrow)
        {
            float arrowX = _memory.ReadFloat(playerBase + 0x40);
            float arrowY = _memory.ReadFloat(playerBase + 0x44);
            Player.ArrowPosition = new Vector2(arrowX, arrowY);
        }
    }

    private void ReadBosses()
    {
        Bosses.Clear();
        
        // TODO: 用 Cheat Engine 找到 boss 数组基址和数量
        // 示例: 遍历 boss 列表
        IntPtr bossListBase = BaseAddress + 0x234567;
        int bossCount = _memory.ReadInt(BaseAddress + 0x234560);
        
        for (int i = 0; i < Math.Min(bossCount, 20); i++)
        {
            IntPtr bossPtr = (IntPtr)_memory.ReadInt(bossListBase + i * 8);
            
            if (bossPtr == IntPtr.Zero) continue;
            
            var boss = new Boss
            {
                Name = $"Boss {i}",
                Position = new Vector2(
                    _memory.ReadFloat(bossPtr + 0x10),
                    _memory.ReadFloat(bossPtr + 0x14)
                ),
                Health = _memory.ReadFloat(bossPtr + 0x20),
                IsActive = _memory.ReadByte(bossPtr + 0x30) == 1
            };
            
            if (boss.IsActive)
                Bosses.Add(boss);
        }
    }
}
