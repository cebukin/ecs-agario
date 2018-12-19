using Unity.Mathematics;
using Unity.Transforms;

public static class Util
{
    public static int GetGridIndex(float pos)
    {
        int arenaSize = Bootstrap.Settings.ArenaSize * 10;
        float gridSize = (float) arenaSize / Bootstrap.Settings.NPartitions;
        float gridPos = pos + arenaSize / 2.0f;
        return (int) math.clamp(math.floor(gridPos / gridSize), 0, Bootstrap.Settings.NPartitions - 1);
    }

    public static int2 GetGridIndex(Position pos)
    {
        return new int2(GetGridIndex(pos.Value.x), GetGridIndex(pos.Value.y));
    }
}
