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
        private bool isGameOver;

        private void OnEnable()
        {
            grid.Initialize(rows, columns);
            visualization.Initialize(grid, material, mesh);
            StartNewGame();
        }

        void StartNewGame()
        {
            isGameOver = false;
            mines = Mathf.Min(mines, grid.CellCount);
            minesText.SetText("{0}", mines);
            markedSureCount = 0;
            grid.PlaceMines(mines);
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
            PerformAction();
            visualization.DrawObsolete();
        }

        void PerformAction()
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
                if (isGameOver)
                {
                    StartNewGame();
                }
                if (revealAction)
                {
                    DoRevealAction(cellIndex);
                }
                else
                {
                    DoMarkAction(cellIndex);
                }
            }
        }

        void DoMarkAction(int cellIndex)
        {
            CellState state = grid[cellIndex];
            if (state.Is(CellState.Revealed))
            {
                return;
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
        }

        void DoRevealAction(int cellIndex)
        {
            CellState state = grid[cellIndex];
            if (state.Is(CellState.MarkedOrRevealed))
            {
                return;
            }
            grid.Reveal(cellIndex);

            if (state.Is(CellState.Mine))
            {
                isGameOver = true;
                minesText.SetText("FAILURE");
                grid.RevealMinesAndMistakes();
            }
            else if (grid.HiddenCellCount == mines)
            {
                isGameOver = true;
                minesText.SetText("SUCCESS");
            }
        }
    }
}

