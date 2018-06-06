using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class planeClick : MonoBehaviour
{
    public Vector2 startPos = Vector2.zero;
    public Vector2 endPos = Vector2.zero;

    private List<Vector3> resultList = new List<Vector3>();

    public float moveSpeed = 0;
    public Text debugText = null;

    private bool m_isMove = false;

    public PlayerControlType m_ControlType = PlayerControlType.Idle;

    private void Awake()
    {
        if(SgtPlaneManager.Instance.m_FieldPlane != null)
        {
            SgtPlaneManager.Instance.m_FieldPlane.InitPlane();
        }
    }

    // Update is called once per frame
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
                            HitTarget(hit);
                        }
                    }
                }
                else
                {
                    m_isMove = false;
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

                Debug.DrawRay(ray.origin, ray.direction);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    HitTarget(hit);
                }
                else
                {
                    m_isMove = false;
                }
            }
        }
    }

    private void HitTarget(RaycastHit hitRay)
    {
        switch(hitRay.collider.gameObject.layer)
        {
            case (int)GlobalEnum.Layer.Player:
                {
                    //  행동 관련 판단 처리입니다.
                    switch(m_ControlType)
                    {
                        case PlayerControlType.Idle:
                            {
                                SgtPlaneManager.Instance.m_ControlPlane.CreatePlane();
                                SgtPlaneManager.Instance.m_ControlPlane.SetActiveControlPlane(gameObject.transform.position, 2);
                                m_ControlType = PlayerControlType.Move;
                            }
                            break;
                    }
                }
                break;
            case (int)GlobalEnum.Layer.ControlRange:
                {
                    if (m_isMove) return;
                    else m_isMove = true;

                    CalculatorPath(hitRay.point);

                    //  활성화된 컨트롤 플랜을 꺼줍니다.
                    SgtPlaneManager.Instance.m_ControlPlane.SetActivePlaneAll(false);
                }
                break;
        }
    }

    /// <summary>
    ///   입력 받는 포지션까지 계산하여 
    ///   캐릭터 움직은 시작합니다.
    /// </summary>
    /// <param name="point"></param>
    private void CalculatorPath(Vector3 point)
    {
        endPos = point;
        resultList.Clear();

        List<PlanePathNode> result = SgtPlaneManager.Instance.FindPath(startPos, endPos, true);

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

    IEnumerator MoveCharater()
    {
        if (resultList.Count == 0)
        {
            m_ControlType = PlayerControlType.Idle;
            m_isMove = false;
            yield break;
        }

        for (int i = 0; i < resultList.Count; i++)
        {
            transform.position = Vector3.Lerp(transform.position, resultList[i], 2);

            yield return new WaitForSeconds(moveSpeed);
        }

        startPos = resultList[resultList.Count - 1];

        m_ControlType = PlayerControlType.Idle;
        m_isMove = false;
    }


    void OnDrawGizmos()
    {
        if (resultList.Count == 0)
            return;

        Vector3 lineStart = Vector3.zero;
        for (int i = 0; i < resultList.Count; i++)
        {
            Gizmos.DrawSphere(resultList[i], 0.5f);

            if (i == 0)
                lineStart = resultList[i];

            else
            {
                Gizmos.DrawLine(lineStart, resultList[i]);

                lineStart = resultList[i];
            }
        }
    }
}
