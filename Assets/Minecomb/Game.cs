using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Minecomb
{
    public class Game : MonoBehaviour
    {
        [SerializeField] private TextMeshPro minesText;

        [SerializeField, Min(1)] private int rows = 8, columns = 21, mines = 30;
        [SerializeField] private Material material;
        [SerializeField] private Mesh mesh;

        private Grid grid;

        private GridVisualization visualization;

        int markedSureCount;

        private void OnEnable()
        {
            grid.Initialize(rows, columns);
            visualization.Initialize(grid, material, mesh);
            mines = Mathf.Min(mines, grid.CellCount);
            minesText.SetText("{0}", mines);
            markedSureCount = 0;

            grid.PlaceMines(mines);
            visualization.Update();
        }

        private void OnDisable()
        {
            grid.Dispose();
            visualization.Dispose();
        }

        private void Update()
        {
            // so we can immediately see these changes while in play mode.
            if (grid.Rows != rows || grid.Columns != columns)
            {
                OnDisable();
                OnEnable();
            }
            if (PerformAction())
            {
                visualization.Update();
            }
            visualization.DrawObsolete();
        }

        bool PerformAction()
        {
            bool revealAction = Input.GetMouseButtonDown(0);
            bool markAction = Input.GetMouseButtonDown(1);
            if (
                (revealAction || markAction) &&
                visualization.TryGetHitCellIndex(
                    Camera.main.ScreenPointToRay(Input.mousePosition), out int cellIndex
                )
            )
            {
                return revealAction ? DoRevealAction(cellIndex) : DoMarkAction(cellIndex);
            }

            return false;
        }

        bool DoMarkAction(int cellIndex)
        {
            CellState state = grid[cellIndex];
            if (state.Is(CellState.Revealed))
            {
                return false;
            }

            if (state.IsNot(CellState.Marked))
            {
                grid[cellIndex] = state.With(CellState.MarkedSure);
                markedSureCount += 1;
            }
            else if (state.Is(CellState.MarkedSure))
            {
                grid[cellIndex] =
                    state.Without(CellState.MarkedSure).With(CellState.MarkedUnsure);
                markedSureCount -= 1;
            }
            else
            {
                grid[cellIndex] = state.Without(CellState.MarkedUnsure);
            }

            minesText.SetText("{0}", mines - markedSureCount);
            return true;
        }

        bool DoRevealAction(int cellIndex)
        {
            CellState state = grid[cellIndex];
            if (state.Is(CellState.MarkedOrRevealed))
            {
                return false;
            }

            grid[cellIndex] = state.With(CellState.Revealed);
            return true;
        }
    }
}

