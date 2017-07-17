using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathNode
{
    //노드의 위치값
    public Vector2 centerPos;           //노드의 센터위치
    public Vector2 minPos;              //노드의 최소위치
    public Vector2 maxPos;              //노드의 최대위치

    //노드의 인덱스
    public int indexX;                  
    public int indexY;

    //임직일수 잇는 노드 확인
    public bool isMoveble = true;

    //astar 패스 계산시 정보
    public PathNode pParent;            //너는 누구로 부터 왓니
    public int costToGoal;              //목적지까지의 비용
    public int costFromtStart = 0;      //시작위치로부터의 간략화된 비용
    public int costTotal = 0;           //코스트 토탈

    //노드 생성자
    public PathNode(int x, int y)
    {
        this.indexX = x;
        this.indexY = y;
        this.Reset();
    }

    //노드 초기화
    //경로 검색전 반드시 초기화 되어 있어야 한다.
    public void Reset()
    {
        this.pParent = null;
        this.costToGoal = 0;
        this.costFromtStart = 0;
        this.costTotal = 0;
    }
}

public class MakePlaneScript : MonoBehaviour {

    //노드 배열
    private PathNode[,] pathNodes;

    //한 노드의 크기
    private float worldWidth  = 0.0f;               //월드 가로
    private float worldHeight = 0.0f;               //월드 세로

    //셀 갯수
    public int cellCount_Width = 20;                   //월드 가로 셀갯수
    public int cellCount_Height = 20;                  //월드 세로 셀갯수
    
    //월드 위치들
    [SerializeField]
    private Vector2 worldMinPos;                    //월드 최소 위치
    [SerializeField]
    private Vector2 worldMaxPos;                    //월드 최대 위치
    [SerializeField]
    private Vector2 worldStartPos;                  //월드 시작 위치

    private List<PathNode> openNodeList = new List<PathNode>();     //오픈노드 리스트
    private List<PathNode> closeNodeList = new List<PathNode>();    //클로즈 노드 리스트

    private List<PathNode> result = new List<PathNode>();           //결과 벡터
    private List<PathNode> resultStack = new List<PathNode>();      //결과 스택(백트레킹 때문에 쓴다.)



    private PathNode startNode;         //시작 노드
    private PathNode endNode;           //목적지 노드
    private bool bFindGoal;             //목적지 찾은값

    [SerializeField]
    private GameObject cell;

    void Awake()
    {
        //콜리더 바운드로 최대 최소영역 계산
        //Bounds colBound = this.GetComponent<Collider>().bounds;       //콜리더의 AABB바운드 영역
        //this.worldMinPos = colBound.min;
        //this.worldMaxPos = colBound.max;

        float fWidth = (cellCount_Width / 2.0f);
        float fHeight = (cellCount_Height / 2.0f);

        //월드 시작위치
        worldStartPos = new Vector2(-fWidth, -fHeight);
        worldMinPos = new Vector2(-fWidth, -fHeight);
        worldMaxPos = new Vector2(fWidth, fHeight);

        //월드의 크기
        //this.worldWidth  = this.worldMaxPos.x - this.worldMinPos.x;
        //this.worldHeight = this.worldMaxPos.y - this.worldMinPos.y;

        //셀하나의 크기
        //this.cellWidth = this.worldWidth / cellWidthNum;
        //this.cellHeight = this.worldHeight / cellHeightNum;

        //노드 배열 생성
        pathNodes = new PathNode[cellCount_Width, cellCount_Height];
        for (int x = 0; x < this.cellCount_Width; x++)
        {
            for (int y = 0; y < this.cellCount_Height; y++)
            {
                this.pathNodes[x, y] = new PathNode(x, y);

                //셀마다 민포스
                pathNodes[x, y].minPos =
                    worldStartPos +
                    (Vector2.right * x * 1f) +
                    (Vector2.up * y * 1f);

                //셀마다 맥포스
                pathNodes[x, y].maxPos = pathNodes[x, y].minPos +
                    (Vector2.right * 1f) +
                    (Vector2.up * 1f);

                //셀마다 센터포스
                pathNodes[x, y].centerPos =
                    (pathNodes[x, y].minPos + pathNodes[x, y].maxPos) * 0.5f;

                GameObject go = Instantiate(cell, pathNodes[x, y].centerPos, Quaternion.identity,this.transform) as GameObject;

                if (x == 0 || x == cellCount_Width - 1 ||
                    y == 0 || y == cellCount_Height - 1)
                    go.GetComponent<BoxCollider>().enabled = false;
            }
        }
    }

