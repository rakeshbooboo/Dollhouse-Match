using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelData : MonoBehaviour
{
    public int MovesAvailable;
    public List<Sprite> sprites;
    public List<int> spritesNo;
    public int totalTargetNo;
    public int collectedTargetNo;
    
    public List<float> buildProgressNoList = new List<float> ();

    public List<WallProgress> wallProgressList;

    public int CollectedTargetNo
    {
        get => collectedTargetNo; set
        {
            for (int i = 0; i < wallProgressList.Count; i++)
            {
                collectedTargetNo = value;
                float progress = (float)collectedTargetNo / buildProgressNoList[i];
                WallProgress wallProgress = wallProgressList[i];
                progress = progress - wallProgress.buildObjNo;
                StartCoroutine(wallProgress.ProgressIEnum(progress));
            }
        }
    }

    public void StartFunc()
    {
        for (int i=0; i< wallProgressList.Count; i++)
        {
            wallProgressList[i].gameObject.SetActive(true);
        }
        for (int i = 0; i < spritesNo.Count; i++)
        {
            totalTargetNo += spritesNo[i];
        }
        for (int i = 0; i < wallProgressList.Count; i++)
        {
            buildProgressNoList.Add((float)totalTargetNo / (float)wallProgressList[i].buildObjList.Count);
        }
    }
}
