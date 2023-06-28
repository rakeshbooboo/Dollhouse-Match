namespace Ilumisoft.Connect.Game
{
    using Ilumisoft.Connect;
    using Ilumisoft.Connect.Core;
    using System.Collections;
    using System.Collections.Generic;

    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Handles the game flow
    /// </summary>
    public class GameManager : SingletonBehaviour<GameManager>
    {
        [SerializeField]
        /// <summary>
        /// The score of the player
        /// </summary>
        public static int Score { get; private set; }

        /// <summary>
        /// Reference to the game grid
        /// </summary>
        [SerializeField] public GameGrid grid = null;

        /// <summary>
        /// The number of moves the player has left
        /// </summary>
        [SerializeField] private int movesAvailable = 20;
        public float targetMinNo, targetMaxNo;

        public int tmpLvlNo = 0;
        public List<LevelData> levelDatas;

        /// <summary>
        /// Gets or sets the  number of moves the player has left
        /// </summary>
        public int MovesAvailable
        {
            get => this.movesAvailable;
            set => this.movesAvailable = value;
        }

        /// <summary>
        /// Start listening to relevant events
        /// </summary>
        private void OnEnable()
        {
            GameEvents.OnElementsDespawned.AddListener(OnElementsDespawned);
        }

        //Stop listening from all events
        private void OnDisable()
        {
            GameEvents.OnElementsDespawned.RemoveListener(OnElementsDespawned);
        }

        /// <summary>
        /// Starts and processes the game flow
        /// </summary>
        /// <returns></returns>
        ///
        public static GameManager In;
        UIManager uiManager;

        private void Awake()
        {
            In = this;
            if (!PlayerPrefs.HasKey("LvlNo"))
            {
                PlayerPrefs.SetInt("LvlNo", 1);
            }

            for (int i=0; i<100; i++)
            {
                string tmpStr1 = "Progress" + i;
                if (!PlayerPrefs.HasKey(tmpStr1))
                {
                    PlayerPrefs.SetInt(tmpStr1, 0);
                }
            }
        }

        private IEnumerator Start()
        {
            uiManager = UIManager.In;
            tmpLvlNo = PlayerPrefs.GetInt("LvlNo")-1;

            Transform DollHouseTra = GameObject.Find("Levels").transform;

            for (int i=0; i< DollHouseTra.childCount; i++)
            {
                levelDatas.Add(DollHouseTra.GetChild(i).GetComponent<LevelData>());
            }

            for (int i=0; i< levelDatas.Count; i++)
            {
                if (i <= tmpLvlNo)
                {
                    levelDatas[i].gameObject.SetActive(true);
                    levelDatas[i].StartFunc();
                }
                else
                {
                    levelDatas[i].gameObject.SetActive(false);
                }
            }
            InitializeGame();
            LeanTween.moveY(gameObject, -2.75f, 1f).setEase(LeanTweenType.easeOutExpo);
            uiManager.GamePlayScreen();
            uiManager.UpdateProgressBar(levelDatas.Count);
            uiManager.itemObOnOff(levelDatas[tmpLvlNo].sprites.Count);
            //Wait for the game to be executed completely
            yield return StartCoroutine(RunGame());


            //Wait for the game to finish
            //yield return StartCoroutine(EndGame());
        }

        /// <summary>
        /// Returns to the menu scene
        /// </summary>
        protected void OnBackButtonClick()
        {
            SceneLoadingManager.Instance.LoadScene(SceneNames.Menu);
        }

        /// <summary>
        /// Check for escape button
        /// </summary>
        //private void Update()
        //{
        //    if (Input.GetKey(KeyCode.Escape))
        //    {
        //        OnBackButtonClick();
        //    }
        //}

        /// <summary>
        /// Initilaizes the game and the grid
        /// </summary>
        public void InitializeGame()
        {
            Score = 0;

            this.grid.SetUpGrid();
        }

        /// <summary>
        /// Runs the game loop
        /// </summary>
        /// <returns></returns>
        public IEnumerator RunGame()
        {
            //Game Loop
            while (this.MovesAvailable > 0)
            {
                //Wait for the Player to select elements
                yield return this.grid.WaitForSelection();

                //Despawn selected elements
                yield return this.grid.DespawnSelection();

                //Wait for the grid elements to finish movement
                yield return this.grid.WaitForMovement();

                //Respawn despawned elements
                yield return this.grid.RespawnElements();
            }
        }

        /// <summary>
        /// Loads the game over scene
        /// </summary>
        /// <returns></returns>
        public IEnumerator EndGame()
        {
            yield return new WaitForSeconds(0.5f);
            
            SceneLoadingManager.Instance.LoadScene(SceneNames.GameOver);
        }

        /// <summary>
        /// Gets invoked when the user has finished its move and 
        /// the selected elements are despawned
        /// </summary>
        /// <param name="count"></param>
        private void OnElementsDespawned(int count)
        {
            int oldScore = Score;
            Score = oldScore + count * (count - 1);

            //Invoke score changed event
            GameEvents.OnScoreChanged.Invoke(oldScore, Score);
        }

        public void LevelComplete()
        {
            StartCoroutine(LevelCompleteIEnum());
        }

        public IEnumerator LevelCompleteIEnum()
        {
            yield return new WaitForSeconds(2.5f);

            //levelDatas[tmpLvlNo].wallProgress.characterObj.GetComponent<Character>().FinishFunc();
            LeanTween.moveY(gameObject, -15.03f, 1f).setEase(LeanTweenType.easeOutBounce).setOnComplete(()=> {
                Destroy(gameObject);
            });
            uiManager.LevelCompleteScreen();
        }

        public void LevelFail()
        {
            StartCoroutine(LevelFailIEnum());
        }

        public IEnumerator LevelFailIEnum()
        {
            yield return new WaitForSeconds(1f);
            LeanTween.moveY(gameObject, -15.03f, 1f).setEase(LeanTweenType.easeOutBounce);
            uiManager.LevelFailScreen();
        }

        public void Add5Moves()
        {
            MovesAvailable = 5;
            Ilumisoft.Connect.Game.GameManager.In.grid.isFinish = false;
            UIManager.In.UpdateMoveTxt(GameManager.In.MovesAvailable);
            LeanTween.moveY(gameObject, -2.75f, 1f).setEase(LeanTweenType.easeOutExpo);
            uiManager.GamePlayScreen();
            StartCoroutine(RunGame());
        }
    }
}