﻿// -----------------------------------------------------------------------
// <copyright file="ChunkEvaluator.cs" company="Asynkron AB">
//      Copyright (C) 2015-2024 Asynkron AB All rights reserved
// </copyright>
// -----------------------------------------------------------------------

using System.Collections;

namespace Proto.Cluster.Partition;

internal class IndexSet
{
    private readonly BitArray _received = new(64);

    public int Count { get; private set; }

    public int ReceivedMax { get; private set; }

    /// <summary>
    ///     Checks if the set is complete from 1 to the highest received index;
    /// </summary>
    public bool IsCompleteSet => ReceivedMax == Count;

    /// <summary>
    ///     Tries to add an index, returns true if it has not been added before, false otherwise
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool TryAddIndex(int index)
    {
        if (_received.Length <= index)
        {
            _received.Length = index * 2;
        }

        if (_received.Get(index))
        {
            return false;
        }

        _received.Set(index, true);
        Count++;

        if (index > ReceivedMax)
        {
            ReceivedMax = index;
        }

        return true;
    }
}