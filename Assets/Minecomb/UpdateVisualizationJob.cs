using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;

namespace Minecomb
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
    struct UpdateVisualizationJob : IJobFor
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<float3> positions, colors;

        [ReadOnly]
        public Grid grid;
        
        readonly static ulong[] bitmaps =
        {
            0b00000_01110_01010_01010_01010_01110_00000, // 0
            0b00000_00100_00110_00100_00100_01110_00000, // 1
            0b00000_01110_01000_01110_00010_01110_00000, // 2
            0b00000_01110_01000_01110_01000_01110_00000, // 3
            0b00000_01010_01010_01110_01000_01000_00000, // 4
            0b00000_01110_00010_01110_01000_01110_00000, // 5
            0b00000_01110_00010_01110_01010_01110_00000  // 6
        };

        public void Execute (int i)
        {
            int blockOffset = i * GridVisualization.blocksPerCell;
		
            for (int bi = 0; bi < GridVisualization.blocksPerCell; bi++)
            {
                float3 position = positions[blockOffset + bi];
                position.y = bi / (float)GridVisualization.blocksPerCell;
                positions[blockOffset + bi] = position;
                colors[blockOffset + bi] = position.y;
            }
        }
    }
}
