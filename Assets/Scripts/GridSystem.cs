using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
    public NativeMultiHashMap<int, int> Grid;

    protected override void OnCreateManager()
    {
        Grid = new NativeMultiHashMap<int, int>(1, Allocator.TempJob);
    }

    public struct Data
    {
        public readonly int Length;
        public ComponentDataArray<Position> Position;
        public ComponentDataArray<Size> Size;
    }

    [Inject] Data m_Data;

    [BurstCompile]
    struct PopulateGridJob : IJobParallelFor
    {
        [ReadOnly] public ComponentDataArray<Position> Positions;
        [ReadOnly] public ComponentDataArray<Size> Sizes;
        public NativeMultiHashMap<int, int>.Concurrent HashMap;

        public int ArenaSize;
        public int NPartitions;

        public void Execute(int index)
        {
            float3 position = Positions[index].Value;
            float radius = Sizes[index].Value / 2.0f;

            for (int i = GetMinGridPosition(position.x, radius); i <= GetMaxGridPosition(position.x, radius); i++)
            {
                for (int j = GetMinGridPosition(position.y, radius); j <= GetMaxGridPosition(position.y, radius); j++)
                {
                    HashMap.Add(Util.GetHashCode(i, j), index);
                }
            }
        }

        int GetMinGridPosition(float pos, float radius)
        {
            float minPos = pos - radius;
            return Util.GetGridIndex(minPos, ArenaSize, NPartitions);
        }

        int GetMaxGridPosition(float pos, float radius)
        {
            float maxPos = pos + radius;
            return Util.GetGridIndex(maxPos, ArenaSize, NPartitions);
        }
    }

    protected override void OnUpdate()
    {
        Grid.Dispose();
        int capacity = m_Data.Length * 9; // each object can be in up to 9 cells
        Grid = new NativeMultiHashMap<int, int>(capacity, Allocator.TempJob);

        var populateGridJob = new PopulateGridJob
        {
            HashMap = Grid.ToConcurrent(),
            Positions = m_Data.Position,
            Sizes = m_Data.Size,
            ArenaSize = Bootstrap.Settings.ArenaSize * 10,
            NPartitions = Bootstrap.Settings.NPartitions
        };

        var jobHandle = populateGridJob.Schedule(m_Data.Length, 64);
        jobHandle.Complete();
    }
}
