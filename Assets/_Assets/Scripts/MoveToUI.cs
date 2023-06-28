using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ilumisoft.Connect.Game;
public class MoveToUI : MonoBehaviour
{
    public SpriteRenderer sprite;
    public Color startColor, endColor;
    public Transform targetTra;
    public int tmpInd;
    public GameGrid gameGrid;
    public int gridInd = 0;
    void Start()
    {
        transform.localScale = new Vector3(110f, 110f, 110f);
        float delayTime = Random.Range(0f, 0.5f);
        LeanTween.value(gameObject, startColor, endColor, 1f).setEase(LeanTweenType.easeInExpo).setOnUpdate((Color val) =>
        {
            sprite.color = val;
        }).setDelay(delayTime).setOnComplete(()=> {
            gameGrid.spritesNo[gridInd] -= 1;
            if (gameGrid.spritesNo[gridInd] <= 0)
            {
                gameGrid.spritesNo[gridInd] = 0;
            }
            gameGrid.UpdateAllTargetValue();
            Destroy(gameObject);
        });

        LeanTween.moveLocal(gameObject, Vector3.zero, 2f).setEase(LeanTweenType.easeOutExpo).setDelay(delayTime);
    }
}
