using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour, IGrid_PathSearch
{

    // Start is called before the first frame update
    void Start()
    {
        InitPathPot();
        InitPathSearch();
        //{
        //    bool ret = pathComponent.CheckCrossWalkable(new Vector2(39.5f, 19.5f), new Vector2(16.5f, 22.5f));
        //    Debug.Log(ret);
        //}
        
    }


    public bool showGrid;
    public GridNode[,] grid;//网格

    private Vector2 gridSize;//网格横纵大小
    public float nodeRadius;//格子的半径
    private float nodeDiameter;//格子的直径
    private int gridCntX, gridCntY;//两个方向上的网格数量

    private MeshCollider ShapeCollider;

    void Awake()
    {

        ShapeCollider = GetComponent<MeshCollider>();

        nodeDiameter = nodeRadius * 2;
        if (nodeDiameter == 0)
        {
            return;
        }

        if (ShapeCollider == null)
        {
            return;
        }

        gridSize.x = ShapeCollider.bounds.size.x;
        gridSize.y = ShapeCollider.bounds.size.z;


        gridCntX = Mathf.RoundToInt(gridSize.x / nodeDiameter);
        gridCntY = Mathf.RoundToInt(gridSize.y / nodeDiameter);
        grid = new GridNode[gridCntX, gridCntY];
        CreateGrid();

        showGrid = true;

        
        for (int i = 20; i < 40; i++)
        {
            for (int j = 20; j < 40; j++)
            {
                grid[i, j].blocked = true;
            }
        }
            

    }

    private void Update()
    {

        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            showGrid = !showGrid;
        }
    }


    GameObject[] pathViewList = new GameObject[120];
    public GameObject pathNodeViewPrefab;
    private void InitPathPot()
    {
        for (int i = 0; i < pathViewList.Length; i++)
        {
            GameObject go = GameObject.Instantiate(pathNodeViewPrefab,transform);
            go.SetActive(false);
            pathViewList[i] = go;
        }
    }

    public void ShowPath(List<Vector2Int> path)
    {
        if(path == null)
        {
            for (int i = 0; i < pathViewList.Length; i++)
            {
                pathViewList[i].SetActive(false);
            }
            return;
        }
 
        for (int i = 0; i < path.Count; i++)
        {

            GridNode node = grid[path[i].x, path[i].y];
            pathViewList[i].transform.position = node._worldPos;
            pathViewList[i].SetActive(true);
        }

        for (int i = path.Count; i < pathViewList.Length; i++)
        {
            pathViewList[i].SetActive(false);
        }
    }

    public Vector2Int GetCenter()
    {
        return new Vector2Int(gridCntX / 2, gridCntY / 2);
    }
    
    public Vector3 GetWorldPos(int x, int y)
    {
        return grid[x, y]._worldPos;
    }

    


    [System.Serializable]
    public class GridNode
    {
        public Vector3 _worldPos;//格子中心点的位置
        public bool blocked;
        public GridNode(Vector3 Position)
        {
            _worldPos = Position;
        }
    }

    //创建网格,起始点在左下角
    private void CreateGrid()
    {
        //获得网格的左下角的坐标
        Vector3 startPoint = transform.position - gridSize.x / 2 * Vector3.right - gridSize.y / 2 * Vector3.forward;
        for (int i = 0; i < gridCntX; i++)
        {
            for (int j = 0; j < gridCntY; j++)
            {
                Vector3 worldPoint = startPoint + Vector3.right * (i * nodeDiameter + nodeRadius) + Vector3.forward * (j * nodeDiameter + nodeRadius);
                grid[i, j] = new GridNode(worldPoint);
            }
        }
    }

    //获取某个坐标处的格子
    public Vector2Int GetFromPosition(Vector3 position)
    {

        float percentX = (position.x + gridSize.x / 2) / gridSize.x;
        float percentY = (position.z + gridSize.y / 2) / gridSize.y;

        //保证百分比值在0到1之间
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridCntX - 1) * percentX);
        int y = Mathf.RoundToInt((gridCntY - 1) * percentY);


        return new Vector2Int(x, y);
    }

    //获取一个正方形区域中随机点,length为区域的边长
    public Vector3 GetZoneRandomPos(Vector3 center, float length)
    {
        //射程一定要大于等于0
        //float len = Mathf.Abs(length) / 2;
        //获取射程网格区域
        Vector2Int zoneLeftDown = GetFromPosition(center - new Vector3(length, length));
        Vector2Int zoneRightUp = GetFromPosition(center + new Vector3(length, length));
        //获取并返回射程网格区域中的一个随机点
        int i = UnityEngine.Random.Range(zoneLeftDown.x, zoneRightUp.x);
        int j = UnityEngine.Random.Range(zoneLeftDown.y, zoneRightUp.y);

        return grid[i, j]._worldPos;
    }

    //获取整个区域中的一个随机点
    public Vector3 GetZoneRandomPos()
    {
        int i = UnityEngine.Random.Range(0, gridCntX);
        int j = UnityEngine.Random.Range(0, gridCntY);
        return grid[i, j]._worldPos;
    }

    #region DrawGrid

    static Material lineMaterial;
    static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }
    void OnRenderObject()
    {
        if (grid == null)
        {
            return;
        }
        if (!showGrid)
        {
            return;
        }
        CreateLineMaterial();
        //线条材质
        lineMaterial.SetPass(0);
        GL.PushMatrix();
        //线条颜色,当前材质下，该方式修改颜色无效，详情可以看官方文档
        //GL.Color(MeshColor);
        //绘制线条
        GL.Begin(GL.LINES);

        //合并节点 减少顶点
        for (int i = 0; i < gridCntX; i++)
        {
            for (int j = 0; j < gridCntY; j++)
            {
                GridNode node = grid[i, j];
                float diff = nodeRadius - 0.01f;

                float a = i / (float)gridCntX;
                if (node.blocked)
                {
                    GL.Color(new Color(1, 0, 0.3f));
                    GL.Vertex(node._worldPos + new Vector3(-diff, 0, -diff));
                    GL.Vertex(node._worldPos + new Vector3(diff, 0, diff));

                    GL.Vertex(node._worldPos + new Vector3(diff, 0, -diff));
                    GL.Vertex(node._worldPos + new Vector3(-diff, 0, diff));
                }
                else
                {
                    GL.Color(new Color(0, 1, 0.35f));
                }


                GL.Vertex(node._worldPos + new Vector3(-diff, 0, -diff));
                GL.Vertex(node._worldPos + new Vector3(diff, 0, -diff));

                GL.Vertex(node._worldPos + new Vector3(diff, 0, -diff));
                GL.Vertex(node._worldPos + new Vector3(diff, 0, diff));

                GL.Vertex(node._worldPos + new Vector3(diff, 0, diff));
                GL.Vertex(node._worldPos + new Vector3(-diff, 0, diff));

                GL.Vertex(node._worldPos + new Vector3(-diff, 0, diff));
                GL.Vertex(node._worldPos + new Vector3(-diff, 0, -diff));


            }
        }

        //所有线条 (两点一条线)

        GL.End();
        GL.PopMatrix();
    }

    private void OnDrawGizmos()
    {
        //绘制网格边界线

        if (ShapeCollider == null)
        {
            float x = GetComponent<MeshCollider>().bounds.size.x;
            float y = GetComponent<MeshCollider>().bounds.size.z;
            Gizmos.DrawWireCube(transform.position, new Vector3(x, 0.3f, y));
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(gridSize.x, 0.3f, gridSize.y));
        }

    }
    #endregion

    #region Path

    PathSearcher pathComponent;
    public void InitPathSearch(){
        pathComponent = new PathSearcher(this,100);
    }

    public List<Vector2Int> FindPath(Vector2Int from, Vector2Int to)
    {
        if (pathComponent == null)
        {
            return new List<Vector2Int>();
        }
        
        return pathComponent.FindPath(from, to);
    }

    public bool canPass(Vector2Int from, Vector2Int to)
    {
        return true;
    }

    public Vector2Int getMaxGridXY(){
        return new Vector2Int(gridCntX, gridCntY);
    }

    public int getGridSize()
    {
        return gridCntX * gridCntY;
    }

    public bool isBlock(int x, int y)
    {
        if (x < 0 || x >= gridCntX || y < 0 || y >= gridCntY)
        {
            return true;
        }

        if (grid[x, y] == null)
        {
            return true;
        }
        return grid[x, y].blocked;
    }

    public float getH(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y); 
    }

    public float getG(Vector2Int from, Vector2Int to)
    {
        float cost = 1f;
        if (!(from.x == to.x || from.y == to.y))
        {
            cost = 1.4f;
        }
        return cost;
    }

    public List<Vector2Int> getNeighbours(int x, int y)
    {
        Vector2Int maxGridXY = getMaxGridXY();

        int startX = x - 1 > 0 ? x - 1 : 0;
        int startY = y - 1 > 0 ? y - 1 : 0;
        int endX = x + 1 <= maxGridXY.x - 1 ? x + 1 : maxGridXY.x - 1;
        int endY = y + 1 <= maxGridXY.y - 1 ? y + 1 : maxGridXY.y - 1;

        List<Vector2Int> ret = new List<Vector2Int>();
        
        for (int xx = startX; xx <= endX; xx++)
        {
            for (int yy = startY; yy <= endY; yy++)
            {
                if(xx == x && yy == y)
                {
                    continue;
                }
                ret.Add(new Vector2Int(xx, yy));
            }
        }
        return ret;
    }

    #endregion
}

