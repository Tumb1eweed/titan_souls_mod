namespace TitanSoulsESP;

public struct Vector2
{
    public float X { get; set; }
    public float Y { get; set; }

    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }
}

public class Entity
{
    public string Name { get; set; } = "";
    public Vector2 Position { get; set; }
    public float Health { get; set; }
    public bool IsActive { get; set; }
}

public class Player : Entity
{
    public Vector2 ArrowPosition { get; set; }
    public bool HasArrow { get; set; }
}

public class Boss : Entity
{
    public string BossType { get; set; } = "";
}
