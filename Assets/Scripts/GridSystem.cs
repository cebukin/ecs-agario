using System;
using System.Collections.Generic;
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
    readonly int NPartitions = 10;

    protected override void OnCreateManager()
    {
        grid = new List<List<List<int>>>();
        for (int i = 0; i < NPartitions; i++)
        {
            List<List<int>> row = new List<List<int>>();
            for (int j = 0; j < NPartitions; j++)
            {
                row.Add(new List<int>());
            }
            grid.Add(row);
        }
    }

    public struct Data
    {
        public readonly int Length;
        public ComponentDataArray<Position> Position;
        public ComponentDataArray<Size> Size;
    }

    [Inject] Data m_Data;

    void ClearGrid()
    {
        for (int i = 0; i < NPartitions; i++)
        {
            for (int j = 0; j < NPartitions; j++)
            {
                grid[i][j].Clear();
            }
        }
    }

    int GetGridIndex(float pos)
    {
        int arenaSize = Bootstrap.Settings.ArenaSize * 10;
        float gridSize = (float) arenaSize / NPartitions;
        float gridPos = pos + arenaSize / 2.0f;
        return (int) math.clamp(math.floor(gridPos / gridSize), 0, NPartitions - 1);
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
        ClearGrid();
        PopulateGrid();
    }
}