public interface IGrid_PathSearch{

    int getGridSize();
    Vector2Int getMaxGridXY();
    bool canPass(Vector2Int from, Vector2Int to);
    bool isBlock(int x, int y);

    float getH(Vector2Int from, Vector2Int to);
    float getG(Vector2Int from, Vector2Int to);
    List<Vector2Int> getNeighbours(int x, int y);

}
public class PathSearcher
{

    public IGrid_PathSearch owner;
    public bool useFloyd = true;
    int maxPathLen;

    private Heap<PathSearchNode> openList;
    //private PriorityQueue<PathSearchNode> openList = new PriorityQueue<PathSearchNode>(new NodeComparer());

    private Dictionary<string, PathSearchNode> openSet;
    private HashSet<string> closeSet;

    private Vector2Int targetPos;

    public PathSearcher(IGrid_PathSearch owner, int maxPathLen = 200)
    {
        this.owner = owner;
        this.maxPathLen = maxPathLen;
    }


    private class PathSearchNode : IHeapItem<PathSearchNode>
    {
        public int x;
        public int y;
        //public GridNode node;
        public float f;
        public float g;
        public float h;
        public PathSearchNode previous;
        public int depth;

        private int _heapIndex = 0;
        public int HeapIndex {
            get {return _heapIndex; }
            set { _heapIndex = value; }
        }

