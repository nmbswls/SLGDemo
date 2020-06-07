using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour, IGrid_PathSearch
{

    private Camera mCamera;
    public GameObject pawn;
    private Vector2Int nowPawnPos;
    private bool moving;
    // Start is called before the first frame update
    void Start()
    {
        InitPathPot();
        InitPathSearch();

        //{
        //    bool ret = pathComponent.CheckCrossWalkable(new Vector2(3, 3), new Vector2(5, 5));
        //    Debug.Log(ret);
        //}
        {
            bool ret = pathComponent.CheckCrossWalkable(new Vector2(5.5f, 5.5f), new Vector2(0.5f, 0.5f));
            Debug.Log(ret);
        }
        
    }


    public bool showGrid;
    private GridNode[,] grid;//网格

    private Vector2 gridSize;//网格横纵大小
    public float nodeRadius;//格子的半径
    private float nodeDiameter;//格子的直径
    private int gridCntX, gridCntY;//两个方向上的网格数量

    private MeshCollider ShapeCollider;

    void Awake()
    {

        mCamera = Camera.main;
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

        pawn.transform.position = grid[gridCntX / 2, gridCntY / 2]._worldPos + Vector3.up * 1f;
        nowPawnPos.x = gridCntX / 2;
        nowPawnPos.y = gridCntY / 2;

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
        HandleMouseClick();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            showGrid = !showGrid;
        }


        ////获取到玩家所在的网格点
        //tar = GetFromPosition(tarTrans.position);
        ////获取射程网格区域
        //zoneLeftDown = GetFromPosition(tarTrans.position - new Vector3(dir, dir));
        //zoneRightUp = GetFromPosition(tarTrans.position + new Vector3(dir, dir));
        //获取一个随机点
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    int len = FindPath(new Vector2Int(1, 1), new Vector2Int(11, 11));

        //    if (len > 0)
        //    {
        //        ShowPath(len);
        //    }

        //    //Vector3 randPos = GetZoneRandomPos(transform.position, 10f);
        //}
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

    private void ShowPath()
    {

 
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


    Ray ray;
    RaycastHit hit;
    private void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (moving)
            {
                return;
            }

            ray = mCamera.ScreenPointToRay(Input.mousePosition);

            //Ray ray = new Ray(mCamera.transform.position,Vector3.forward);
            
            if (Physics.Raycast(ray, out hit))
            {
                Vector2Int pos = GetFromPosition(hit.point);

                FindPath(nowPawnPos, pos);

                for(int i=0;i<path.Count;i++){
                    Debug.Log(path[i]);
                }


                if (path == null)
                {
                    return;
                }


                nowPawnPos = pos;

                ShowPath();

                StartCoroutine(GoPath());
                //pawn.transform.position = grid[pos.x, pos.y]._worldPos;

                //mark.transform.position = hit.point;
            }
        }   
    }


    IEnumerator GoPath()
    {
        moving = true;
        for (int i = 0; i < path.Count; i++)
        {
            GridNode node = grid[path[i].x, path[i].y];
            pawn.transform.position = node._worldPos;
            yield return new WaitForSeconds(0.1f);
        }
        moving = false;
        ShowPath();
        yield break;
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
        int i = Random.Range(zoneLeftDown.x, zoneRightUp.x);
        int j = Random.Range(zoneLeftDown.y, zoneRightUp.y);

        return grid[i, j]._worldPos;
    }

    //获取整个区域中的一个随机点
    public Vector3 GetZoneRandomPos()
    {
        int i = Random.Range(0, gridCntX);
        int j = Random.Range(0, gridCntY);
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
    List<Vector2Int> path;
    public void InitPathSearch(){
        pathComponent = new PathSearcher(this,100);
    }

    public void FindPath(Vector2Int from, Vector2Int to)
    {
        path = null;
        if (pathComponent == null)
        {
            return ;
        }
        path = pathComponent.FindPath(from, to);
    }

    public bool canPass(Vector2Int from, Vector2Int to)
    {
        return true;
    }

    public Vector2Int getMaxGridXY(){
        return new Vector2Int(gridCntX, gridCntY);
    }

    public bool isBlock(Vector2Int checkPos)
    {
        if (checkPos.x < 0 || checkPos.x >= gridCntX || checkPos.y < 0 || checkPos.y >= gridCntY)
        {
            return true;
        }

        if (grid[checkPos.x, checkPos.y] == null)
        {
            return true;
        }
        return grid[checkPos.x, checkPos.y].blocked;
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

    #endregion
}

public interface IGrid_PathSearch{

    Vector2Int getMaxGridXY();
    bool canPass(Vector2Int from, Vector2Int to);
    bool isBlock(Vector2Int pos);

    float getH(Vector2Int from, Vector2Int to);
    float getG(Vector2Int from, Vector2Int to);
}
public class PathSearcher
{

    public IGrid_PathSearch owner;
    public bool useFloyd = false;
    int maxPathLen;

    public PathSearcher(IGrid_PathSearch owner, int maxPathLen = 200)
    {
        this.owner = owner;
        this.maxPathLen = maxPathLen;
    }


    private class PathSearchNode
    {
        public int x;
        public int y;
        //public GridNode node;
        public float f;
        public float g;
        public float h;
        public PathSearchNode previous;
        public int depth;
    }

    private PriorityQueue<PathSearchNode> openList = new PriorityQueue<PathSearchNode>(new NodeComparer());
    private Dictionary<string, PathSearchNode> openSet = new Dictionary<string, PathSearchNode>();
    private HashSet<string> closeSet = new HashSet<string>();


    private class NodeComparer : IComparer<PathSearchNode>
    {
        public int Compare(PathSearchNode x, PathSearchNode y)
        {
            if (float.Equals(x.f, y.f))
            {
                return 0;
            }
            return x.f > y.f ? -1 : 1;
        }
    }


    public List<Vector2Int> FindPath(Vector2Int from, Vector2Int to)
    {
        List<Vector2Int> pathNodes = new List<Vector2Int>();
        if (!FindPathAstar(from, to, ref pathNodes))
        {
            Debug.Log("?");
            return null;
        }
        if (useFloyd)
        {
            Floyd(pathNodes);

        }
        return pathNodes;
    }

    //直线 直接算距离 
    public bool FindPathAstar(Vector2Int from, Vector2Int to, ref List<Vector2Int> pathNodes)
    {

        if (owner == null || pathNodes == null)
        {
            return false;
        }

        PathSearchNode pNode = null;
        closeSet.Clear();
        openSet.Clear();
        openList.Clear();

        Vector2Int maxGridXY = owner.getMaxGridXY();

        bool found = false;
        PathSearchNode endNode = null;

        PathSearchNode startNode = new PathSearchNode();
        startNode.x = from.x;
        startNode.y = from.y;
        openList.Push(startNode);
        openSet.Add(startNode.x + " " + startNode.y, startNode);


        while (openList.Count > 0)
        {

            pNode = openList.Pop();

            Vector2Int pNodePos = new Vector2Int(pNode.x, pNode.y);

            if (closeSet.Contains(pNode.x + " " + pNode.y))
            {
                continue;
            }
            if (pNode.x == to.x && pNode.y == to.y)
            {
                found = true;
                endNode = pNode;
                break;
            }

            if (pNode.depth > maxPathLen)
            {
                continue;
            }
            int startX = pNode.x - 1 > 0 ? pNode.x - 1 : 0;
            int startY = pNode.y - 1 > 0 ? pNode.y - 1 : 0;
            int endX = pNode.x + 1 <= maxGridXY.x - 1 ? pNode.x + 1 : maxGridXY.x - 1;
            int endY = pNode.y + 1 <= maxGridXY.y - 1 ? pNode.y + 1 : maxGridXY.y - 1;
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    Vector2Int probePos = new Vector2Int(x,y);
                    if (x == pNode.x && y == pNode.y)
                    {
                        continue;
                    }
                    if (owner.isBlock(probePos))
                    {
                        continue;
                    }
                    float gCost = owner.getG(pNodePos, probePos);
                    
                    float g = pNode.g + gCost;
                    float h = owner.getH(probePos,to);
                    float f = g + h;

                    if (closeSet.Contains(x + " " + y))
                    {
                        continue;
                    }

                    if (openSet.ContainsKey(x + " " + y))
                    {
                        if (openSet[x + " " + y].f <= f)
                        {
                            continue;
                        }
                    }

                    PathSearchNode newNode = new PathSearchNode();
                    newNode.x = x;
                    newNode.y = y;
                    newNode.f = f;
                    newNode.g = g;
                    newNode.h = h;
                    newNode.depth = pNode.depth + 1;
                    newNode.previous = pNode;

                    openSet[x + " " + y] = newNode;
                    openList.Push(newNode);
                }
            }
            closeSet.Add(pNode.x + " " + pNode.y);
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
        for (int i = len - 1; i >= 0; i--)
        {
            for (int j = 0; j <= i - 1; j++)
            {
                Vector2 p1 = new Vector2(path[i].x + 0.5f, path[i].y + 0.5f);
                Vector2 p2 = new Vector2(path[j].x + 0.5f, path[j].y + 0.5f);
                if (CheckCrossWalkable(p1, p2))
                {
                    for (int k = i - 1; k >= j; k--)
                    {
                        path.RemoveAt(k);
                    }
                    i = j;
                    //len = path.Count;
                    break;
                }
            }
        }
    }

    //输入值为 点在单元格内百分比位置
    public bool CheckCrossWalkable(Vector2 p1, Vector2 p2)
    {
        if (p1.x > p2.x)
        {
            Vector2 tmp = p1;
            p1 = p2;
            p2 = tmp;
        }

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

        if (!checkWalkable(Vector2Int.FloorToInt(p1), changexz))
        {
            return false;
        }
        
        
        float stepX = p2.x > p1.x ? 1 : (p2.x < p1.x ? -1 : 0);
        float stepY = p2.y > p1.y ? 1 : (p2.y < p1.y ? -1 : 0);
        float deltay = 1 * ((p2.y - p1.y) / Mathf.Abs(p2.x - p1.x));
        
        float nowX = p1.x + stepX / 2;
        float nowY = p1.y - stepY / 2;
        float CheckY = nowY;


        while (nowX < (int)p2.x)
        {

            if (!checkWalkable(new Vector2Int((int)nowX, (int)CheckY), changexz))
            {
                return false;
            }
            nowY += deltay;
            if (nowY >= CheckY + stepY)
            {
                CheckY += stepY;
                if (!checkWalkable(new Vector2Int((int)nowX, (int)CheckY), changexz))
                {
                    return false;
                }
            }
            nowX += stepX;

        }
        return true;
    }
    

    private bool checkWalkable(Vector2Int pos, bool changexz)
    {


        bool ret = false;
        if (changexz)
        {
            ret = owner.isBlock(new Vector2Int(pos.y, pos.x));
        }
        else
        {
            ret = owner.isBlock(new Vector2Int(pos.x, pos.y));
        }

        return !ret;
    }


#endregion


    #region PriorityQueue

    class PriorityQueue<T>
    {
        IComparer<T> comparer;
        T[] heap;

        Dictionary<T, bool> dic = new Dictionary<T, bool>();

        public int Count { get; private set; }

        public PriorityQueue() : this(null) { }
        public PriorityQueue(int capacity) : this(capacity, null) { }
        public PriorityQueue(IComparer<T> comparer) : this(16, comparer) { }

        public PriorityQueue(int capacity, IComparer<T> comparer)
        {
            this.comparer = (comparer == null) ? Comparer<T>.Default : comparer;
            this.heap = new T[capacity];
        }

        public void Push(T v)
        {
            if (Count >= heap.Length) System.Array.Resize(ref heap, Count * 2);
            heap[Count] = v;
            dic[v] = true;
            SiftUp(Count++);
        }

        public T Pop()
        {
            var v = Top();
            heap[0] = heap[--Count];
            if (Count > 0) SiftDown(0);
            dic.Remove(v);
            return v;
        }

        public bool Contains(T v)
        {
            return dic.ContainsKey(v);
        }

        public T Top()
        {
            if (Count > 0) return heap[0];
            throw new System.InvalidOperationException("优先队列为空");
        }

        public void Clear()
        {
            heap = new T[16];
            Count = 0;
            dic.Clear();
        }

        void SiftUp(int n)
        {
            var v = heap[n];
            for (var n2 = n / 2; n > 0 && comparer.Compare(v, heap[n2]) > 0; n = n2, n2 /= 2) heap[n] = heap[n2];
            heap[n] = v;
        }

        void SiftDown(int n)
        {
            var v = heap[n];
            for (var n2 = n * 2; n2 < Count; n = n2, n2 *= 2)
            {
                if (n2 + 1 < Count && comparer.Compare(heap[n2 + 1], heap[n2]) > 0) n2++;
                if (comparer.Compare(v, heap[n2]) >= 0) break;
                heap[n] = heap[n2];
            }
            heap[n] = v;
        }
    }
    #endregion

}