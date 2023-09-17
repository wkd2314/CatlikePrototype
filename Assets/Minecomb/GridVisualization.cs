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
            rowsPerCell = 7,
            columnsPerCell = 5,
            blocksPerCell = rowsPerCell * columnsPerCell;

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
                positions = positions,
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

        public bool TryGetHitCellIndex(Ray ray, out int cellIndex)
        {
            Vector3 p = ray.origin - ray.direction * (ray.origin.y / ray.direction.y);

            float x = p.x + columnsPerCell / 2 + 0.5f;
            x /= columnsPerCell + 1;
            x += (grid.Columns - 1) * 0.5f;
            int c = Mathf.FloorToInt(x);

            float z = p.z + rowsPerCell / 2f + 0.5f;
            z /= rowsPerCell + 1;
            z += (grid.Rows - 1) * 0.5f + (c & 1) * 0.5f - 0.25f;
            int r = Mathf.FloorToInt(z);

            return grid.TryGetCellIndex(r, c, out cellIndex) &&
            x - c <= 1f - 1f / (columnsPerCell + 1) &&
            z - r <= 1f - 1f / (rowsPerCell + 1);
        }

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