        public int CompareTo(PathSearchNode other)
        {

            int compare = f.CompareTo(other.f);
            if (compare == 0)
            {
                compare = h.CompareTo(other.h);
            }
            return -compare;
        }

        public override string ToString()
        {
            return x.ToString() + "," + y.ToString();
        }
    }

    private void Reset()
    {
        openList = new Heap<PathSearchNode>(owner.getGridSize());
        openSet = new Dictionary<string, PathSearchNode>();
        closeSet = new HashSet<string>();
    }


    public List<Vector2Int> FindPath(Vector2Int from, Vector2Int to)
    {
        List<Vector2Int> pathNodes = new List<Vector2Int>();
        if (!FindPathAstar(from, to, ref pathNodes))
        {
            return null;
        }
        if (useFloyd)
        {
            Floyd(pathNodes);

        }
        return pathNodes;
    }

    private string pos2key(int x, int y)
    {
        return x + " " + y;
    }

    //直线 直接算距离 
    public bool FindPathAstar(Vector2Int from, Vector2Int to, ref List<Vector2Int> pathNodes)
    {

        if (owner == null || pathNodes == null)
        {
            return false;
        }

        PathSearchNode pNode = null;
        Reset();


        bool found = false;
        PathSearchNode endNode = null;

        PathSearchNode startNode = new PathSearchNode();
        startNode.x = from.x;
        startNode.y = from.y;
        openList.Add(startNode);
        openSet.Add(pos2key(from.x, from.y), startNode);


        while (openList.Count > 0)
        {
            pNode = openList.RemoveFirst();

            Vector2Int pNodePos = new Vector2Int(pNode.x, pNode.y);
            if (pNode.x == to.x && pNode.y == to.y)
            {
                found = true;
                endNode = pNode;
                break;
            }

            //只有不修改原值 压入新值时才需要这一步
            //if (closeSet.Contains(pos2key(pNode.x,pNode.y)))
            //{
            //    continue;
            //}
            

            if (pNode.depth > maxPathLen)
            {
                continue;
            }

            List<Vector2Int> neighbours = owner.getNeighbours(pNode.x, pNode.y);
            for(int i = 0; i < neighbours.Count; i++)
            {

                Vector2Int probePos = neighbours[i];
                
                if (owner.isBlock(probePos.x, probePos.y))
                {
                    continue;
                }
                string probeKey = pos2key(probePos.x, probePos.y);
                if (closeSet.Contains(probeKey))
                {
                    continue;
                }

                float newG = pNode.g + owner.getG(pNodePos, probePos);
                float newH = owner.getH(probePos, to);
                float newF = newG + newH;

                if (openSet.ContainsKey(probeKey))
                {
                    if (openSet[probeKey].g <= newG)
                    {
                        continue;
                    }
                    PathSearchNode oldNode = openSet[probeKey];
                    oldNode.f = newF;
                    oldNode.g = newG;
                    oldNode.h = newH;
                    oldNode.depth = pNode.depth + 1;
                    oldNode.previous = pNode;
                    openList.UpdateItem(oldNode);
                }
                else
                {
                    PathSearchNode newNode = new PathSearchNode();
                    newNode.x = probePos.x;
                    newNode.y = probePos.y;
                    newNode.f = newF;
                    newNode.g = newG;
                    newNode.h = newH;
                    newNode.depth = pNode.depth + 1;
                    newNode.previous = pNode;

                    openSet[probeKey] = newNode;
                    openList.Add(newNode);
                }
            }
            closeSet.Add(pos2key(pNode.x, pNode.y));
        }

        if (!found)
        {
            return false;
        }
        PathSearchNode p = endNode;
        int pathLen = p.depth + 1;
        while (p != null)
        {
            pathNodes.Insert(0, new Vector2Int(p.x, p.y));
            p = p.previous;
        }
        return true;
    }


