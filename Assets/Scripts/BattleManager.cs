using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
        gridListener.ClickEvent += HandleMouseClick;
    }

    private void OnDestroy()
    {
        //unregister
        gridListener.ClickEvent -= HandleMouseClick;
    }


    void Update()
    {
        
        //HandleMouseClick();

        //TickAbility();
        TickAction();
        TickMove();
    }


    public enum eHudState
    {
        Normal,
        Attack,
    }

    public eHudState hudState = eHudState.Normal;
    public Ability nowAbility;
    public void StartChooseTarget()
    {
        nowAbility = nowTurnActor.AbilityList[0];
        if ((nowAbility.Config.targetType & 4) != 0)
        {
            hudState = eHudState.Attack;
            hudCtrl.SwitchAttackBtn(true);
            //ShowAtkRange;
        }
    }

    public void OnActorClick(BaseUnit target)
    {
        if (hudState == eHudState.Normal)
        {
            Debug.Log("show info");
        }
        else if (hudState == eHudState.Attack)
        {
            //获取target  进行筛选是否可行
            nowAbility.UseAbility();
            hudCtrl.SwitchAttackBtn(false);
        }
    }

    

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
    BaseUnit nowTurnActor;
    bool isPlayerTurn;

    void InitBattle()
    {

        for (int i = 0; i < 5; i++)
        {
            GameObject go = Instantiate(BaseUnitPrefab);
            BaseUnit unit = go.GetComponent<BaseUnit>();
            unit.InstId = i;
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
        NextRoleAct();
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
        NextRoleAct();
    }

    public void NextRoleAct()
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
            TurnMark.transform.SetParent(nowTurnActor.transform,true);
            TurnMark.transform.localPosition = Vector3.zero;
        }
        isPlayerTurn = true;

        pawn = nowTurnActor;


        //if (nowTurnActor.InstId == 0)
        //{
        //    isPlayerTurn = true;
        //}
        //else
        //{
        //    isPlayerTurn = false;
        //}

    }


    IEnumerator AIAct()
    {
        yield return new WaitForSeconds(0.2f);
        Debug.Log("ai round finish");
        NextRoleAct();
    }
    #endregion


    #region acion

    //public List<TimelineExecutor> pendingActions = new List<TimelineExecutor>();
    public ActionExecutor actionExecutor = new ActionExecutor();

    //public void AddEffect(global::ActionNode newNode)
    //{
    //    if (pendingActions.Count == 0)
    //    {
    //        pendingActions.Add(new ActionExecutor());
    //    }


    //    pendingActions[0].AddEffectNode(newNode);
    //}


    //public void AddEffect(eEffectType name, string paramstring)
    //{
    //    if (pendingActions.Count == 0)
    //    {
    //        pendingActions.Add(new ActionExecutor());
    //    }


    //    pendingActions[0].AddEffect(name, paramstring);
    //}

    //public void AddEffectImmediate(eEffectType name, string paramstring)
    //{
    //    if (pendingActions.Count == 0)
    //    {
    //        pendingActions.Add(new ActionExecutor());
    //    }
    //    pendingActions[0].InsertEffect(0, name, paramstring);
    //}

    private void TickAction()
    {
        actionExecutor.Tick();

        
    }



    #endregion



    private List<Vector2Int> path;

    private void HandleMouseClick(SceneClickData data)
    {
        if(hudState == eHudState.Attack)
        {
            return;
        }

        if (!isPlayerTurn)
        {
            return;
        }
        if (Locked)
        {
            return;
        }
        if(data.Go.Equals(grid1))
        {
            Debug.Log("click on actor " + data.Go.name + " " + grid1.name);
            return;
        }
        Vector2Int pos = grid1.GetFromPosition(data.PosInWorld);

        path = grid1.FindPath(pawn.nowGridPos, pos);

        if (path == null)
        {
            return;
        }

        
        grid1.ShowPath(path);
        //StartGoPath(pawn, path);
        //StartMove(pawn, path);
    }


    public void ConfirmMove()
    {
        StartMove(pawn, path);
        pawn.nowGridPos = path[path.Count-1];
    }


    IEnumerator GoPath(List<Vector2Int> path)
    {
        Lock();

        if (path.Count == 0)
        {
            yield break;
        }
        int pathIdx = 0;
        while (pathIdx < path.Count)
        {
            Vector3 targetGridWorldPos = grid1.GetWorldPos(path[pathIdx].x, path[pathIdx].y);
            while (true)
            {
                Vector3 diff = (targetGridWorldPos - pawn.transform.position);
                Vector3 moveDist = diff.normalized * 3f * Time.deltaTime;
                if (moveDist.magnitude >= diff.magnitude)
                {
                    pawn.transform.position = targetGridWorldPos;
                    ++pathIdx;
                    break;
                }
                pawn.transform.position += moveDist;
                yield return null;
            }
        }
        Unlock();
        yield break;
    }



    public void StartGoPath(BaseUnit unit, List<Vector2Int> path)
    {
        //ActionExecutor exec = new ActionExecutor();
        //exec.AddEffectMove(unit, path);
        //exec.AddEffect(eEffectType.Animation, "animanim_002");
        //pendingActions.Add(exec);
    }


    #region move

    private List<Vector2Int> _path;
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

    public void StartMove(BaseUnit target, List<Vector2Int> path)
    {
        _path = path;
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
        if(_path == null || _path.Count == 0)
        {
            return;
        }

        if (_pathIdx >= _path.Count)
        {
            Unlock();
            isMoving = false;
            return;
        }


        Vector3 targetGridWorldPos = grid1.GetWorldPos(_path[_pathIdx].x, _path[_pathIdx].y);

        Vector3 diff = (targetGridWorldPos - _unit.transform.position);
        Vector3 moveDist = diff.normalized * 3f * Time.deltaTime;


        if (moveDist.magnitude >= diff.magnitude)
        {
            _unit.transform.position = targetGridWorldPos;
            ++_pathIdx;
        }
        else
        {
            _unit.transform.position += moveDist;
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

