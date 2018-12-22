using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[UpdateAfter(typeof(GridSystem))]
public class CandidatesSystem : ComponentSystem
{
    HashSet<int2> _candidatePairs;
    [Inject] GridSystem _gridSystem;

    public int2[] CandidatePairs => _candidatePairs.ToArray();

    protected override void OnCreateManager()
    {
        _candidatePairs = new HashSet<int2>(new PointComparer());
    }

    void AddCandidatePair(int a, int b)
    {
        if (a == b)
        {
            return;
        }

        int2 pair = a > b ? new int2(a, b) : new int2(b, a);
        _candidatePairs.Add(pair);
    }

    void AddCandidatePairs(IReadOnlyList<int> candidatesArray)
    {
        for (int i = 0; i < candidatesArray.Count; i++)
        {
            for (int j = i + 1; j < candidatesArray.Count; j++)
            {
                AddCandidatePair(candidatesArray[i], candidatesArray[j]);
            }
        }
    }

    void BuildCandidates()
    {
        int nPartitions = Bootstrap.Settings.NPartitions;

        for (int i = 0; i < nPartitions; i++)
        {
            for (int j = 0; j < nPartitions; j++)
            {
                int key = Util.GetHashCode(i, j);
                int value;
                NativeMultiHashMapIterator<int> iterator;
                if (!_gridSystem.Grid.TryGetFirstValue(key, out value, out iterator))
                {
                    continue;
                }

                List<int> candidates = new List<int>();
                do
                {
                    candidates.Add(value);
                } while (_gridSystem.Grid.TryGetNextValue(out value, ref iterator));
                AddCandidatePairs(candidates);
            }
        }
    }

    protected override void OnUpdate()
    {
        _candidatePairs.Clear();
        BuildCandidates();
    }
}

