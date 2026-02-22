using Memory;

namespace TitanSoulsESP;

public class MemoryManager : IDisposable
{
    private const string PROCESS_NAME = "TITAN";
    private Mem? _mem;
    private bool _attached;
    private bool _disposed;

    public bool IsAttached => _attached;

    public event Action? OnAttach;
    public event Action? OnDetach;

    public bool Attach()
    {
        if (_attached) return true;

        _mem = new Mem();
        int processId = _mem.GetProcIdFromName(PROCESS_NAME);

        if (processId == 0)
        {
            _attached = false;
            return false;
        }

        _attached = _mem.OpenProcess(processId);
        
        if (_attached)
            OnAttach?.Invoke();
        else
            OnDetach?.Invoke();

        return _attached;
    }

    public void Detach()
    {
        if (!_attached) return;
        
        _attached = false;
        OnDetach?.Invoke();
    }

    public float ReadFloat(IntPtr address)
    {
        if (!_attached || _mem == null) return 0;
        return _mem.ReadFloat(address.ToString("X"));
    }

    public int ReadInt(IntPtr address)
    {
        if (!_attached || _mem == null) return 0;
        return _mem.ReadInt(address.ToString("X"));
    }

    public byte ReadByte(IntPtr address)
    {
        if (!_attached || _mem == null) return 0;
        return _mem.ReadByte(address.ToString("X"));
    }

    public byte[] ReadBytes(IntPtr address, int length)
    {
        if (!_attached || _mem == null) return Array.Empty<byte>();
        return _mem.ReadBytes(address.ToString("X"), length);
    }

    public IntPtr GetModuleBase(string moduleName)
    {
        if (!_attached || _mem == null) return IntPtr.Zero;
        return (IntPtr)_mem.GetModuleBase(moduleName);
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        Detach();
        _disposed = true;
    }
}
