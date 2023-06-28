namespace Ilumisoft.Connect.Game
{
    using Ilumisoft.Connect.Core.Extensions;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Despawns and respawns elements in the game grid
    /// </summary>
    public class GameGridSpawning
    {
        private GameGrid grid;

        public GameGridSpawning(GameGrid grid)
        {
            this.grid = grid;
        }

        /// <summary>
        /// Respawns all despawned elements
        /// </summary>
        /// <returns></returns>
        public IEnumerator RespawnElements()
        {
            GameSFX.Instance.Play(GameSFX.Instance.SpawnClip);

            foreach (GameGridElement element in this.grid.Elements)
            {
                if (element.IsSpawned == false)
                {
                    element.sprite = this.grid.sprites.GetRandom();

                    element.Spawn();
                }
            }

            yield return new WaitForSeconds(0.4f);
        }

        /// <summary>
        /// Despawns the given list of elements
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        public IEnumerator Despawn(List<GameGridElement> elements)
        {
            bool checkSound = false;
            for (int i=0; i<grid.sprites.Count; i++)
            {
                if (elements[0].sprite == grid.sprites[i] && grid.spritesNo[i] > 0)
                {
                    //grid.spritesNo[i] -= elements.Count;
                    int tmpNo = grid.spritesNo[i] - elements.Count;
                    if (tmpNo <= 0)
                    {
                        for (int j = 0; j < elements.Count - Mathf.Abs(tmpNo); j++)
                        {
                            GamePlayManager.In.CreateSprite(elements[j].transform, UIManager.In.itemImgList[i].transform, elements[0].sprite, grid, i);
                            elements[j].CreateSpriteParticle();
                            checkSound = true;
                        }
                        grid.gameManager.levelDatas[grid.tmpLvlNo].CollectedTargetNo += elements.Count - Mathf.Abs(tmpNo);
                        tmpNo = 0;
                    }
                    else
                    {
                        for (int j = 0; j < elements.Count; j++)
                        {
                            GamePlayManager.In.CreateSprite(elements[j].transform, UIManager.In.itemImgList[i].transform, elements[0].sprite, grid, i);
                            elements[j].CreateSpriteParticle();
                            checkSound = true;
                        }
                        grid.gameManager.levelDatas[grid.tmpLvlNo].CollectedTargetNo += elements.Count;
                    }
                    break;
                }
            }

            if (checkSound)
            {
                GameSFX.Instance.Play(GameSFX.Instance.DespawnClip);
            }
            else
            {
                GameSFX.Instance.Play(GameSFX.Instance.DespawnClip2);
            }

            foreach (GameGridElement gridElement in elements)
            {
                gridElement.Despawn();
            }

            yield return new WaitForSeconds(0.4f);

            grid.gameManager.MovesAvailable--;
            UIManager.In.UpdateMoveTxt(GameManager.In.MovesAvailable);
            GameEvents.OnElementsDespawned.Invoke(elements.Count);
            grid.UpdateAllTargetValue();

            //if (grid.gameManager.levelDatas[grid.tmpLvlNo].CollectedTargetNo >= grid.gameManager.levelDatas[grid.tmpLvlNo].totalTargetNo)
            //{
            //    grid.gameManager.LevelComplete();
            //    Debug.Log("Level Complete.....");
            //}
            //else
            //if(grid.gameManager.MovesAvailable <= 0)
            //{
            //    grid.gameManager.LevelFail();
            //    Debug.Log("Level Fail.....");
            //}
        }
    }
}