    //private bool FindPathJPS(Vector2Int from, Vector2Int to)
    //{
    //    PathSearchNode currentNode;

    //    PathSearchNode startNode = new PathSearchNode();
    //    startNode.x = from.x;
    //    startNode.y = from.y;

    //    openList.Push(startNode);
    //    openSet.Add(startNode.x + " " + startNode.y, startNode);



    //    while (openList.Count > 0)
    //    {
    //        currentNode = openSet.RemoveFirst();
    //        openSetContainer.Remove(_startNode);

    //        if (currentNode == _targetNode)
    //        {
    //            return true;
    //        }


    //        closedSet.Add(currentNode);
    //        List<Node> Nodes = _GetSuccessors(currentNode);

    //        foreach (Node node in Nodes)
    //        {
    //            jumpNodes.Add(node);

    //            if (closedSet.Contains(node))
    //                continue;

    //            int newGCost = currentNode.gCost + _GetDistance(currentNode, node);
    //            if (newGCost < node.gCost || !openSetContainer.Contains(node))
    //            {
    //                node.gCost = newGCost;
    //                node.hCost = _GetDistance(node, _targetNode);
    //                node.parent = currentNode;

    //                if (!openSetContainer.Contains(node))
    //                {
    //                    openSetContainer.Add(node);
    //                    openSet.Add(node);
    //                }
    //                else
    //                {
    //                    openSet.UpdateItem(node);
    //                }
    //            }
    //        }

    //    }
    //    return false;
    //}


    //private List<Node> _GetSuccessors(Node currentNode)
    //{
    //    Node jumpNode;
    //    List<Node> successors = new List<Node>();
    //    List<Node> neighbours = _grid.GetNeighbours(currentNode);

    //    foreach (Node neighbour in neighbours)
    //    {
    //        int xDirection = neighbour.x - currentNode.x;
    //        int yDirection = neighbour.y - currentNode.y;

    //        jumpNode = _Jump(neighbour, currentNode, xDirection, yDirection);

    //        if (jumpNode != null)
    //            successors.Add(jumpNode);
    //    }
    //    return successors;
    //}


    //private bool _Jump(Vector2Int currentNode, Vector2Int parentNode, int xDirection, int yDirection, out Vector2Int ret)
    //{
    //    ret = Vector2Int.zero;

