using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class BotInputSystem : PostGridSystem
{
    public struct BotData
    {
        public readonly int Length;
        public ComponentDataArray<BotInput> BotInput;
        public ComponentDataArray<Heading> Heading;
        public ComponentDataArray<Position> Position;
        public ComponentDataArray<Size> Size;
    }

    [Inject] BotData m_BotData;

    [BurstCompile]
    struct CalculateBotHeadingsJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Position> Positions;
        [ReadOnly] public NativeMultiHashMap<int, int> Grid;
        [ReadOnly] public NativeArray<Size> Sizes;

        [ReadOnly] public ComponentDataArray<Position> BotPositions;
        [ReadOnly] public ComponentDataArray<Size> BotSizes;
        public ComponentDataArray<Heading> BotHeadings;

        public float CellSize;

        public void Execute(int index)
        {
            float3 botPosition = BotPositions[index].Value;
            int botSize = BotSizes[index].Value;

            int cellGridHash = Util.Hash(botPosition, CellSize);

            int otherItem;
            NativeMultiHashMapIterator<int> iterator;

            float3 heading = BotHeadings[index].Value;
            int targetSize = int.MinValue;

            if (Grid.TryGetFirstValue(cellGridHash, out otherItem, out iterator))
            {
                do
                {
                    float3 bodyPosition = Positions[otherItem].Value;
                    int bodySize = Sizes[otherItem].Value;

                    if (bodySize <= targetSize)
                    {
                        continue;
                    }

                    float3 targetVector = bodyPosition - botPosition;
                    float distToTarget = math.length(targetVector);

                    if (distToTarget <= float.Epsilon)
                    {
                        continue;
                    }

                    targetSize = bodySize;
                    bool isThreat = bodySize > botSize;
                    heading = (isThreat ? -1 : 1) * math.normalize(targetVector);
                } while (Grid.TryGetNextValue(out otherItem, ref iterator));
            }

            BotHeadings[index] = new Heading
            {
                Value = heading
            };
        }


    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var copyBarrier = Setup(inputDeps);

        var calculateBotHeadingsJob = new CalculateBotHeadingsJob
        {
            Positions = _positionsCopy,
            Sizes = _sizeCopy,
            Grid = _gridSystem.Grid,
            BotPositions = m_BotData.Position,
            BotSizes = m_BotData.Size,
            BotHeadings = m_BotData.Heading,
            CellSize = Bootstrap.Settings.CellSize
        };

        return calculateBotHeadingsJob.Schedule(m_BotData.Length, 64, copyBarrier);
    }
}