    /// <summary>
    /// 알고리즘으로 경로 찾기
    /// </summary>
    /// <param name="startPos">월드 시작 위치</param>
    /// <param name="endPos">월드 종료 위치</param>
    /// <param name="bDiagonal">대각 갈수있는</param>
    /// <returns></returns>
    public List<PathNode> FindPath(Vector2 startPos, Vector2 endPos, bool bDiagonal)
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
        PathNode curNode = startNode;       //현재 검색노드


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
                PathNode node = this.endNode;

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
    public PathNode GetPathNode(Vector2 pos)
    {
        if (pos.x > this.worldMinPos.x &&
            pos.x < this.worldMaxPos.x &&
            pos.y > this.worldMinPos.y &&
            pos.y < this.worldMaxPos.y)
        {
            int idxX = (int)((pos.x - this.worldMinPos.x));
            int idxY = (int)((pos.y - this.worldMinPos.y));

            return this.pathNodes[idxX, idxY];
        }


        return null;
    }

    //해당 인덱스의 위치가 갈수 있는 노드인지 확인
    bool isMoveable(int indexX, int indexZ)
    {
        if (indexX >= 0 && indexX < this.cellCount_Width &&
            indexZ >= 0 && indexZ < this.cellCount_Height)
        {
            return this.pathNodes[indexX, indexZ].isMoveble;
        }

        return false;
    }

    /// <summary>
    /// 현재 노드 중심으로 갈수있는 노드를 오픈리스트에 추가
    /// </summary>
    /// <param name="curNode">현재노드</param>
    /// <param name="bDiagonal">대각 검색</param>
    void CheckAround(PathNode curNode, bool bDiagonal)
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
    void AddOpenList(int indexX, int indexY, PathNode parent)
    {
        PathNode node = this.pathNodes[indexX, indexY];

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

    int GetCostToGoal(PathNode node)
    {
        return Mathf.Abs(node.indexX - endNode.indexX) + Mathf.Abs(node.indexY - endNode.indexY);
    }


    PathNode GetMiniumNodeFromOpenNode()
    {
        if (this.openNodeList.Count == 0)
            return null;

        PathNode miniumNode = this.openNodeList[0];
        for (int i = 1; i < this.openNodeList.Count; i++)
        {
            if (this.openNodeList[i].costTotal < miniumNode.costTotal)
                miniumNode = this.openNodeList[i];
        }

        this.openNodeList.Remove(miniumNode);

        this.closeNodeList.Add(miniumNode);

        return miniumNode;
    }

    //void OnDrawGizmos()
    //{
    //    Bounds colBound = this.GetComponent<Collider>().bounds;
    //
    //    Vector2 min = colBound.min;
    //    Vector2 max = colBound.max;
    //
    //    Gizmos.DrawLine(new Vector2(min.x, min.y), new Vector2(min.x, max.y));
    //    Gizmos.DrawLine(new Vector2(min.x, max.y), new Vector2(max.x, max.y));
    //    Gizmos.DrawLine(new Vector2(max.x, min.y), new Vector2(max.x, max.y));
    //    Gizmos.DrawLine(new Vector2(max.x, min.y), new Vector2(min.x, min.y));
    //
    //    Gizmos.color = Color.blue;
    //
    //    if (this.pathNodes != null)
    //    {
    //        for (int x = 0; x < this.cellWidthNum; x++)
    //        {
    //            for (int y = 0; y < this.cellHeightNum; y++)
    //            {
    //                //갈수 없는곳
    //                if (this.pathNodes[x, y].isMoveble == false)
    //                {
    //                    Gizmos.DrawCube(
    //                        this.pathNodes[x, y].centerPos,
    //                        new Vector2(this.cellWidth, this.cellHeight));
    //                }
    //                //갈수 있는곳
    //                else
    //                {
    //                    Gizmos.DrawWireCube(
    //                        this.pathNodes[x, y].centerPos,
    //                        new Vector2(this.cellWidth, this.cellHeight));
    //                }
    //            }
    //        }
    //    }
    //}
}
