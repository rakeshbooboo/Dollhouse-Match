namespace Ilumisoft.Connect.Game
{
    using Ilumisoft.Connect.Core.Extensions;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The grid holding all elements/bubbles
    /// </summary>
    ///
    public class GameGrid : MonoBehaviour
    {
        [SerializeField] private SelectionLine selectionLine = null;
        [SerializeField] private GameGridElement gridElementPrefab = null;

        [SerializeField] private int rowCount = 8;
        [SerializeField] private int columnCount = 8;
        [SerializeField] private float cellSize = 1.0f;

        [SerializeField] private List<Color> colors = new List<Color>();
        [SerializeField] private List<Sprite> sprite = new List<Sprite>();
        [SerializeField] private List<int> spriteNo = new List<int>();

        private GameGridInput gridInput;
        private GameGridMovement gridMovement;
        private GameGridSpawning gridSpawning;

        /// <summary>
        /// Gets the list of all elements in the grid
        /// </summary>
        public List<GameGridElement> Elements = new List<GameGridElement>();

        /// <summary>
        /// Gets the list of available colors
        /// </summary>
        public List<Color> Colors => this.colors;
        public List<Sprite> sprites => this.sprite;
        public List<int> spritesNo => this.spriteNo;

        /// <summary>
        /// Returns the center (worldposition) of the most left cell on the bottom 
        /// </summary>
        public Vector2 StartCellPosition => this.GridCenter + Vector2.up * 0.5f * this.cellSize * (this.rowCount - 1) + Vector2.left * 0.5f * this.cellSize * (this.columnCount - 1);

        /// <summary>
        /// Returns the size of a single cell
        /// </summary>
        public float CellSize => this.cellSize;

        /// <summary>
        /// Returns the number of rows (=vertical length)
        /// </summary>
        public int RowCount => this.rowCount;

        /// <summary>
        /// Returns the number of columns (=horizontal length)
        /// </summary>
        public int ColumnCount => this.columnCount;

        /// <summary>
        /// Returns the center poistion of the grid
        /// </summary>
        public Vector2 GridCenter => this.transform.position;

        /// <summary>
        /// Returns the Transform containing all grid elements 
        /// </summary>
        public Transform GridContainer => this.transform;
        UIManager uiManager;
        public GameManager gameManager;
        internal int tmpLvlNo;

        public List<GameObject> elementObjList;

        private void Awake()
        {
            this.gridInput = new GameGridInput(this, this.selectionLine);
            this.gridMovement = new GameGridMovement(this);
            this.gridSpawning = new GameGridSpawning(this);

            float aspectRatioVal = 1242f / CellSize;
        }

        /// <summary>
        /// Creates all elements of the grid and sets them up
        /// </summary>
        public void SetUpGrid()
        {
            uiManager = UIManager.In;
            tmpLvlNo = PlayerPrefs.GetInt("LvlNo")-1;
            gameManager.MovesAvailable = gameManager.levelDatas[tmpLvlNo].MovesAvailable;
            sprites.AddRange(gameManager.levelDatas[tmpLvlNo].sprites);
            spritesNo.AddRange(gameManager.levelDatas[tmpLvlNo].spritesNo);
            for (int x = 0; x < this.RowCount; x++)
            {
                for (int y = 0; y < this.ColumnCount; y++)
                {
                    GameGridElement element = Instantiate(this.gridElementPrefab);

                    element.transform.localScale = Vector2.one * this.cellSize;

                    element.transform.position = this.GridToWorldPosition(x, y);

                    element.transform.SetParent(this.GridContainer.transform, true);

                    //element.Color = this.colors.GetRandom();
                    element.sprite = this.sprite.GetRandom();
                    element.endScaleMultiply = cellSize;
                    elementObjList.Add(element.gameObject);
                    this.Elements.Add(element);
                }
            }

            uiManager = UIManager.In;

            for (int i = 0; i < spritesNo.Count; i++)
            {
                if (i < sprite.Count)
                {
                    uiManager.itemObjList[i].SetActive(true);
                    uiManager.itemImgList[i].sprite = sprites[i];
                    uiManager.itemTxtList[i].text = spritesNo[i].ToString();
                }
                else
                {
                    uiManager.itemObjList[i].SetActive(false);
                }
            }

            uiManager.UpdateMoveTxt(gameManager.MovesAvailable);
            //for (int i=0; i<this.Elements.Count; i++) {
            //    gridInput.IsSelectable(this.Elements[i]);
            //}

            //gridInput.IsSelectable(this.Elements[0]);

            //Elements
        }
        internal bool isFinish = false;
        public void UpdateAllTargetValue()
        {
            for (int i = 0; i < spritesNo.Count; i++)
            {
                uiManager.itemTxtList[i].text = spritesNo[i].ToString();
            }
            GameManager gameManager = Ilumisoft.Connect.Game.GameManager.In;
            
            if (isFinish == false && gameManager.levelDatas[gameManager.tmpLvlNo].CollectedTargetNo >= gameManager.levelDatas[gameManager.tmpLvlNo].totalTargetNo)
            {
                isFinish = true;
                for (int i = 0; i < gameManager.levelDatas[gameManager.tmpLvlNo].wallProgressList.Count; i++)
                {
                    //wallProgressList[i].totalProgress += (1f / (float)wallProgressList[i].progressPerLvl) + wallProgressList[i].currProgress;
                    gameManager.levelDatas[gameManager.tmpLvlNo].wallProgressList[i].totalProgress += (1f / (float)gameManager.levelDatas[gameManager.tmpLvlNo].wallProgressList[i].progressPerLvl);
                }
                gameManager.LevelComplete();
                Debug.Log("Level Complete.....");
            }
            else if (isFinish == false && gameManager.MovesAvailable <= 0)
            {
                isFinish = true;
                gameManager.LevelFail();
                Debug.Log("Level Fail.....");
            }
        }

        /// <summary>
        /// Returns a coroutine waiting until all grid elements are not moving
        /// </summary>
        /// <returns></returns>
        public Coroutine WaitForMovement()
        {
            return StartCoroutine(this.gridMovement.WaitForMovement());
        }

        /// <summary>
        /// Returns a coroutine waiting until the user has made a selection
        /// </summary>
        /// <returns></returns>
        public Coroutine WaitForSelection()
        {
            return StartCoroutine(this.gridInput.WaitForSelection());
        }

        /// <summary>
        /// Returns a coroutine respawning all despawned elements and waiting until they are
        /// finished
        /// </summary>
        /// <returns></returns>
        public Coroutine RespawnElements()
        {
            return StartCoroutine(this.gridSpawning.RespawnElements());
        }

        /// <summary>
        /// Returns a coroutine despawning the selected elements and waiting until they are finished
        /// </summary>
        /// <returns></returns>
        public Coroutine DespawnSelection()
        {
            return StartCoroutine(this.gridSpawning.Despawn(this.gridInput.SelectedElements));
        }
    }
}