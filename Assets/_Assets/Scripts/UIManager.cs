using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Ilumisoft.Connect.Game;
public class UIManager : MonoBehaviour
{
    public static UIManager In;
    public GameObject menuScreen, gameplayScreen, levelCompleteScreen, levelFailScreen;
    public RectTransform soundBarTra, progressBarTra;
    public TMP_Text moveTxt, progressPercentTxt;
    public List<GameObject> itemObjList;
    public List<Image> itemImgList;
    public List<TMP_Text> itemTxtList;
    public Image lvlImg, characterImage;
    public List<Sprite> lvlImgSprite, characterImageSprite;

    public bool isSettingBool = false;

    public Image soundImg, musicImg, vibrateImg;
    public Sprite soundOnSprite, soundOffSprite, musicOnSprite, musicOffSprite, vibrateOnSprite, vibrateOffSprite;

    float timeNo = 1f;
    private void Awake()
    {
        In = this;

        if (!PlayerPrefs.HasKey("Sound"))
        {
            PlayerPrefs.SetInt("Sound", 0);
        }
        if (!PlayerPrefs.HasKey("Music"))
        {
            PlayerPrefs.SetInt("Music", 0);
        }
        if (!PlayerPrefs.HasKey("Vibrate"))
        {
            PlayerPrefs.SetInt("Vibrate", 0);
        }

        DontDestroyOnLoad(this);
    }
    void Start()
    {
        Application.targetFrameRate = 60;
        if (PlayerPrefs.GetInt("Sound") == 0)
        {
            soundImg.sprite = soundOnSprite;
        }
        else
        {
            soundImg.sprite = soundOffSprite;
        }
        if (PlayerPrefs.GetInt("Music") == 0)
        {
            musicImg.sprite = musicOnSprite;
        }
        else
        {
            musicImg.sprite = musicOffSprite;
        }
        if (PlayerPrefs.GetInt("Vibrate") == 0)
        {
            vibrateImg.sprite = vibrateOnSprite;
        }
        else
        {
            vibrateImg.sprite = vibrateOffSprite;
        }
        LoadScene();
    }

    public void itemObOnOff( int countNo)
    {
        for(int i=0; i< itemObjList.Count; i++)
        {
            if (i <= countNo-1)
            {
                itemObjList[i].SetActive(true);
            }
            else
            {
                itemObjList[i].SetActive(false);
            }
        }
    }

    public void progressPercentFunc(int propercentVal)
    {
        progressPercentTxt.text = propercentVal.ToString() + "%";
    }

    public void UpdateProgressBar( int maxLvl)
    {
        float tmpProgress = ((float)PlayerPrefs.GetInt("LvlNo") / (float)maxLvl) * 100f;
        progressPercentFunc((int)tmpProgress);
        //progressBarTra
        float progressVal = ((1f-((float)PlayerPrefs.GetInt("LvlNo") / (float)maxLvl))) * 550f;
        LeanTween.cancel(progressBarTra.gameObject);
        Vector2 point1 = new Vector2(-progressVal, 0f);
        LeanTween.value(progressBarTra.gameObject, progressBarTra.offsetMin, point1, 1f).setEase(LeanTweenType.easeOutExpo).setOnUpdate((Vector2 val) =>
        {
            progressBarTra.offsetMin = val;
        }).setOnComplete(() => {
            progressBarTra.offsetMin = point1;
        });

        Vector2 point2 = new Vector2(-progressVal, 0f);
        LeanTween.value(progressBarTra.gameObject, progressBarTra.offsetMax, point2, 1f).setEase(LeanTweenType.easeOutExpo).setOnUpdate((Vector2 val) =>
        {
            progressBarTra.offsetMax = val;
        }).setOnComplete(() => {
            progressBarTra.offsetMax = point2;
        });
    }

