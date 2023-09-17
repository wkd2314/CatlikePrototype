
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

namespace Minecomb
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
    struct PlaceMinesJob : IJob
    {
        public Grid grid;
        public int mines, seed;
        public void Execute()
        {

            for (int i = 0; i < grid.CellCount; i++)
            {
                grid[i] = CellState.Zero;
            }

            Random random = new Random((uint)seed);
            for (int m = 0; m < mines; m++)
            {
                grid[random.NextInt(grid.CellCount)] = CellState.Mine;
            }
        }
    }

}
