using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;

namespace Minecomb
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
    struct RevealMinesAndMistakesJob : IJobFor
    {
        public Grid grid;

        public void Execute(int i) => grid[i] = grid[i].With(
            grid[i].Is(CellState.MarkedSureOrMine) ? 
                CellState.Revealed : CellState.Zero
        );
    }
}

