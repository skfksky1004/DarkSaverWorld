using System;
using System.Collections.Generic;
using UnityEngine;

public class SgtPlaneManager : MonoSingleton<SgtPlaneManager>
{
    public FieldPlane m_FieldPlane;
    public ControlPlane m_ControlPlane;

    //노드 배열
    [SerializeField]
    public PlanePathNode[,] pathNodes;
                
    private List<PlanePathNode> openNodeList = new List<PlanePathNode>();     //오픈노드 리스트
    private List<PlanePathNode> closeNodeList = new List<PlanePathNode>();    //클로즈 노드 리스트

    private List<PlanePathNode> result = new List<PlanePathNode>();           //결과 벡터
    private List<PlanePathNode> resultStack = new List<PlanePathNode>();      //결과 스택(백트레킹 때문에 쓴다.)
    
    private PlanePathNode startNode;         //시작 노드
    private PlanePathNode endNode;           //목적지 노드
    private bool bFindGoal;             //목적지 찾은값


    public int Cell_Width { get; set; }
    public int Cell_Height { get; set; }

    protected override void OnInit()
    {
        if(m_FieldPlane == null)
        {
            m_FieldPlane = FindObjectOfType<FieldPlane>();

            if(m_FieldPlane == null)
            {
                m_FieldPlane = new GameObject("FieldPlane").AddComponent<FieldPlane>();
            }

            m_FieldPlane.transform.SetParent(this.transform);
        }

        if(m_ControlPlane == null)
        {
            m_ControlPlane = FindObjectOfType<ControlPlane>();

            if (m_ControlPlane == null)
            {
                m_ControlPlane = new GameObject("ControlPlane").AddComponent<ControlPlane>();
            }

            m_ControlPlane.transform.SetParent(this.transform);
        }

        pathNodes = m_FieldPlane.InitPlane();
    }

    protected override void OnExit()
    {

    }

    /// <summary>
    /// 알고리즘으로 경로 찾기
    /// </summary>
    /// <param name="startPos">월드 시작 위치</param>
    /// <param name="endPos">월드 종료 위치</param>
    /// <param name="bDiagonal">대각 갈수있는</param>
    /// <returns></returns>
    public List<PlanePathNode> FindPath(Vector2 startPos, Vector2 endPos, bool bDiagonal)
    {
        //위치에 따른 시작노드와 종료 노드를 얻는다.
        this.startNode = this.GetPathNode(startPos);
        this.endNode = this.GetPathNode(endPos);

        //유효하지 않는 경로다.
        if (this.startNode == null || this.endNode == null)
            return null;
        
        //경로 계산을 할 필요가 없다.
        if (startNode == endNode)
            return null;
        
        //갈수 없는 목적지를 선택햇다면 
        if (endNode.isMoveble == false)
            return null;
                
        //경로 검색전 리셋 작업
        //모든 리스트 초기화
        this.result.Clear();
        this.resultStack.Clear();
        this.openNodeList.Clear();
        this.closeNodeList.Clear();
        this.bFindGoal = false;
        
        //시작 노드의 부모는 반드시 널
        this.startNode.pParent = null;

        //
        //시작 노드를 클로즈리스트에 넣고 그놈을 현재 검색노드로 설정한다.
        //
        this.closeNodeList.Add(startNode);
        PlanePathNode curNode = startNode;       //현재 검색노드
        //
        //핵심 패스파인딩 loop검색노드를 찾을수 없다면 검색실패
        //보통 이루프가 빠져나가는 경우는 목적지까지 길이 다 막혀 있을때 이다.
        //

        while (curNode != null)
        {
            //루프에서 첫번째
            //현재노드 주변으로 갈수 있는 노드를 오픈노드에 추가한다.
            this.CheckAround(curNode, bDiagonal);

            //위의 첫번째 과정에서 엔드노드를 찾았다면
            if (this.bFindGoal)
            {
                //백트래킹
                PlanePathNode node = this.endNode;

                //스택에 목적지부터 result 거꾸로 쌓는다.
                while (node != null)
                {
                    //스택에 추가
                    this.result.Add(node);
                    node = node.pParent;
                }

                //반대로 뒤집는다.
                this.result.Reverse();

                return this.result;
            }

            //루프 두번째
            //오픈노드리스트중에 토탈코스트가 가장 작은놈을 오픈노드리스트에서 빼고 클로즈노드리스트에 넣은후
            //그놈을 검색 노드로 지정
            curNode = this.GetMiniumNodeFromOpenNode();
        }
        
        return null;
    }



