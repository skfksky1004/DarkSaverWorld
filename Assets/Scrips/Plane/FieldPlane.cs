using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FieldPlane : MonoBehaviour {

    [SerializeField]
    private List<BasePlaneNode> m_fieldPlaneList = new List<BasePlaneNode>();

    //셀 갯수
    public int m_cellCount_Width = 20;                   //월드 가로 셀갯수
    public int m_cellCount_Height = 20;                  //월드 세로 셀갯수

    //월드 위치들
    public Vector2 m_worldMinPos;                    //월드 최소 위치
    public Vector2 m_worldMaxPos;                    //월드 최대 위치
    public Vector2 m_worldStartPos;                  //월드 시작 위치

    public GameObject cellObject;

    // Use this for initialization
    void Start () {
        CreatePlane();

    }

    public void CreatePlane()
    {
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

                plane.gameObject.layer = (int)GlobalEnum.Layer.Field;
                m_fieldPlaneList.Add(plane);
            }
        }
    }

    public PlanePathNode[,] InitPlane()
    {
        if (m_fieldPlaneList.Count == 0)
        {
            BasePlaneNode[] planes = GetComponentsInChildren<BasePlaneNode>();
            for (int i = 0; i < planes.Length; i++)
            {
                m_fieldPlaneList.Add(planes[i]);
            }
        }

        SgtPlaneManager.Instance.Cell_Width = m_cellCount_Width;
        SgtPlaneManager.Instance.Cell_Height = m_cellCount_Height;

       PlanePathNode[,] pathNodes = new PlanePathNode[m_cellCount_Width, m_cellCount_Height];

        //노드 배열 생성
        pathNodes = new PlanePathNode[m_cellCount_Width, m_cellCount_Height];

        for (int x = 0; x < m_cellCount_Width; x++)
        {
            for (int y = 0; y < m_cellCount_Height; y++)
            {
                pathNodes[x, y] = m_fieldPlaneList[(m_cellCount_Width * x) + y].m_nodeData;
                ////셀마다 민포스
                //pathNodes[x, y].minPos = m_worldStartPos + (Vector2.right * x * 1f) + (Vector2.up * y * 1f);
                ////셀마다 맥포스
                //pathNodes[x, y].maxPos = pathNodes[x, y].minPos + (Vector2.right * 1f) + (Vector2.up * 1f);
                ////셀마다 센터포스
                //pathNodes[x, y].centerPos = (pathNodes[x, y].minPos + pathNodes[x, y].maxPos) * 0.5f;
            }
        }

        return pathNodes;
    }

    public void ClearPlane()
    {
        if (m_fieldPlaneList.Count > 0)
        {
            for (int i = 0; i < m_fieldPlaneList.Count; i++)
            {
                DestroyImmediate(m_fieldPlaneList[i].gameObject);
            }
        }

        m_fieldPlaneList.Clear();
    }

    /// <summary>
    /// 해당 좌표의 패스를 리턴
    /// </summary>
    /// <param name="posX"></param>
    /// <param name="posY"></param>
    /// <returns></returns>
    public PlanePathNode GetIsMove(int posX,int posY)
    {
        if(m_fieldPlaneList != null || m_fieldPlaneList.Count > 0)
        {
            for(int i = 0; i< m_fieldPlaneList.Count; i++)
            {
                if(m_fieldPlaneList[i].m_nodeData.indexX == posX &&
                   m_fieldPlaneList[i].m_nodeData.indexY == posY)
                {
                    return m_fieldPlaneList[i].m_nodeData;
                }
            }
        }
        return null;
    }
}
