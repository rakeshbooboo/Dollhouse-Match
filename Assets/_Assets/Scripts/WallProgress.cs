using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallProgress : MonoBehaviour
{
    public int startLvl, endLvl;
    public float progressPerLvl = 0;
    public float currProgress = 0;
    public float totalProgress = 0;
    public List<GameObject> buildObjList;
    public int buildObjNo = 0;

    public Material normalMat;
    internal Material dissolveMat;
    
    internal GameObject tmpObj;
    internal GameObject characterObj;
    bool isFinish = false;
    public IEnumerator ProgressIEnum(float progress)
    {
        progress = totalProgress + (progress / (float)progressPerLvl);
        
        yield return new WaitForEndOfFrame();
        if (dissolveMat.HasFloat("_Dissolve"))
        {
            LeanTween.cancel(tmpObj);
            LeanTween.value(tmpObj, dissolveMat.GetFloat("_Dissolve"), Mathf.Clamp(progress, 0f, 1f), 0.6f).setEase(LeanTweenType.easeOutSine).setOnUpdate((float val) =>
            {
                dissolveMat.SetFloat("_Dissolve", val);
            }).setOnComplete(() =>
            {
                if (progress < 1)
                {
                    currProgress = progress;
                    progress = 0;
                }
                else if (progress >= 1f)
                {
                    progress -= 1;
                    buildObjList[buildObjNo].GetComponent<MeshRenderer>().material = normalMat;
                    buildObjNo += 1;
                    if (buildObjNo >= buildObjList.Count - 1)
                    {
                        buildObjNo = buildObjList.Count - 1;
                    }
                    dissolveMat = buildObjList[buildObjNo].GetComponent<MeshRenderer>().material;
                    tmpObj = buildObjList[buildObjNo];
                }
                if (progress > 0)
                {
                    StartCoroutine(ProgressIEnum(progress));
                }
            });
        }
        if (totalProgress >= 1f && characterObj != null && isFinish == false)
        {
            isFinish = true;
            characterObj.GetComponent<Character>().FinishFunc();
        }
    }

    public void StartProgress(float progress)
    {
        float tmpVal = (float)buildObjList.Count * progress;
        buildObjNo = (int)tmpVal;
        for (int i=0; i< buildObjList.Count; i++)
        {
            if (tmpVal < 1f)
            {
                dissolveMat = buildObjList[i].GetComponent<MeshRenderer>().material;
                dissolveMat.SetFloat("_Dissolve", Mathf.Clamp(tmpVal, 0f, 1f));
                tmpObj = buildObjList[i];
                tmpVal = 0;
            }
            if (tmpVal >= 1f)
            {
                tmpVal -= 1;
                buildObjList[i].GetComponent<MeshRenderer>().material = normalMat;
            }
        }
    }

    private void Awake()
    {
        progressPerLvl = (endLvl - startLvl) + 1;
    }

    private void Start()
    {
        int lvlDiff = (PlayerPrefs.GetInt("LvlNo") - startLvl);
        if (lvlDiff <= 0)
        {
            totalProgress = 0;
        }
        else
        {
            totalProgress = (1f / (float)progressPerLvl) * lvlDiff;
        }
        //currProgress = totalProgress;

        characterObj = transform.GetChild(transform.childCount - 1).gameObject;
        dissolveMat = buildObjList[0].GetComponent<MeshRenderer>().material;
        tmpObj = buildObjList[0];
        if (totalProgress > 0)
        {
            StartProgress(totalProgress);
        }
    }

    //private void OnValidate()
    //{
    //    buildObjList.Clear();
    //    for (int i = 0; i < transform.childCount - 1; i++)
    //    {
    //        buildObjList.Add(transform.GetChild(i).gameObject);
    //    }
    //}
}
