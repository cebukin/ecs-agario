using Unity.Mathematics;
using Unity.Transforms;

public static class Util
{
    public static int GetGridIndex(float pos, int arenaSize, int nPartitions)
    {
        float gridSize = (float) arenaSize / nPartitions;
        float gridPos = pos + arenaSize / 2.0f;
        return (int) math.clamp(math.floor(gridPos / gridSize), 0, nPartitions - 1);
    }

    public static int2 GetGridIndex(Position pos, int arenaSize, int nPartitions)
    {
        return new int2
        (
            GetGridIndex(pos.Value.x, arenaSize, nPartitions),
            GetGridIndex(pos.Value.y, arenaSize, nPartitions)
        );
    }

    public static int GetHashCode(int x, int y)
    {
        return (y << 16) ^ x;
    }
}
