using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Tilemaps;
using WFC_Tutorial;

namespace WFC_Tutorial
{
    public class TileBaseInputReader : IInputReader<TileBase>
    {
        private Tilemap inputTilemap;

        public TileBaseInputReader(Tilemap inputTilemap)
        {
            this.inputTilemap = inputTilemap;
        }

        private IValue<TileBase>[,] ReadInputTileMap()
        {
            return CreateTileBaseGrid();
        }

        private IValue<TileBase>[,] CreateTileBaseGrid()
        {
            throw new NotImplementedException();
        }

        public IValue<TileBase>[,] ReadInputToGrid()
        {
            IValue<TileBase>[,] grid = ReadInputTileMap();
            TileBaseValue[,] gridOfValues = null;

            if (grid is not null)
            {
                gridOfValues = new TileBaseValue[grid.LongLength, grid.Length];
                for (int row = 0; row < grid.Length; row++)
                {
                    for (int col = 0; col < gridOfValues.LongLength; col++)
                    {
                        gridOfValues[col, row] = new TileBaseValue(grid[row, col]);

                    }
                }
            }
            return gridOfValues;
        }
    }
}