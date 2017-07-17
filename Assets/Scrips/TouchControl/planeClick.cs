using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

public class planeClick : MonoBehaviour
{
    public MakePlaneScript  planeScript;        //플랜

    //터치아이디
    private int touchID = -1;

    private bool addStart = false;
    public Vector2 startPos = Vector2.zero;
    public Vector2 endPos = Vector2.zero;

    private List<Vector3> resultList = new List<Vector3>();

    public float moveSpeed = 0;
    [SerializeField]
    private GameObject m_startPos_Player = null;
    [SerializeField]
    private GameObject m_startPos_Monster = null;

    void Start()
    {
        startPos = m_startPos_Player.transform.localPosition;
    }

     //Update is called once per frame
    void Update () {
    
        mouseAndTouch();
	}



    public void mouseAndTouch()
    {
        //터치 했을시
        if (Input.touchCount > 0)
        {
            //터치한 갯수를 체크
            for (int _touchNum = 0; _touchNum < Input.touchCount; _touchNum++)
            {
                Vector3 touchPos = Input.GetTouch(_touchNum).position;
    
                Ray ray = Camera.main.ScreenPointToRay(touchPos);
                RaycastHit hit;
    
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    //처음 터치된 순간
                    if (Input.GetTouch(_touchNum).phase == TouchPhase.Began)
                    {
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                        {
                            this.endPos = hit.point;
                            this.resultList.Clear();
    
    
                            float startTime = Time.realtimeSinceStartup;
    
                            List<PathNode> result =
                                planeScript.FindPath(startPos, endPos, true);
    
                            float endTime = Time.realtimeSinceStartup;
    
                            float deltaTime = endTime - startTime;
    
    
    
    
                            resultList.Clear();
    
                            if (result != null)
                            {
                                for (int _count = 0; _count < result.Count; _count++)
                                {
                                    resultList.Add(result[_count].centerPos);
                                }
                            }
    
    
                            StartCoroutine(MoveCharater());
                        }
                    }
    
                    //터치하고 이동
                    else if (Input.GetTouch(_touchNum).phase == TouchPhase.Moved)
                    {
    
                    }
    
                    //터치를 땐 순간
                    else if (Input.GetTouch(_touchNum).phase == TouchPhase.Ended)
                    {
    
                    }
                }
            }
        }
        else
        {
            //마우스 클릭시
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
    
                Debug.DrawRay(ray.origin,ray.direction);
                if (Physics.Raycast(ray, out hit,Mathf.Infinity))
                {
                    this.endPos = hit.point;
                    this.resultList.Clear();
    
    
                    float startTime = Time.realtimeSinceStartup;
    
                    List<PathNode> result =
                        planeScript.FindPath(startPos, endPos, true);
    
                    float endTime = Time.realtimeSinceStartup;
    
                    float deltaTime = endTime - startTime;
                    
    
    
    
                    resultList.Clear();
    
                    if (result != null)
                    {
                        for (int i = 0; i < result.Count; i++)
                        {
                            resultList.Add(result[i].centerPos);
                        }
                    }
    
    
                    StartCoroutine(MoveCharater());
                }
            }
        }
    }

    IEnumerator MoveCharater()
    {
        for (int i = 0; i < resultList.Count; i++)
        {
            transform.position = Vector3.Lerp(transform.position, resultList[i], 2);

            yield return new WaitForSeconds(moveSpeed);
        }

        if(resultList.Count > 0)
            startPos = resultList[resultList.Count - 1];
    }


    void OnDrawGizmos()
    {
        if (this.resultList.Count == 0)
            return;

        Vector3 lineStart = Vector3.zero;
        for (int i = 0; i < resultList.Count; i++)
        {
            Gizmos.DrawSphere(this.resultList[i], 0.5f);

            if (i == 0)
                lineStart = this.resultList[i];

            else
            {
                Gizmos.DrawLine(lineStart, this.resultList[i]);

                lineStart = this.resultList[i];
            }
        }
    }
}
