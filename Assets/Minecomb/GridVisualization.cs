using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.Jobs;
using Unity.VisualScripting;
using static Unity.Mathematics.math;

namespace Minecomb
{
    public struct GridVisualization
    {
        private static int
            positionsId = Shader.PropertyToID("_Positions"),
            colorsId = Shader.PropertyToID("_Colors");

        private ComputeBuffer positionsBuffer, colorsBuffer;
        private NativeArray<float3> positions, colors;

        private Grid grid;
        private Material material;
        private Mesh mesh;

        public const int
            blockRowsPerCell = 7,
            blockColumnsPerCell = 5,
            blocksPerCell = blockRowsPerCell * blockColumnsPerCell;

        public void Initialize(Grid grid, Material material, Mesh mesh)
        {
            this.grid = grid;
            this.material = material;
            this.mesh = mesh;

            int instanceCount = grid.CellCount * blocksPerCell;
            positions = new NativeArray<float3>(instanceCount, Allocator.Persistent);
            colors = new NativeArray<float3>(instanceCount, Allocator.Persistent);

            positionsBuffer = new ComputeBuffer(instanceCount, 3 * 4);
            colorsBuffer = new ComputeBuffer(instanceCount, 3 * 4);
            material.SetBuffer(positionsId, positionsBuffer);
            material.SetBuffer(colorsId, colorsBuffer);
            
            new InitializeVisualizationJob
            {
                positions =  positions,
                colors = colors,
                rows = grid.Rows,
                columns = grid.Columns
            }.ScheduleParallel(grid.CellCount, grid.Columns, default).Complete();
            
            positionsBuffer.SetData(positions);
            colorsBuffer.SetData(colors);
        }

        public void Dispose()
        {
            positions.Dispose();
            colors.Dispose();
            positionsBuffer.Release();
            colorsBuffer.Release();
        }

        public void DrawObsolete() => Graphics.DrawMeshInstancedProcedural(
            mesh, 0, material, new Bounds(Vector3.zero, Vector3.one), positionsBuffer.count
        );

        public void Draw() => Graphics.RenderMeshPrimitives(
            new RenderParams(material)
            {
                worldBounds = new Bounds(Vector3.zero, Vector3.one)
            }, 
            mesh, 0, positionsBuffer.count
        );

        public void Update()
        {
            new UpdateVisualizationJob
            {
                positions = positions,
                colors = colors,
                grid = grid
            }.ScheduleParallel(grid.CellCount, grid.Columns, default).Complete();
            positionsBuffer.SetData(positions);
            colorsBuffer.SetData(colors);
        }
    }
}
