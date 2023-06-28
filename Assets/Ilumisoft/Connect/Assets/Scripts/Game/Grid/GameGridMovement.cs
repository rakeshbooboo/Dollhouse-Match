﻿namespace Ilumisoft.Connect.Game
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Performs the animated movement of elements in the game grid
    /// </summary>
    public class GameGridMovement
    {
        private GameGrid grid;

        public GameGridMovement(GameGrid grid)
        {
            this.grid = grid;
        }

        /// <summary>
        /// Moves all elements downwards if there are empty cells below them and 
        /// waits for their movement to be finished
        /// </summary>
        /// <returns></returns>
        public IEnumerator WaitForMovement()
        {
            MoveElements();

            while (IsMovementDone() == false)
            {
                yield return new WaitForSeconds(0.05f);
            }

            yield break;
        }

        /// <summary>
        /// Returns true if no element is moving, false otherwise
        /// </summary>
        /// <returns></returns>
        private bool IsMovementDone()
        {
            foreach (GameGridElement element in this.grid.Elements)
            {
                if (element.IsSpawned && element.IsMoving)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Moves elements downwards to fill gaps between cells 
        /// </summary>
        private void MoveElements()
        {
            //Run from top to bottom through all rows
            for (int x = this.grid.RowCount - 1; x >= 0; x--)
            {
                for (int y = 0; y < this.grid.ColumnCount; y++)
                {
                    ProcessCell(x, y);
                }
            }
        }

        /// <summary>
        /// Checks for the cell in the given row and column, whether it is not spawned.
        /// If thats the case, the next spawned element above it, will move to its position.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        private void ProcessCell(int row, int column)
        {
            GameGridElement element = this.grid.GetElement(row, column);

            //Is the element not spawned? 
            //=> Cell is empty and elements above should move downwards
            if (element.IsSpawned == false)
            {
                //Move the next element above it down
                for (int i = row - 1; i >= 0; i--)
                {
                    GameGridElement next = this.grid.GetElement(i, column);

                    if (next != null && next.IsSpawned && !next.IsMoving)
                    {
                        MoveElement(new Vector2Int(i, column), new Vector2Int(row, column));
                        return;
                    }
                }
            }
        }

        private void MoveElement(Vector2Int oldPos, Vector2Int newPos)
        {
            //Catch the elements of the two grid positions
            GameGridElement element1 = this.grid.GetElement(oldPos.x, oldPos.y);
            GameGridElement element2 = this.grid.GetElement(newPos.x, newPos.y);

            //Switch them in the grid
            this.grid.Elements[this.grid.GridPositionToIndex(oldPos.x,oldPos.y)] = element2;
            this.grid.Elements[this.grid.GridPositionToIndex(newPos.x,newPos.y)] = element1;

            //Update world positions
            element2.transform.position = this.grid.GridToWorldPosition(oldPos);
            element1.Move(this.grid.GridToWorldPosition(newPos), 0.4f);
        }
    }
}