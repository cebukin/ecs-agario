using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;

[UpdateAfter(typeof(MoveSystem))]
public class GridSystem : ComponentSystem
{
    public List<List<List<int>>> grid;

    public struct Data
    {
        public readonly int Length;
        public ComponentDataArray<Position> Position;
        public ComponentDataArray<Size> Size;
    }

    [Inject] Data m_Data;

    void Init()
    {
        grid = new List<List<List<int>>>();
        for (int i = 0; i < Bootstrap.Settings.NPartitions; i++)
        {
            List<List<int>> row = new List<List<int>>();
            for (int j = 0; j < Bootstrap.Settings.NPartitions; j++)
            {
                row.Add(new List<int>());
            }

            grid.Add(row);
        }
    }

    void ClearGrid()
    {
        foreach (var t in grid)
        {
            foreach (var t1 in t)
            {
                t1.Clear();
            }
        }
    }

    int GetGridIndex(float pos)
    {
        int arenaSize = Bootstrap.Settings.ArenaSize * 10;
        float gridSize = (float) arenaSize / Bootstrap.Settings.NPartitions;
        float gridPos = pos + arenaSize / 2.0f;
        return (int) math.clamp(math.floor(gridPos / gridSize), 0, Bootstrap.Settings.NPartitions - 1);
    }

    int GetMinGridPosition(float pos, float radius)
    {
        float minPos = pos - radius;
        return GetGridIndex(minPos);
    }

    int GetMaxGridPosition(float pos, float radius)
    {
        float maxPos = pos + radius;
        return GetGridIndex(maxPos);
    }

    void PopulateGrid(int index)
    {
        float3 position = m_Data.Position[index].Value;
        float radius = m_Data.Size[index].Value / 2.0f;

        for (int i = GetMinGridPosition(position.x, radius); i <= GetMaxGridPosition(position.x, radius); i++)
        {
            for (int j = GetMinGridPosition(position.y, radius); j <= GetMaxGridPosition(position.y, radius); j++)
            {
                grid[i][j].Add(index);
            }
        }
    }

    void PopulateGrid()
    {
        for (int i = 0; i < m_Data.Length; i++)
        {
            PopulateGrid(i);
        }
    }

    protected override void OnUpdate()
    {
        if (grid == null)
        {
            Init();
        }

        ClearGrid();
        PopulateGrid();
    }
}
