using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPlane : MonoBehaviour {
    
    [SerializeField]
    private List<BasePlaneNode> m_controlPlaneList = new List<BasePlaneNode>();

    //셀 갯수
    public int m_cellCount_Width;                   //월드 가로 셀갯수
    public int m_cellCount_Height;                  //월드 세로 셀갯수

    //월드 위치들
    public Vector2 m_worldMinPos;                    //월드 최소 위치
    public Vector2 m_worldMaxPos;                    //월드 최대 위치
    public Vector2 m_worldStartPos;                  //월드 시작 위치

    public GameObject cellObject;

    public bool m_isInit = true;
    
    // Use this for initialization
    //void Start()
    //{
    //    CreatePlane();
    //}
    
    public void CreatePlane()
    {
        if (m_isInit == false)
            return;

        m_cellCount_Width = SgtPlaneManager.Instance.Cell_Width;
        m_cellCount_Height = SgtPlaneManager.Instance.Cell_Height;

        float fWidth = (m_cellCount_Width * 0.5f);
        float fHeight = (m_cellCount_Height * 0.5f);

        //월드 시작위치
        m_worldStartPos = new Vector2(-fWidth, -fHeight);
        m_worldMinPos = new Vector2(-fWidth, -fHeight);
        m_worldMaxPos = new Vector2(fWidth, fHeight);

        ClearPlane();

        //노드 배열 생성
        PlanePathNode[,] pathNodes = new PlanePathNode[m_cellCount_Width, m_cellCount_Height];

        for (int x = 0; x < m_cellCount_Width; x++)
        {
            for (int y = 0; y < m_cellCount_Height; y++)
            {
                pathNodes[x, y] = new PlanePathNode(x, y);

                pathNodes[x, y].minPos = m_worldStartPos + (Vector2.right * x * 1f) + (Vector2.up * y * 1f);
                pathNodes[x, y].maxPos = pathNodes[x, y].minPos + (Vector2.right * 1f) + (Vector2.up * 1f);
                pathNodes[x, y].centerPos = (pathNodes[x, y].minPos + pathNodes[x, y].maxPos) * 0.5f;

                GameObject go = Instantiate(cellObject, pathNodes[x, y].centerPos, Quaternion.identity, transform) as GameObject;
                BasePlaneNode plane = go.AddComponent<BasePlaneNode>();
                go.GetComponent<BasePlaneNode>().m_nodeData = pathNodes[x, y];
                
                int calc = Mathf.Abs((int)pathNodes[x, y].centerPos.x) + Mathf.Abs((int)pathNodes[x, y].centerPos.y);
                go.SetActive(!(calc <= 0 || calc >= 3));

                plane.gameObject.layer = (int)GlobalEnum.Layer.ControlRange;
                m_controlPlaneList.Add(plane);
            }
        }

        m_isInit = false;
    }


    public void SetActiveControlPlane(Vector3 pos, int cellCount = 1)
    {
        if (cellObject == null)
            return;

        transform.localPosition = pos;

        m_cellCount_Width = cellCount * 2 + 1;
        m_cellCount_Height = cellCount * 2 + 1;

        PlanePathNode[,] pathNodes = new PlanePathNode[m_cellCount_Width, m_cellCount_Height];

        int calc = 0;
        //노드 배열 생성
        pathNodes = new PlanePathNode[m_cellCount_Width, m_cellCount_Height];

        for (int x = 0; x < m_cellCount_Width; x++)
        {
            for (int y = 0; y < m_cellCount_Height; y++)
            {
                pathNodes[x, y] = m_controlPlaneList[(m_cellCount_Width * x) + y].m_nodeData;
            }
        }

        for (int i = 0; i < m_controlPlaneList.Count; i++)
        {
            int centerX = (int)m_controlPlaneList[i].m_nodeData.centerPos.x;
            int centerY = (int)m_controlPlaneList[i].m_nodeData.centerPos.y;

            calc = Mathf.Abs(centerX) + Mathf.Abs(centerY);

            //활성화/비활성화 판단
            m_controlPlaneList[i].gameObject.SetActive(!(calc <= 0 || calc >= cellCount + 1));
        }
    }


    public void ClearPlane()
    {
        if (m_controlPlaneList.Count > 0)
        {
            for (int i = 0; i < m_controlPlaneList.Count; i++)
            {
                DestroyImmediate(m_controlPlaneList[i]);
            }
        }

        m_controlPlaneList.Clear();
    }

    /// <summary>
    /// 컨트롤 플랜의 활성화/비활성화 합니다.
    /// </summary>
    public void SetActivePlaneAll(bool isActive)
    {
        if (m_controlPlaneList != null || m_controlPlaneList.Count > 0)
        {
            var itor = m_controlPlaneList.GetEnumerator();
            while(itor.MoveNext())
            {
                itor.Current.gameObject.SetActive(isActive);
            }
        }
    }
}
