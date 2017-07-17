using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;
using System.Collections.Generic;

public class TouchController : MonoBehaviour, IPointerClickHandler
{
    public MakePlaneScript planeScript = null;        //플랜
    public GameObject m_Player = null;
    public Camera m_UICamera = null;

    public Vector2 startPos = Vector2.zero;
    public Vector2 endPos = Vector2.zero;

    private List<Vector3> resultList = new List<Vector3>();

    public float moveSpeed = 0;
    public bool m_isTouch = true;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (planeScript == null)
            return;

        if (m_isTouch == false)
            return;
        else
            m_isTouch = false;

        startPos = m_Player.transform.position;
        endPos =  m_UICamera.ScreenToViewportPoint(eventData.pressPosition);

        resultList.Clear();

        float startTime = Time.realtimeSinceStartup;

        List<PathNode> result = planeScript.FindPath(startPos, endPos, true);

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

    IEnumerator MoveCharater()
    {
        for (int i = 0; i < resultList.Count; i++)
        {
            m_Player.transform.position = Vector3.Lerp(transform.position, resultList[i], 2);

            yield return new WaitForSecondsRealtime(moveSpeed);
        }

        if (resultList.Count > 0)
        {
            startPos = resultList[resultList.Count - 1];
            m_isTouch = true;
        }
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
