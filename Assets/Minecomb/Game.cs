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

        [SerializeField, Min(1)] private int rows = 8, columns = 21;
        [SerializeField] private Material material;
        [SerializeField] private Mesh mesh;

        private Grid grid;

        private GridVisualization visualization;

        private void OnEnable()
        {
            grid.Initialize(rows, columns);
            visualization.Initialize(grid, material, mesh);
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
            visualization.Update();
            visualization.DrawObsolete();
        }
    }
}

