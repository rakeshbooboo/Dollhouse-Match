using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Character : MonoBehaviour
{
    public Animator animator;
    public NavMeshAgent navMeshAgent;
    public bool isJackpack, IsCarpenter, IsPainter, IsSwing;
    public bool isMove = true;
    public Transform childTra;

    public GameObject jakpackObj, cutterObj, hammerObj, bucketObj, rollerObj, needleObj, scissorObj;
    Vector3 targetPos, childPos;
    public int buildObjNo = 0;

    bool isFinish = true;
    public WallProgress wallProgress;
    void Start()
    {
        animator.speed = 1f;
        wallProgress = transform.parent.GetComponent<WallProgress>();
        StartCoroutine(StartIEnum());
    }

    IEnumerator StartIEnum() {
        yield return new WaitForEndOfFrame();
        buildObjNo = wallProgress.buildObjNo;
        if (buildObjNo >= wallProgress.buildObjList.Count)
        {
            Destroy(gameObject);
        }
        yield return new WaitForSeconds(1f);

        targetPos = new Vector3 (wallProgress.buildObjList[buildObjNo].transform.GetChild(0).position.x, transform.position.y, wallProgress.buildObjList[buildObjNo].transform.GetChild(0).position.z);
        LeanTween.value(gameObject, navMeshAgent.speed, 3.5f, 3f).setEase(LeanTweenType.easeOutSine).setOnUpdate((float val) =>
        {
            navMeshAgent.speed = val;
        });

        animator.speed = 2f;
        isFinish = false;
    }

    void LateUpdate()
    {
        if (isFinish == false)
        {
            if (isMove)
            {
                if (Vector3.Distance(transform.position, targetPos) <= 0.5f)
                {
                    LeanTween.cancel(gameObject);
                    LeanTween.rotate(gameObject, wallProgress.buildObjList[buildObjNo].transform.GetChild(0).transform.eulerAngles, 1f).setEase(LeanTweenType.easeOutExpo);
                    isMove = false;
                    if (IsCarpenter)
                    {
                        CarpenterFunc();
                    }
                    else if (IsPainter)
                    {
                        PainterFunc();
                    }
                    else if (IsSwing)
                    {
                        SwingFunc();
                    }
                }
                else if(navMeshAgent)
                {
                    navMeshAgent.SetDestination(targetPos);
                }
            }
            if (isMove == false && buildObjNo != wallProgress.buildObjNo)
            {
                buildObjNo = wallProgress.buildObjNo;
                targetPos = new Vector3(wallProgress.buildObjList[buildObjNo].transform.GetChild(0).position.x, transform.position.y, wallProgress.buildObjList[buildObjNo].transform.GetChild(0).position.z);
                animator.SetBool("IsPainter", false);
                animator.SetBool("IsSwing", false);
                animator.SetBool("IsCarpenter", false);
                isMove = true;
            }

            childPos = new Vector3(transform.position.x, wallProgress.buildObjList[buildObjNo].transform.GetChild(0).position.y, transform.position.z);
            childTra.position = Vector3.Lerp(childTra.position, childPos, 5f * Time.deltaTime);
        }
    }

    public void FinishFunc()
    {
        if (isFinish == false)
        {
            isFinish = true;
            animator.SetBool("IsPainter", false);
            animator.SetBool("IsSwing", false);
            animator.SetBool("IsCarpenter", false);
            GamePlayManager gamePlayManager = GamePlayManager.In;

            Vector3 tmpPos = gamePlayManager.transform.GetChild(Random.Range(0, gamePlayManager.transform.childCount)).transform.position;
            Vector3 direction = tmpPos - transform.position;
            LeanTween.value(gameObject, 0f, 100f, 2f).setEase(LeanTweenType.easeOutExpo).setOnUpdate((float val) =>
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction, Vector3.up), 5f * Time.deltaTime);
            });
            LeanTween.moveLocal(childTra.gameObject, Vector3.zero, 3f);
            LeanTween.move(gameObject, tmpPos, 10f).setOnComplete(() =>
            {
                Destroy(gameObject);
            });

            navMeshAgent.enabled = false;
        }
    }

    public void CarpenterFunc() {
        IsCarpenter = true;
        animator.SetBool("IsPainter", false);
        animator.SetBool("IsSwing", false);
        animator.SetBool("IsCarpenter", true);
        cutterObj.SetActive(true);
        hammerObj.SetActive(true);

        bucketObj.SetActive(false);
        rollerObj.SetActive(false);
        needleObj.SetActive(false);
        scissorObj.SetActive(false);
    }

    public void PainterFunc()
    {
        IsPainter = true;
        animator.SetBool("IsCarpenter", false);
        animator.SetBool("IsSwing", false);
        animator.SetBool("IsPainter", true);
        bucketObj.SetActive(true);
        rollerObj.SetActive(true);

        cutterObj.SetActive(false);
        hammerObj.SetActive(false);
        needleObj.SetActive(false);
        scissorObj.SetActive(false);
    }

    public void SwingFunc()
    {
        IsPainter = true;
        animator.SetBool("IsCarpenter", false);
        animator.SetBool("IsPainter", false);
        animator.SetBool("IsSwing", true);
        needleObj.SetActive(true);
        scissorObj.SetActive(true);

        cutterObj.SetActive(false);
        hammerObj.SetActive(false);
        bucketObj.SetActive(false);
        rollerObj.SetActive(false);
    }

}
