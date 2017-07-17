using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ControlPlane : MonoBehaviour {

    private List<GameObject> m_plane = new List<GameObject>();
    [SerializeField]
    private GameObject m_planePrefabs = null;
    private GridLayoutGroup m_grid = null;
    
    private float m_fSizeScale = 90;
    private int m_iWidthCount = 0;
    private int m_iHeightCount = 0;


    void Awake()
    {
        m_grid = GetComponent<GridLayoutGroup>();

        SetCreateSize(2);
    }

    public void SetCreateSize(int count)
    {
        m_iWidthCount = m_iHeightCount = (count * 2) + 1;
        m_grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        m_grid.constraintCount = m_iWidthCount;

        int maxCount = m_iWidthCount * m_iHeightCount;

        if (m_planePrefabs == null)
            return;

        int space = (int)(m_iWidthCount * 0.5f);
        int plane = 1;
        bool isFlagUP = true;

        //1차원 배열로 생성
        for(int i = 0; i < maxCount; i++)
        {
            GameObject go = Instantiate(m_planePrefabs);
            go.transform.SetParent(m_grid.transform);

            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;
            m_plane.Add(go);

            //빈영역 만들기
            if(space > i % m_iWidthCount ||  
               (m_iWidthCount - space) - 1 < i % m_iWidthCount ||
               i == (maxCount-1) * 0.5f)
            {
                go.GetComponent<Image>().enabled = false;
            }

            //빈영역 계산
            if ((i % m_iWidthCount) + 1 == m_iWidthCount)
            {
                if (isFlagUP == true)
                {
                    plane += 2;
                    space -= 1;
                    if (plane == m_iWidthCount)
                        isFlagUP = false;
                }
                else
                {
                    plane -= 2;
                    space += 1;
                }
            }
        }
    }
}


