using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager In;
    public List<LevelData> levelDatasList;
    public Transform allWallTra;
    public List<WallProgress> wallProgressesList;
    private void Awake()
    {
        In = this;

        for (int i=0; i<allWallTra.childCount; i++)
        {
            wallProgressesList.Add(allWallTra.GetChild(i).GetComponent<WallProgress>());
        }

        for (int i=0; i< wallProgressesList.Count; i++)
        {
            for (int j = wallProgressesList[i].startLvl - 1; j <= wallProgressesList[i].endLvl-1; j++)
            {
                levelDatasList[j].wallProgressList.Add(wallProgressesList[i]);
            }
        }
    }
    private void Start()
    {

    }
}
