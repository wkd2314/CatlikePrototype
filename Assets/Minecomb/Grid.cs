using System.Collections;
using System.Collections.Generic;
using Minecomb;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Minecomb
{
    [System.Flags]
    public enum CellState
    {
        Zero, One, Two, Three, Four, Five, Six,
        Mine = 1 << 3,
        MarkedSure = 1 << 4,
        MarkedUnsure = 1 << 5,
        Revealed = 1 << 6,
        Marked = MarkedSure | MarkedUnsure,
        MarkedOrRevealed = Marked | Revealed,
        MarkedSureOrMine = MarkedSure | Mine,
    }
    public static class CellStateExtensionMethods
    {
        public static bool Is(this CellState s, CellState mask) => (s & mask) != 0;
        public static bool IsNot(this CellState s, CellState mask) => (s & mask) == 0;
        public static CellState With(this CellState s, CellState mask) => s | mask;
        public static CellState Without(this CellState s, CellState mask) => s & ~mask;
    }
    public struct Grid
    {
        public int Rows { get; private set; }
        public int Columns { get; private set; }

        public int CellCount => states.Length;
        NativeArray<CellState> states;

        public void Initialize(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            states = new NativeArray<CellState>(Rows * Columns, Allocator.Persistent);
        }

        public void Dispose() => states.Dispose();

        public CellState this[int i]
        {
            get => states[i];
            set => states[i] = value;
        }

        public int GetCellIndex(int row, int column) => row * Columns + column;

        public bool TryGetCellIndex(int row, int column, out int index)
        {
            bool valid = 0 <= row && row < Rows && 0 <= column && column < Columns;
            index = valid ? GetCellIndex(row, column) : -1;
            return valid;
        }

        public void GetRowColumn(int index, out int row, out int column)
        {
            row = index / Columns;
            column = index - row * Columns;
        }

        public void PlaceMines(int mines) => new PlaceMinesJob
        {
            grid = this,
            mines = mines,
            seed = Random.Range(1, int.MaxValue)
        }.Schedule().Complete();
    }
}