    //    if (currentNode == null || owner.isBlock(currentNode.x, currentNode.y))
    //        return false;
    //    if (currentNode == targetPos)
    //    {
    //        _forced = true;
    //        ret = currentNode;
    //        return true;
    //    }

    //    _forced = false;
    //    if (xDirection != 0 && yDirection != 0)
    //    {
    //        if ((owner.isBlock(currentNode.x - xDirection, currentNode.y) && !owner.isBlock(currentNode.x - xDirection, currentNode.y + yDirection)) ||
    //            (owner.isBlock(currentNode.x, currentNode.y - yDirection) && !owner.isBlock(currentNode.x + xDirection, currentNode.y - yDirection)))
    //        {
    //            ret = currentNode;
    //            return true;
    //        }


    //        Vector2Int nextHorizontalNode = new Vector2Int(currentNode.x + xDirection, currentNode.y);
    //        Vector2Int nextVerticalNode = new Vector2Int(currentNode.x, currentNode.y + yDirection);
    //        if (nextHorizontalNode == null || nextVerticalNode == null)
    //        {
    //            bool found = false;
    //            if (nextHorizontalNode != null && !owner.isBlock(currentNode.x + xDirection, currentNode.y + yDirection))
    //            {
    //                found = true;
    //            }
    //            if (nextVerticalNode != null && owner.isBlock(currentNode.x + xDirection, currentNode.y + yDirection))
    //            {
    //                found = true;
    //            }

    //            if (!found)
    //                return false;
    //        }
    //        Vector2Int outV2;
    //        if (_Jump(nextHorizontalNode, currentNode, xDirection, 0, out outV2) || _Jump(nextVerticalNode, currentNode, 0, yDirection, out outV2))
    //        {
    //            if (!_forced)
    //            {
    //                UnityEngine.Debug.Log(currentNode);
    //                Vector2Int temp = new Vector2Int(currentNode.x + xDirection, currentNode.y + yDirection);
    //                if (temp != null && _grid.showDebug)
    //                    UnityEngine.Debug.DrawLine(new Vector3(currentNode.x, 1, currentNode.y), new Vector3(temp.x, 1, temp.y), Color.green, Mathf.Infinity);
    //                return _Jump(temp, currentNode, xDirection, yDirection);
    //            }
    //            else
    //            {
    //                return currentNode;
    //            }
    //        }
    //    }
    //    else
    //    {
    //        if (xDirection != 0)
    //        {
    //            if ((_grid.IsWalkable(currentNode.x + xDirection, currentNode.y + 1) && !_grid.IsWalkable(currentNode.x, currentNode.y + 1)) ||
    //                (_grid.IsWalkable(currentNode.x + xDirection, currentNode.y - 1) && !_grid.IsWalkable(currentNode.x, currentNode.y - 1)))
    //            {
    //                _forced = true;
    //                return currentNode;
    //            }
    //        }
    //        else
    //        {
    //            if ((_grid.IsWalkable(currentNode.x + 1, currentNode.y + yDirection) && !_grid.IsWalkable(currentNode.x + 1, currentNode.y)) ||
    //                (_grid.IsWalkable(currentNode.x - 1, currentNode.y + yDirection) && !_grid.IsWalkable(currentNode.x - 1, currentNode.y)))
    //            {
    //                _forced = true;
    //                return currentNode;
    //            }
    //        }
    //    }
    //    Node nextNode = _grid.GetNodeFromIndex(currentNode.x + xDirection, currentNode.y + yDirection);
    //    if (nextNode != null && _grid.showDebug)
    //        UnityEngine.Debug.DrawLine(new Vector3(currentNode.x, 1, currentNode.y), new Vector3(nextNode.x, 1, nextNode.y), Color.green, Mathf.Infinity);
    //    return _Jump(nextNode, currentNode, xDirection, yDirection);
    //}


    #region floyd