    public void SoundFunc()
    {
        if (PlayerPrefs.GetInt("Sound") == 0) {
            PlayerPrefs.SetInt("Sound", 1);
            soundImg.sprite = soundOffSprite;
        }
        else
        {
            PlayerPrefs.SetInt("Sound", 0);
            soundImg.sprite = soundOnSprite;
            if (PlayerPrefs.GetInt("Sound") == 0)
            {
                GameSFX.Instance.Play(GameSFX.Instance.btnClick, 1f);
            }
        }
    }
    public void MusicFunc()
    {
        if (PlayerPrefs.GetInt("Music") == 0)
        {
            PlayerPrefs.SetInt("Music", 1);
            musicImg.sprite = musicOffSprite;
            GameSFX.Instance.PauseBG();
        }
        else
        {
            PlayerPrefs.SetInt("Music", 0);
            musicImg.sprite = musicOnSprite;
            GameSFX.Instance.ResumeBG();
        }
        if (PlayerPrefs.GetInt("Sound") == 0)
        {
            GameSFX.Instance.Play(GameSFX.Instance.btnClick, 1f);
        }
    }
    public void VibrateFunc()
    {
        if (PlayerPrefs.GetInt("Vibrate") == 0)
        {
            PlayerPrefs.SetInt("Vibrate", 1);
            vibrateImg.sprite = vibrateOffSprite;
        }
        else
        {
            PlayerPrefs.SetInt("Vibrate", 0);
            vibrateImg.sprite = vibrateOnSprite;
        }
        if (PlayerPrefs.GetInt("Sound") == 0)
        {
            GameSFX.Instance.Play(GameSFX.Instance.btnClick, 1f);
        }
    }

    public void SettingFunc()
    {
        if (isSettingBool == false)
        {
            SettingOn();
            isSettingBool = true;
        }
        else
        {
            SettingOff();
            isSettingBool = false;
        }
        if (PlayerPrefs.GetInt("Sound") == 0)
        {
            GameSFX.Instance.Play(GameSFX.Instance.btnClick, 1f);
        }
    }

    void SettingOn()
    {
        LeanTween.cancel(soundBarTra.gameObject);
        Vector2 point1 = new Vector2(0f, -212.5f);
        LeanTween.value(soundBarTra.gameObject, soundBarTra.anchoredPosition, point1, 1f).setEase(LeanTweenType.easeOutExpo).setOnUpdate((Vector2 val) =>
        {
            soundBarTra.anchoredPosition = val;
        }).setOnComplete(()=> {
            soundBarTra.anchoredPosition = point1;
        });

        Vector2 point2 = new Vector2(135.11f, 425f);
        LeanTween.value(soundBarTra.gameObject, soundBarTra.sizeDelta, point2, 1f).setEase(LeanTweenType.easeOutExpo).setOnUpdate((Vector2 val) =>
        {
            soundBarTra.sizeDelta = val;
        }).setOnComplete(() => {
            soundBarTra.sizeDelta = point2;
        });
    }
    void SettingOff()
    {
        LeanTween.cancel(soundBarTra.gameObject);
        Vector2 point1 = new Vector2(0f, 0f);
        LeanTween.value(soundBarTra.gameObject, soundBarTra.anchoredPosition, point1, 1f).setEase(LeanTweenType.easeOutExpo).setOnUpdate((Vector2 val) =>
        {
            soundBarTra.anchoredPosition = val;
        }).setOnComplete(() => {
            soundBarTra.sizeDelta = point1;
        });

        Vector2 point2 = new Vector2(135.11f, 0f);
        LeanTween.value(soundBarTra.gameObject, soundBarTra.sizeDelta, point2, 1f).setEase(LeanTweenType.easeOutExpo).setOnUpdate((Vector2 val) =>
        {
            soundBarTra.sizeDelta = val;
        }).setOnComplete(() => {
            soundBarTra.sizeDelta = point2;
        });
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(1);
    }

    public void UpdateMoveTxt(int moveNo)
    {
        moveTxt.text = moveNo.ToString();
    }

