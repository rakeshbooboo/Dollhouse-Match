using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ilumisoft.Connect.Game;
using BezierSolution;
[System.Serializable]
public struct moves
{
    public List<int> validMovesIndList;
}
public class Tutorial : MonoBehaviour
{
    GameManager gameManager;
    public LineRenderer lineRenderer;
    public List<GameObject> validObjList;
    public SpriteRenderer handSpriteRenderer;
    public Sprite handNormal, handTap;
    public Transform bezierSplineTra;
    public BezierWalkerWithSpeed bezierWalkerWithSpeed;
    public float timeVal, targetTime = 0;
    public bool isTutorial = false;

    void Start()
    {
        StartCoroutine(CreateHintsIEnum(1f));
        
    }

    public void LateUpdate()
    {
        if (isTutorial == false)
        {
            if (timeVal >= targetTime)
            {
                timeVal = 0;
                StartCoroutine(CreateHintsIEnum(0f));
            }
            else
            {
                timeVal += Time.deltaTime;
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            isTutorial = true;
            timeVal = 0;
            StartCoroutine(HandOffIEnum());
        }
        if (Input.GetMouseButtonUp(0))
        {
            isTutorial = false;
        }
    }

    IEnumerator CreateHintsIEnum(float timeNo)
    {
        gameManager = GameManager.In;
        isTutorial = true;
        yield return new WaitForSeconds(timeNo);
        validObjList.Clear();
        validObjList = new List<GameObject>();
        yield return new WaitForSeconds(0.05f);

        List<GameGridElement> newGridElementList = new List<GameGridElement> ();
        newGridElementList.AddRange(gameManager.grid.Elements);

        int randomNo = Random.Range(0, newGridElementList.Count);
        for (int k = 0; k < newGridElementList.Count; k++)
        {
            if (newGridElementList[randomNo].spriteRenderer != null && newGridElementList[k].spriteRenderer != null)
            {
                if (newGridElementList[randomNo].spriteRenderer.sprite == newGridElementList[k].spriteRenderer.sprite)
                {
                    if (Vector3.Distance(newGridElementList[randomNo].transform.position, newGridElementList[k].transform.position) <= 1.0f)
                    {
                        randomNo = k;
                        //newGridElementList[k].spriteRenderer.color = Color.green;
                        validObjList.Add(newGridElementList[k].gameObject);
                    }
                }
            }
            //else
            //{
            //    newGridElementList[k].spriteRenderer.color = Color.white;
            //}
        }

        if (validObjList.Count < 2)
        {
            StartCoroutine(CreateHintsIEnum(0));
        }
        else
        {
            lineRenderer.positionCount = validObjList.Count;
            handSpriteRenderer.transform.parent.position = validObjList[0].transform.position;
            handSpriteRenderer.enabled = true;
            yield return new WaitForEndOfFrame();
            for (int i=0; i< validObjList.Count; i++)
            {
                lineRenderer.SetPosition(i, validObjList[i].transform.position);
                GameObject tmpObj = new GameObject();
                tmpObj.transform.parent = bezierSplineTra;
                tmpObj.transform.position = validObjList[i].transform.position;
                tmpObj.AddComponent<BezierPoint>();
                BezierPoint bezierPoint = tmpObj.GetComponent<BezierPoint>();

                if (i == validObjList.Count-1)
                {
                    if (validObjList[i].transform.position.x >= validObjList[i-1].transform.position.x-0.1f &&
                        validObjList[i].transform.position.x <= validObjList[i - 1].transform.position.x + 0.1f)
                    {
                        bezierPoint.precedingControlPointLocalPosition = new Vector3(0.05f, 0.005f, 0.005f);
                        bezierPoint.followingControlPointLocalPosition = new Vector3(0.05f, 0.005f, 0.005f);
                    }
                    else
                    {
                        bezierPoint.precedingControlPointLocalPosition = new Vector3(0.005f, 0.05f, 0.005f);
                        bezierPoint.followingControlPointLocalPosition = new Vector3(0.005f, 0.05f, 0.005f);
                    }
                }
                else
                {
                    if (validObjList[i].transform.position.x >= validObjList[i + 1].transform.position.x - 0.1f &&
                        validObjList[i].transform.position.x <= validObjList[i + 1].transform.position.x + 0.1f)
                    {
                        bezierPoint.precedingControlPointLocalPosition = new Vector3(0.05f, 0.005f, 0.005f);
                        bezierPoint.followingControlPointLocalPosition = new Vector3(0.05f, 0.005f, 0.005f);
                    }
                    else
                    {
                        bezierPoint.precedingControlPointLocalPosition = new Vector3(0.005f, 0.05f, 0.005f);
                        bezierPoint.followingControlPointLocalPosition = new Vector3(0.005f, 0.05f, 0.005f);
                    }
                }

                tmpObj.layer = 5;
            }

            bezierSplineTra.gameObject.AddComponent<BezierSpline>();
            bezierWalkerWithSpeed.spline = bezierSplineTra.gameObject.GetComponent<BezierSpline>();
            bezierWalkerWithSpeed.enabled = true;
            print("Searching Finish...");
        }
    }

    public IEnumerator HandOffIEnum()
    {
        bezierWalkerWithSpeed.enabled = false;
        handSpriteRenderer.enabled = false;
        lineRenderer.positionCount = 0;

        yield return new WaitForEndOfFrame();

        if (bezierSplineTra.GetComponent<BezierSpline>())
        {
            Destroy(bezierSplineTra.GetComponent<BezierSpline>());
        }

        yield return new WaitForEndOfFrame();

        int tmpInd = bezierSplineTra.childCount;
        print(tmpInd);
        for (int i = 0; i < tmpInd; i++)
        {
            Destroy(bezierSplineTra.GetChild(i).gameObject);
        }
    }
}