    //平滑路径 弗洛伊德算法
    public void Floyd(List<Vector2Int> path)
    {

        if (path == null)
        {
            return;
        }

        int len = path.Count;

        if (len == 0 || len == 1)
        {
            return;
        }

        //去掉同一条线上的点。
        Vector2 vector = path[len - 1] - path[len - 2];
        Vector2 tempvector;
        for (int i = len - 3; i >= 0; i--)
        {
            tempvector = path[i + 1] - path[i];
            float angle = Vector2.Angle(vector, tempvector);
            if (angle == 0 || angle == 180)
            {
                path.RemoveAt(i + 1);
            }
            else
            {
                vector = tempvector;
            }
        }


        //去掉无用拐点
        len = path.Count;
        for (int i = len - 1; i >= 2; i--)
        {
            for (int j = 0; j <= i - 2; j++)
            {
                Vector2 p1 = new Vector2(path[i].x + 0.5f, path[i].y + 0.5f);
                Vector2 p2 = new Vector2(path[j].x + 0.5f, path[j].y + 0.5f);
                if (CheckCrossWalkable(p1, p2))
                {
                    for (int k = i - 1; k > j; k--)
                    {
                        path.RemoveAt(k);
                    }
                    i = j;
                    break;
                }
            }
        }
    }

    //输入值为 点在单元格内百分比位置
    //只支持非负
    //只支持中心开始
    public bool CheckCrossWalkable(Vector2 p1, Vector2 p2)
    {

        bool changexz = Mathf.Abs(p2.y - p1.y) > Mathf.Abs(p2.x - p1.x);

        if (changexz)
        {
            float temp = p1.x;
            p1.x = p1.y;
            p1.y = temp;
            temp = p2.x;
            p2.x = p2.y;
            p2.y = temp;
        }

        if (p1.x == p2.x && p1.y == p2.y)
        {
            return true;
        }

        if(p1.x > p2.x)
        {
            Vector2 tmp = p1;
            p1 = p2;
            p2 = tmp;
        }

        

        if (!checkWalkable(Vector2Int.FloorToInt(p1), changexz))
        {
            return false;
        }

        float deltay = 1.0f * ((p2.y - p1.y) / (p2.x - p1.x));
        
        float nowX = p1.x + 0.5f;
        float nowY = p1.y + deltay / 2;
        float lastY;

        //for (int i = (int)start.y; i <= (int)nowY; i++)
        //{
        //    if (checkY((int)start.y, nowY, xInt - 1))
        //    {
        //        return true;
        //    }
        //}



        while (nowX < p2.x)
        {
            lastY = nowY;
            nowY += deltay;
            if (!checkYStripWalkable(lastY, nowY, (int)nowX, changexz))
            {
                return false;
            }
            nowX += 1;
        }

        //for (int i = (int)lastY; i <= (int)end.y; i++)
        //{
        //    if (checkY(lastY, (int)end.y, xInt - 1))
        //    {
        //        return true;
        //    }
        //}


        return true;
    }


    public bool checkYStripWalkable(float yf, float yt, int x, bool changexz)
    {
        if (yf > yt)
        {
            float tmp = yf;
            yf = yt;
            yt = tmp;
        }
        for (int y = (int)yf; y <= (int)yt; y++)
        {
            if (!checkWalkable(new Vector2Int(x,y), changexz))
            {
                return false;
            }
        }
        return true;
    }



    private bool checkWalkable(Vector2Int pos, bool changexz)
    {
        bool ret;
        if (changexz)
        {
            ret = owner.isBlock(pos.y, pos.x);
            //Debug.Log("check " + pos.y + " " + pos.x + " " + ret);
            //Grid grid = owner as Grid;
            //Vector3 showPos = grid.grid[pos.y, pos.x]._worldPos;
            //GameObject.Instantiate(grid.pathNodeViewPrefab, showPos, Quaternion.identity);
        }
        else
        {
            ret = owner.isBlock(pos.x, pos.y);
            //Debug.Log("check " + pos.x + " " + pos.y + " " + ret);
            //Grid grid = owner as Grid;
            //Vector3 showPos = grid.grid[pos.x, pos.y]._worldPos;
            //GameObject.Instantiate(grid.pathNodeViewPrefab, showPos, Quaternion.identity);
        }

        return !ret;
    }


    #endregion


    #region Heap

    public class Heap<T> where T : IHeapItem<T>
    {
        private T[] _items = null;
        private int _currentItemCount = 0;
        public int Count
        {
            get
            {
                return _currentItemCount;
            }
        }

        public Heap(int MaxHeapSize)
        {
            _items = new T[MaxHeapSize];
        }

        public T[] ToArray()
        {
            return _items;
        }

        public void Add(T item)
        {
            item.HeapIndex = _currentItemCount;
            _items[_currentItemCount] = item;
            _SortUp(item);
            _currentItemCount++;
        }

