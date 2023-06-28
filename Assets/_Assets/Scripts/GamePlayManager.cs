using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ilumisoft.Connect.Game;
public class GamePlayManager : MonoBehaviour
{
    public static GamePlayManager In;
    public GameObject gameManagerObj, spriteObj;
    
    private void Awake()
    {
        In = this;
        CreateGameManager();
    }
    public void Start()
    {
    }

    public void CreateGameManager()
    {
        GameObject tmpGameManagerObj = Instantiate(gameManagerObj, gameManagerObj.transform.position, gameManagerObj.transform.rotation) as GameObject;
        tmpGameManagerObj.name = gameManagerObj.name;
        //tmpGameManagerObj.transform.parent = UIManager.In.gameplayScreen.transform;
    }

    public void CreateSprite(Transform startTra, Transform targetTra, Sprite sprite, GameGrid gameGrid, int gridInd)
    {
        GameObject tmpspriteObj = Instantiate(spriteObj, startTra.position, Quaternion.identity) as GameObject;
        MoveToUI moveToUI = tmpspriteObj.GetComponent<MoveToUI>();
        moveToUI.gameGrid = gameGrid;
        moveToUI.gridInd = gridInd;
        tmpspriteObj.transform.parent = targetTra;
        tmpspriteObj.GetComponent<SpriteRenderer>().sprite = sprite;
        moveToUI.targetTra = targetTra;
    }
}
