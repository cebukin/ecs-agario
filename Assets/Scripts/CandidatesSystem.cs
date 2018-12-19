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
    List<List<NativeArray<int>>> _candidates;
    [Inject] GridSystem _gridSystem;

    public NativeArray<int> GetCandidates(Position pos)
    {
        int x = (int) pos.Value.x; // TODO
        int y = (int) pos.Value.y; // TODO

        return _candidates[x][y];
    }

    void Init()
    {
        if (_candidates != null)
        {
            return;
        }

        _candidates = new List<List<NativeArray<int>>>();
        for (int i = 0; i < Bootstrap.Settings.NPartitions; i++)
        {
            List<NativeArray<int>> row = new List<NativeArray<int>>();
            for (int j = 0; j < Bootstrap.Settings.NPartitions; j++)
            {
                row.Add(new NativeArray<int>(1, Allocator.Persistent));
            }
            _candidates.Add(row);
        }
    }

    protected override void OnDestroyManager()
    {
        CleanUp();
    }

    void CleanUp()
    {
        foreach (var row in _candidates)
        {
            foreach (var entry in row)
            {
                entry.Dispose();
            }
            row.Clear();
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

                if (_candidates[i][j].Length != allCandidates.Count)
                {
                    _candidates[i][j].Dispose();
                    _candidates[i][j] = new NativeArray<int>(allCandidates.Count, Allocator.Persistent);
                }

                _candidates[i][j].CopyFrom(allCandidates.ToArray());
            }
        }
    }

    protected override void OnUpdate()
    {
        Init();
        BuildCandidates();
    }
}

