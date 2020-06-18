using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattleManager : MonoBehaviour
{
    //public delegate void EffectCallback(EffectNode ctx);

    public Grid grid1;
    private Camera mCamera;

    public GameObject TurnMark;
    public BaseUnit pawn;
    //private Vector2Int nowPawnPos;

    private int NowRound = 0;

    public static BattleManager Instance;
    public GameObject BaseUnitPrefab;


    public List<BaseUnit> BattleUnits = new List<BaseUnit>();
    public Dictionary<Int64, BaseUnit> AllUnitDict = new Dictionary<long, BaseUnit>();


    private SceneClickable gridListener;

    public HudCtrl hudCtrl;
    public RangeIndicator mRangeIndicator;
    //public List<FakeActor> fakeActors = new List<FakeActor>();
    //public class FakeActor 
    //{
    //    public int id;
    //    public int speed;
    //}


    // Start is called before the first frame update
    void Start()
    {
        if(Instance == null)
        {
            Instance = this;
        }

        mCamera = Camera.main;

        InitBattle();
        //nowPawnPos = grid1.GetCenter();
        //pawn.transform.position = grid1.GetWorldPos(nowPawnPos.x, nowPawnPos.y);

        gridListener = grid1.GetComponent<SceneClickable>();
        gridListener.ClickEvent += OnGridClick;


        GameObject indicatorPrefab = Resources.Load("RangeIndicator") as GameObject;
        GameObject go = GameObject.Instantiate(indicatorPrefab);

        mRangeIndicator = go.GetComponent<RangeIndicator>();
        mRangeIndicator.Init();
    }

    private void OnDestroy()
    {
        //unregister
        gridListener.ClickEvent -= OnGridClick;
    }


    void Update()
    {
        //TickAbility();
        TickAction();
        TickMove();

        

        ProjectileManager.Instance.Tick(Time.deltaTime);
    }


    public bool HandleStartAttack()
    {
        if (nowTurnActor == null)
        {
            return false;
        }
        nowAbility = nowTurnActor.AbilityList[0];
        if (nowAbility == null)
        {
            return false;
        }
        if ((nowAbility.Config.targetType & (int)eAbilityTargetType.Target) != 0)
        {
            switchMouseState(eMouseState.ChooseTarget); 
            pathPreview = null;
            ShowAtkRange();
        }
        else if ((nowAbility.Config.targetType & (int)eAbilityTargetType.Point) != 0)
        {
            switchMouseState(eMouseState.ChoosePoint);
            pathPreview = null;
            ShowAtkRange();
        }
        else
        {
            nowAbility.TryUseAbility();
            return false;
        }
        return true;
    }

    #region handleMouse



    public enum eMouseState
    {
        Normal,  //点地移动 点人看属性
        ChooseTarget,  //点地无效 点人攻击
        ChoosePoint,  //点地攻击 点人也攻击
    }

    private eMouseState mouseState = eMouseState.Normal;
    private Ability nowAbility;


    private void switchMouseState(eMouseState newState)
    {
        mouseState = newState;
        pathPreview = null;
        grid1.ShowPathPots(null);
        if(newState == eMouseState.Normal)
        {
            HideAtkRange();
        }
    }

    public void OnActorClick(BaseUnit target)
    {
        switch (mouseState)
        {
            case eMouseState.Normal:
                {
                    Debug.Log("show info");
                }
                hudCtrl.ShowInfo(target);
                break;
            case eMouseState.ChoosePoint:
                {
                    if (nowAbility.TryUseAbility(target))
                    {
                        hudCtrl.SwitchAttackBtn();
                        HandleCancelAttack();
                    }
                    
                }
                break;
            case eMouseState.ChooseTarget:
                {
                    if (nowAbility.TryUseAbility(target))
                    {
                        hudCtrl.SwitchAttackBtn();
                        HandleCancelAttack();
                    }
                }
                break;
            default:
                break;
        }
    }

    private void OnGridClick(SceneClickData data)
    {
        Vector2Int pos = grid1.GetFromPosition(data.PosInWorld);

        switch (mouseState)
        {
            case eMouseState.Normal:
                {
                    CalcPath(pos);
                }
                break;
            case eMouseState.ChoosePoint:
                {
                    break;
                }
            case eMouseState.ChooseTarget:
                //不处理
                break;
            default:
                break;
        }
    }

    public bool HandleCancelAttack()
    {
        if(mouseState == eMouseState.Normal)
        {
            Debug.Log("ui error");
            return false;
        }

        switchMouseState(eMouseState.Normal);
        return true;
    }


    public void ShowAtkRange()
    {
        mRangeIndicator.SetActive(true);
        mRangeIndicator.SetPosition(nowTurnActor.transform.position);
        mRangeIndicator.SetRange(nowAbility.Config.RangeInt);
    }

    public void HideAtkRange()
    {
        if(mRangeIndicator)
            mRangeIndicator.SetActive(false);
    }


    #endregion



    

    

    bool Locked
    {
        get { return lockedCnt > 0; }
    }
    int lockedCnt;
    public void Lock()
    {
        lockedCnt++;
    }

    public void Unlock()
    {
        if(lockedCnt > 0)
        {
            lockedCnt--;
        }
    }


    #region round

    public class ActorTurnNode : System.IComparable<ActorTurnNode>
    {
        public BaseUnit battleActor;
        public int prevIdx;
        public int nowIdx;

        public int CompareTo(ActorTurnNode obj)  //实现该比较方法即可
        {
            return battleActor.stats.speed.CompareTo(obj.battleActor.stats.speed);
        }
    }

    List<ActorTurnNode> NowRoundActionSeq;
    List<ActorTurnNode> NextRoundActionSeq;
    public BaseUnit nowTurnActor;
    bool isPlayerTurn;

    void InitBattle()
    {

        for (int i = 0; i < 5; i++)
        {
            GameObject go = Instantiate(BaseUnitPrefab);
            BaseUnit unit = go.GetComponent<BaseUnit>();
            unit.InstId = i;
            unit.name = "小兵" + i;
            unit.stats.maxHp = 100;
            unit.stats.hp = 100;
            unit.stats.speed = UnityEngine.Random.Range(10,20);
            if(i == 0)
            {
                unit.nowGridPos = grid1.GetCenter();
            }
            else
            {
                unit.nowGridPos = new Vector2Int(UnityEngine.Random.Range(50, 70), UnityEngine.Random.Range(50, 70));
            }
            unit.Init();
            unit.AdjustPos();
            BattleUnits.Add(unit);
        }

        pawn = BattleUnits[0];

        NextRoundActionSeq = new List<ActorTurnNode>();

        for (int i = 0; i < BattleUnits.Count; i++)
        {
            ActorTurnNode newNode = new ActorTurnNode();
            newNode.battleActor = BattleUnits[i];
            NextRoundActionSeq.Add(newNode);
        }
        NextRoundActionSeq.Sort();

        for (int i = 0; i < NextRoundActionSeq.Count; i++)
        {
            NextRoundActionSeq[i].nowIdx = i;
            NextRoundActionSeq[i].prevIdx = i;
        }

        NextRound();

    }
    public void NextRound()
    {
        NowRound++;
        NowRoundActionSeq = NextRoundActionSeq;
        NextRoundActionSeq = new List<ActorTurnNode>();

        for (int i = 0; i < NowRoundActionSeq.Count; i++)
        {
            ActorTurnNode newNode = new ActorTurnNode();
            newNode.battleActor = NowRoundActionSeq[i].battleActor;
            NextRoundActionSeq.Add(newNode);
        }
        nowTurnActor = null;
        Debug.Log("round " + NowRound);
        FinishRoleAct();
    }


    public void PlayerFinishTurn()
    {
        if (Locked)
        {
            return;
        }
        if (!isPlayerTurn)
        {
            return;
        }
        FinishRoleAct();
    }

    //结束回合
    public void FinishRoleAct()
    {
        switchMouseState(eMouseState.Normal);

        if(nowTurnActor != null)
        {
            nowTurnActor.OnTurnFinish();
        }
        

        NextRoleAct();

        //if (nowTurnActor.InstId == 0)
        //{
        //    isPlayerTurn = true;
        //}
        //else
        //{
        //    isPlayerTurn = false;
        //}

    }

    private void NextRoleAct()
    {
        if (NowRoundActionSeq.Count == 0)
        {
            NextRound();
            return;
        }

        ActorTurnNode frontNode = NowRoundActionSeq[0];
        NowRoundActionSeq.RemoveAt(0);
        nowTurnActor = frontNode.battleActor;

        Debug.Log(nowTurnActor.InstId);

        if (nowTurnActor != null)
        {
            TurnMark.transform.SetParent(nowTurnActor.transform, true);
            TurnMark.transform.localPosition = Vector3.zero;
        }
        isPlayerTurn = true;

        pawn = nowTurnActor;

        nowTurnActor.OnTurnBegin();
        
    }


    IEnumerator AIAct()
    {
        yield return new WaitForSeconds(0.2f);
        Debug.Log("ai round finish");
        FinishRoleAct();
    }
    #endregion


    #region acion

    //public List<TimelineExecutor> pendingActions = new List<TimelineExecutor>();
    public ActionExecutor actionExecutor = new ActionExecutor();


    private void TickAction()
    {
        actionExecutor.Tick();

        
    }



    #endregion



    private List<Vector2Int> pathPreview;

    
    private void CalcPath(Vector2Int pos)
    {
        if (!isPlayerTurn)
        {
            return;
        }
        if (Locked)
        {
            return;
        }
        if(pathPreview != null)
        {
            if(pos == pathPreview[pathPreview.Count - 1])
            {
                ConfirmMove();
                return;
            }
        }

        pathPreview = grid1.FindPath(pawn.nowGridPos, pos);

        if (pathPreview == null)
        {
            return;
        }

        genPreviewPath(pathPreview, nowTurnActor.MovePoint);
        float length = getPathLengh(pathPreview);
        Debug.Log("总长度： " + length);



        //grid1.ShowPath(pathPreview);
        grid1.ShowPath(previewPots, outRangeIdx);
    }

    public float getPathLengh(List<Vector2Int> path)
    {
        float ret = 0;
        for(int i = 1; i < path.Count; i++)
        {
            float len = (path[i] - path[i-1]).magnitude;
            ret += len;
        }
        return ret;
    }


    private int outRangeIdx = 500;
    private List<Vector3> previewPots;
    public void genPreviewPath(List<Vector2Int> path, float maxMove)
    {
        float totalLen = 0;
        previewPots = new List<Vector3>();
        outRangeIdx = 500;
        bool outRange = false;
        float step = 0.25f;
        for (int i = 1; i < path.Count; i++)
        {
            Vector3 end = grid1.grid[path[i].x, path[i].y]._worldPos;
            Vector3 begin = grid1.grid[path[i-1].x, path[i-1].y]._worldPos;

            //float segLen = (path[i] - path[i - 1]).magnitude; 
            float segLen = Mathf.Sqrt((end.x - begin.x)* (end.x - begin.x) + (end.z - begin.z)* (end.z - begin.z));

            //Vector2 dir = (path[i] - path[i - 1]);
            Vector3 dir = (end - begin);
            //Debug.Log(dir);
            dir = dir.normalized;
            float rate = 1.0f / Mathf.Sqrt((dir.x * dir.x) + (dir.z * dir.z));
            if(rate > 10000)
            {
                Debug.Log("slope too much !");
            }
            Vector3 pot = begin;
            float nowlen = 0;
            while (nowlen < segLen)
            {

                if(nowlen + step > segLen)
                {
                    nowlen = segLen;
                    pot = end;
                }
                else
                {
                    nowlen += step;
                    pot += dir * step * rate;
                }
                previewPots.Add(pot);

                if (!outRange && nowlen + totalLen > maxMove)
                {
                    outRangeIdx = previewPots.Count - 1;
                    outRange = true;
                }

            }
        }
    }



    public void ConfirmMove()
    {
        //StartMove(pawn, pathPreview);
        StartMove(pawn, previewPots);
    }

    #region move

    //private List<Vector2Int> _path;
    private List<Vector3> _movingPath;

    private BaseUnit _unit;
    private int _pathIdx = 0;
    private bool isMoving = false;

    public bool CanMove()
    {
        if (actionExecutor.isActioning())
        {
            return false;
        }
        return true;
    }

    //public void StartMove(BaseUnit target, List<Vector2Int> path)
    //{
    //    _path = path;
    //    _unit = target;
    //    isMoving = true;
    //    _pathIdx = 0;
    //}
    public void StartMove(BaseUnit target, List<Vector3> path)
    {
        Lock();
        _movingPath = path;
        _unit = target;
        isMoving = true;
        _pathIdx = 0;
    }

    public void TickMove()
    {
        if (!CanMove())
        {
            return;
        }
        if (!isMoving)
        {
            return;
        }
        if(_movingPath == null || _movingPath.Count == 0)
        {
            return;
        }

        if (_pathIdx >= _movingPath.Count)
        {
            Unlock();
            isMoving = false;
            pawn.nowGridPos = grid1.GetFromPosition(nowTurnActor.transform.position);
            //pawn.nowGridPos = pathPreview[_pathIdx - 1];
            pathPreview = null;
            return;
        }

        if(nowTurnActor.MovePoint <= 0)
        {
            Unlock();
            isMoving = false;
            pawn.nowGridPos = grid1.GetFromPosition(nowTurnActor.transform.position);
            //pawn.nowGridPos = pathPreview[_pathIdx-1];
            //Debug.Log("fiish idx"+ _pathIdx);
            pathPreview = null;
            return;
        }

        //Vector3 targetGridWorldPos = grid1.GetWorldPos(_path[_pathIdx].x, _path[_pathIdx].y);
        Vector3 targetGridWorldPos = _movingPath[_pathIdx];

        Vector3 diff = (targetGridWorldPos - _unit.transform.position);
        Vector3 moveDist = diff.normalized * 3f * Time.deltaTime;


        if (moveDist.magnitude >= diff.magnitude)
        {
            _unit.transform.position = targetGridWorldPos;
            nowTurnActor.MovePoint -= diff.magnitude;
            grid1.HidePathSpot(_pathIdx);
            ++_pathIdx;
            //check new point
        }
        else
        {
            _unit.transform.position += moveDist;
            nowTurnActor.MovePoint -= moveDist.magnitude;
        }
    }

    #endregion



    #region 常用函数


    public static void DoDamage(Int64 source, Int64 target, Int64 damage, Int64 mask = 0)
    {


        for(int i=1; i< Instance.BattleUnits.Count; i++)
        {
            BaseUnit unit = Instance.BattleUnits[i];
            unit.DoDamage(damage);
        }
    }

    public static void DoDamage(Int64 source, List<Int64> target, Int64 damage, Int64 mask = 0)
    {
        for (int i = 1; i < Instance.BattleUnits.Count; i++)
        {
            BaseUnit unit = Instance.BattleUnits[i];
            unit.DoDamage(damage);
        }
    }

    public static void AddModifier(string modifierName, Dictionary<string,string> param)
    {

    }
    
    public static void AddAbility(string abilityName)
    {

    }


    
    public void DoDie(BaseUnit unit)
    {
        //事件

        unit.OnDie();
        //遍历modifier
        //
        
    }

    #endregion


}

