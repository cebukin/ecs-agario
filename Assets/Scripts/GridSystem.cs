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
    public List<List<List<int>>> Grid;

    public struct Data
    {
        public readonly int Length;
        public ComponentDataArray<Position> Position;
        public ComponentDataArray<Size> Size;
    }

    [Inject] Data m_Data;

    void Init()
    {
        if (Grid != null)
        {
            return;
        }

        Grid = new List<List<List<int>>>();
        for (int i = 0; i < Bootstrap.Settings.NPartitions; i++)
        {
            List<List<int>> row = new List<List<int>>();
            for (int j = 0; j < Bootstrap.Settings.NPartitions; j++)
            {
                row.Add(new List<int>());
            }

            Grid.Add(row);
        }
    }

    void ClearGrid()
    {
        foreach (var t in Grid)
        {
            foreach (var t1 in t)
            {
                t1.Clear();
            }
        }
    }

    int GetMinGridPosition(float pos, float radius)
    {
        float minPos = pos - radius;
        return Util.GetGridIndex(minPos);
    }

    int GetMaxGridPosition(float pos, float radius)
    {
        float maxPos = pos + radius;
        return Util.GetGridIndex(maxPos);
    }

    void PopulateGrid(int index)
    {
        float3 position = m_Data.Position[index].Value;
        float radius = m_Data.Size[index].Value / 2.0f;

        for (int i = GetMinGridPosition(position.x, radius); i <= GetMaxGridPosition(position.x, radius); i++)
        {
            for (int j = GetMinGridPosition(position.y, radius); j <= GetMaxGridPosition(position.y, radius); j++)
            {
                Grid[i][j].Add(index);
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
        Init();
        ClearGrid();
        PopulateGrid();
    }
}