    public void Add5Moves()
    {
        if (PlayerPrefs.GetInt("Sound") == 0)
        {
            GameSFX.Instance.Play(GameSFX.Instance.btnClick, 1f);
        }
        Ilumisoft.Connect.Game.GameManager.In.Add5Moves();
    }

    public void Btn_LevelComplete()
    {
        if (PlayerPrefs.GetInt("Sound") == 0)
        {
            GameSFX.Instance.Play(GameSFX.Instance.btnClick, 1f);
        }
        PlayerPrefs.SetInt("LvlNo", PlayerPrefs.GetInt("LvlNo") + 1);
        GamePlayManager.In.CreateGameManager();
        //LoadScene();
    }

    public void Btn_LevelFail()
    {
        if (PlayerPrefs.GetInt("Sound") == 0)
        {
            GameSFX.Instance.Play(GameSFX.Instance.btnClick, 1f);
        }
        LoadScene();
    }

    public void GamePlayScreen()
    {
        RectTransform gameplayTra = gameplayScreen.GetComponent<RectTransform>();
        LeanTween.value(3000f, 0f, timeNo).setEase(LeanTweenType.easeOutExpo).setOnUpdate((float val) =>
        {
            gameplayTra.offsetMax = new Vector2(0f, val);
            gameplayTra.offsetMin = new Vector2(0f, -val);
        });

        RectTransform levelCompleteTra = levelCompleteScreen.GetComponent<RectTransform>();
        
        LeanTween.value(0f, 3000f, timeNo).setEase(LeanTweenType.easeOutExpo).setOnUpdate((float val) =>
        {
            levelCompleteTra.offsetMax = new Vector2(0f, -val);
            levelCompleteTra.offsetMin = new Vector2(0f, -val);
        });

        RectTransform levelFailTra = levelFailScreen.GetComponent<RectTransform>();
        LeanTween.value(0f, 3000f, timeNo).setEase(LeanTweenType.easeOutExpo).setOnUpdate((float val) =>
        {
            levelFailTra.offsetMax = new Vector2(0f, -val);
            levelFailTra.offsetMin = new Vector2(0f, -val);
        });
    }

    public void LevelCompleteScreen()
    {
        lvlImg.sprite = lvlImgSprite[PlayerPrefs.GetInt("LvlNo") - 1];
        RectTransform gameplayTra = gameplayScreen.GetComponent<RectTransform>();
        LeanTween.value(0f, 3000f, timeNo).setEase(LeanTweenType.easeOutExpo).setOnUpdate((float val) =>
        {
            gameplayTra.offsetMax = new Vector2(0f, -val);
            gameplayTra.offsetMin = new Vector2(0f, -val);
        });

        RectTransform levelCompleteTra = levelCompleteScreen.GetComponent<RectTransform>();
        LeanTween.value(3000f, 0f, timeNo).setEase(LeanTweenType.easeOutExpo).setOnUpdate((float val) =>
        {
            levelCompleteTra.offsetMax = new Vector2(0f, val);
            levelCompleteTra.offsetMin = new Vector2(0f, -val);
        });
    }

    public void LevelFailScreen()
    {
        RectTransform gameplayTra = gameplayScreen.GetComponent<RectTransform>();
        LeanTween.value(0f, 3000f, timeNo).setEase(LeanTweenType.easeOutExpo).setOnUpdate((float val) =>
        {
            gameplayTra.offsetMax = new Vector2(0f, -val);
            gameplayTra.offsetMin = new Vector2(0f, -val);
        });

        RectTransform levelFailTra = levelFailScreen.GetComponent<RectTransform>();
        LeanTween.value(3000f, 0f, timeNo).setEase(LeanTweenType.easeOutExpo).setOnUpdate((float val) =>
        {
            levelFailTra.offsetMax = new Vector2(0f, val);
            levelFailTra.offsetMin = new Vector2(0f, -val);
        });
    }
}
