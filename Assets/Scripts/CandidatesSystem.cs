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

[UpdateAfter(typeof(GridSystem))]
public class CandidatesSystem : ComponentSystem
{
    HashSet<int2> _candidatePairs;
    [Inject] GridSystem _gridSystem;

    public int2[] CandidatePairs => _candidatePairs.ToArray();

    class PointComparer : IEqualityComparer<int2> {
        public bool Equals(int2 a, int2 b) {
            return a.x == b.x && a.y == b.y;
        }

        public int GetHashCode(int2 obj) {
            // Perfect hash for practical bitmaps, their width/height is never >= 65536
            return (obj.y << 16) ^ obj.x;
        }
    }

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
                // collect all candidates
                SortedSet<int> allCandidates = new SortedSet<int>();
                for (int m = -1; m <= 1; m++)
                {
                    int xGridPos = i + m;
                    if (xGridPos < 0 || xGridPos > nPartitions - 1)
                    {
                        continue;
                    }
                    for (int n = -1; n <= 1; n++)
                    {
                        int yGridPos = j + n;
                        if (yGridPos < 0 || yGridPos > nPartitions - 1)
                        {
                            continue;
                        }

                        List<int> candidates = _gridSystem.Grid[xGridPos][yGridPos];
                        foreach (int entry in candidates)
                        {
                            allCandidates.Add(entry);
                        }
                    }
                }

                AddCandidatePairs(allCandidates.ToArray());
            }
        }
    }

    protected override void OnUpdate()
    {
        _candidatePairs.Clear();
        BuildCandidates();
    }
}