    //해당위치가 유효한 위치인지 파악
    //유효한 위치면 해당위치 패스 리턴
    //유효하지 않으면 null을 리천
    public PlanePathNode GetPathNode(Vector2 pos)
    {
        if (pos.x > m_ControlPlane.m_worldMinPos.x &&
            pos.x < m_ControlPlane.m_worldMaxPos.x &&
            pos.y > m_ControlPlane.m_worldMinPos.y &&
            pos.y < m_ControlPlane.m_worldMaxPos.y)
        {
            int idxX = (int)((pos.x - m_ControlPlane.m_worldMinPos.x));
            int idxY = (int)((pos.y - m_ControlPlane.m_worldMinPos.y));

            return pathNodes[idxX, idxY];
        }
        
        return null;
    }



    //해당 인덱스의 위치가 갈수 있는 노드인지 확인
    bool isMoveable(int indexX, int indexY)
    {
        //  0부터 만들어진 필드플랜의 갯수 유효한 노드인지를 판단합니다.
        if (indexX >= 0 && indexX < m_FieldPlane.m_cellCount_Width &&
            indexY >= 0 && indexY < m_FieldPlane.m_cellCount_Height)
        {
            return pathNodes[indexX, indexY].isMoveble;
        }
        
        return false;
    }



    /// <summary>
    /// 현재 노드 중심으로 갈수있는 노드를 오픈리스트에 추가
    /// </summary>
    /// <param name="curNode">현재노드</param>
    /// <param name="bDiagonal">대각 검색</param>
    void CheckAround(PlanePathNode curNode, bool bDiagonal)
    {
        if (isMoveable(curNode.indexX, curNode.indexY + 1))
            AddOpenList(curNode.indexX, curNode.indexY + 1, curNode);

        if (isMoveable(curNode.indexX, curNode.indexY - 1))
            AddOpenList(curNode.indexX, curNode.indexY - 1, curNode);

        if (isMoveable(curNode.indexX + 1, curNode.indexY))
            AddOpenList(curNode.indexX + 1, curNode.indexY, curNode);

        if (isMoveable(curNode.indexX - 1, curNode.indexY))
            AddOpenList(curNode.indexX - 1, curNode.indexY, curNode);

        if (bDiagonal)
        {
            if (isMoveable(curNode.indexX + 1, curNode.indexY + 1))
                AddOpenList(curNode.indexX + 1, curNode.indexY + 1, curNode);

            if (isMoveable(curNode.indexX + 1, curNode.indexY - 1))
                AddOpenList(curNode.indexX + 1, curNode.indexY - 1, curNode);

            if (isMoveable(curNode.indexX - 1, curNode.indexY + 1))
                AddOpenList(curNode.indexX - 1, curNode.indexY + 1, curNode);

            if (isMoveable(curNode.indexX - 1, curNode.indexY - 1))
                AddOpenList(curNode.indexX - 1, curNode.indexY - 1, curNode);
        }
    }

    /// <summary>
    /// 해당 인덱스의 노드 오픈리스트 추가
    /// </summary>
    /// <param name="indexX">인덱스X</param>
    /// <param name="indexY">인덱스Y</param>
    /// <param name="parent">누구로 부터왓니?</param>
    void AddOpenList(int indexX, int indexY, PlanePathNode parent)
    {
        PlanePathNode node = this.pathNodes[indexX, indexY];
        
        if (this.closeNodeList.Contains(node))
            return;
        
        if (this.openNodeList.Contains(node))
        {
            int nowStartCost = parent.costFromtStart + 1;

            int prevStartCost = node.costFromtStart;

            if (nowStartCost < prevStartCost)
            {
                node.costFromtStart = nowStartCost;
                node.pParent = parent;
                node.costTotal = node.costFromtStart + node.costToGoal;
            }
        }
        else
        {
            node.pParent = parent;
            node.costFromtStart = parent.costFromtStart + 1;
            node.costToGoal = GetCostToGoal(node);
            node.costTotal = node.costFromtStart + node.costToGoal;

            
            this.openNodeList.Add(node);
            

            if (node == this.endNode)
                this.bFindGoal = true;
        }
    }
    
    int GetCostToGoal(PlanePathNode node)
    {
        return Mathf.Abs(node.indexX - endNode.indexX) + Mathf.Abs(node.indexY - endNode.indexY);
    }

    PlanePathNode GetMiniumNodeFromOpenNode()
    {
        if (this.openNodeList.Count == 0)
            return null;
                
        PlanePathNode miniumNode = this.openNodeList[0];
        for (int i = 1; i < this.openNodeList.Count; i++)
        {
            if (this.openNodeList[i].costTotal < miniumNode.costTotal)
                miniumNode = this.openNodeList[i];
        }
        
        this.openNodeList.Remove(miniumNode);
        this.closeNodeList.Add(miniumNode);

        return miniumNode;
    }


}