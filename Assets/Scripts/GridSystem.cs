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

    protected override void OnDestroyManager()
    {
        Grid.Dispose();
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
        public NativeMultiHashMap<int, int>.Concurrent HashMap;
        public float CellSize;

        public void Execute(int index)
        {
            float3 position = Positions[index].Value;
            HashMap.Add(Util.Hash(position, CellSize), index);
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
            CellSize = Bootstrap.Settings.CellSize
        };

        var jobHandle = populateGridJob.Schedule(m_Data.Length, 64);
        jobHandle.Complete();
    }
}