        public T RemoveFirst()
        {
            T firstItem = _items[0];
            _currentItemCount--;
            _items[0] = _items[_currentItemCount];
            _items[0].HeapIndex = 0;
            _SortDown(_items[0]);
            return firstItem;
        }

        public void UpdateItem(T item)
        {
            _SortUp(item);
        }

        public bool Contains(T item)
        {
            return Equals(_items[item.HeapIndex], item);
        }

        private void _SortUp(T item)
        {
            int parentIndex = (item.HeapIndex - 1) / 2;

            while (true)
            {
                T parentItem = _items[parentIndex];

                if (item.CompareTo(parentItem) > 0)
                    _Swap(item, parentItem);
                else
                    break;

                parentIndex = (item.HeapIndex - 1) / 2;
            }
        }

        private void _SortDown(T item)
        {
            while (true)
            {
                int childLeftIndex = (item.HeapIndex * 2) + 1;
                int childRightIndex = (item.HeapIndex * 2) + 2;
                int swapIndex = 0;

                if (childLeftIndex < _currentItemCount)
                {
                    swapIndex = childLeftIndex;

                    if (childRightIndex < _currentItemCount)
                        if (_items[childLeftIndex].CompareTo(_items[childRightIndex]) < 0)
                            swapIndex = childRightIndex;

                    if (item.CompareTo(_items[swapIndex]) < 0)
                        _Swap(item, _items[swapIndex]);
                    else
                        return;
                }
                else
                    return;
            }
        }

        private void _Swap(T itemA, T itemB)
        {
            _items[itemA.HeapIndex] = itemB;
            _items[itemB.HeapIndex] = itemA;

            int tempItemAIndex = itemA.HeapIndex;
            itemA.HeapIndex = itemB.HeapIndex;
            itemB.HeapIndex = tempItemAIndex;
        }
    }

    public interface IHeapItem<T> : IComparable<T>
    {
        int HeapIndex
        {
            get; set;
        }
    }

    //class PriorityQueue<T>
    //{
    //    IComparer<T> comparer;
    //    T[] heap;

    //    Dictionary<T, bool> dic = new Dictionary<T, bool>();

    //    public int Count { get; private set; }

    //    public PriorityQueue() : this(null) { }
    //    public PriorityQueue(int capacity) : this(capacity, null) { }
    //    public PriorityQueue(IComparer<T> comparer) : this(16, comparer) { }

    //    public PriorityQueue(int capacity, IComparer<T> comparer)
    //    {
    //        this.comparer = (comparer == null) ? Comparer<T>.Default : comparer;
    //        this.heap = new T[capacity];
    //    }

    //    public void Push(T v)
    //    {
    //        if (Count >= heap.Length) System.Array.Resize(ref heap, Count * 2);
    //        heap[Count] = v;
    //        dic[v] = true;
    //        SiftUp(Count++);
    //    }

    //    public T Pop()
    //    {
    //        var v = Top();
    //        heap[0] = heap[--Count];
    //        if (Count > 0) SiftDown(0);
    //        dic.Remove(v);
    //        return v;
    //    }

    //    public bool Contains(T v)
    //    {
    //        return dic.ContainsKey(v);
    //    }

    //    public T Top()
    //    {
    //        if (Count > 0) return heap[0];
    //        throw new System.InvalidOperationException("优先队列为空");
    //    }

    //    public void Clear()
    //    {
    //        heap = new T[16];
    //        Count = 0;
    //        dic.Clear();
    //    }

    //    void SiftUp(int n)
    //    {
    //        var v = heap[n];
    //        for (var n2 = n / 2; n > 0 && comparer.Compare(v, heap[n2]) > 0; n = n2, n2 /= 2) heap[n] = heap[n2];
    //        heap[n] = v;
    //    }

    //    void SiftDown(int n)
    //    {
    //        var v = heap[n];
    //        for (var n2 = n * 2; n2 < Count; n = n2, n2 *= 2)
    //        {
    //            if (n2 + 1 < Count && comparer.Compare(heap[n2 + 1], heap[n2]) > 0) n2++;
    //            if (comparer.Compare(v, heap[n2]) >= 0) break;
    //            heap[n] = heap[n2];
    //        }
    //        heap[n] = v;
    //    }
    //}
    #endregion

}