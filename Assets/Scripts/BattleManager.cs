﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattleManager : MonoBehaviour
{
    //public delegate void EffectCallback(EffectNode ctx);

    public Grid grid1;
    private Camera mCamera;


    public BaseUnit pawn;
    //private Vector2Int nowPawnPos;

    private int NowRound = 0;

    public static BattleManager Instance;
    public GameObject BaseUnitPrefab;


    public List<BaseUnit> BattleUnits = new List<BaseUnit>();
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
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            NextRoleAct();
        }
        HandleMouseClick();
        if (Input.GetKeyDown(KeyCode.T))
        {
            ActionExecutor exec = new ActionExecutor();
            exec.AddEffect(eEffectType.Damage, "20");
            exec.AddEffect(eEffectType.Delay, "1");
            pendingActions.Add(exec);
        }

        TickAction();
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

    public class ActionNode : System.IComparable<ActionNode>
    {
        public BaseUnit battleActor;
        public int prevIdx;
        public int nowIdx;

        public int CompareTo(ActionNode obj)  //实现该比较方法即可
        {
            return battleActor.stats.speed.CompareTo(obj.battleActor.stats.speed);
        }
    }

    List<ActionNode> NowRoundActionSeq;
    List<ActionNode> NextRoundActionSeq;
    BaseUnit nowTurnActor;
    bool isPlayerTurn;

    void InitBattle()
    {

        for (int i = 0; i < 5; i++)
        {
            GameObject go = Instantiate(BaseUnitPrefab);
            BaseUnit unit = go.GetComponent<BaseUnit>();
            unit.unitEntIdx = i;
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
            unit.AdjustPos();
            BattleUnits.Add(unit);
        }

        pawn = BattleUnits[0];

        NextRoundActionSeq = new List<ActionNode>();

        for (int i = 0; i < BattleUnits.Count; i++)
        {
            ActionNode newNode = new ActionNode();
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
        NextRoundActionSeq = new List<ActionNode>();

        for (int i = 0; i < NowRoundActionSeq.Count; i++)
        {
            ActionNode newNode = new ActionNode();
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

        ActionNode frontNode = NowRoundActionSeq[0];
        NowRoundActionSeq.RemoveAt(0);
        nowTurnActor = frontNode.battleActor;

        Debug.Log(nowTurnActor.unitEntIdx);


        if (nowTurnActor.unitEntIdx == 0)
        {
            isPlayerTurn = true;
        }
        else
        {
            isPlayerTurn = false;
        }

        if (!isPlayerTurn)
        {
            Debug.Log("ai round start");
            //抛出ai事件
            StartCoroutine(AIAct());
        }
    }


    IEnumerator AIAct()
    {
        yield return new WaitForSeconds(2);
        Debug.Log("ai round finish");
        NextRoleAct();
    }
    #endregion


    #region acion

    List<ActionExecutor> pendingActions = new List<ActionExecutor>();
    private void TickAction()
    {
        if (pendingActions.Count == 0)
        {
            return;
        }
        ActionExecutor exec = pendingActions[0];
        if (exec.isUpdating)
        {
            exec.Tick();
        }
        else
        {
            exec.HandleEffect();
        }
        if(exec.isUpdating)
        {
            return;
        }
        else if (exec.isDone)
        {
            Debug.Log("action finish");
            pendingActions.RemoveAt(0);
        }
        else
        {
            Debug.Log("action error");
            pendingActions.RemoveAt(0);
        }
    }

    public void AddActionExecutor(ActionExecutor newAction)
    {
        pendingActions.Insert(0, newAction);
    }

    #endregion


    

    Ray ray;
    RaycastHit hit;
    private void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
             }

            //if (!isPlayerTurn)
            //{
            //    return;
            //}
            if (Locked)
            {
                return;
            }

            ray = mCamera.ScreenPointToRay(Input.mousePosition);
            //Ray ray = new Ray(mCamera.transform.position,Vector3.forward);

            if (Physics.Raycast(ray, out hit))
            {
                Vector2Int pos = grid1.GetFromPosition(hit.point);

                List<Vector2Int> path = grid1.FindPath(pawn.nowGridPos, pos);

                if (path == null)
                {
                    return;
                }

                for (int i = 0; i < path.Count; i++)
                {
                    Debug.Log(path[i].x + " " + path[i].y);
                }

                pawn.nowGridPos = pos;

                grid1.ShowPath(path);

                StartGoPath(pawn, path);
                //StartCoroutine(GoPath(path));
                //pawn.transform.position = grid[pos.x, pos.y]._worldPos;

                //mark.transform.position = hit.point;
            }
        }
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
        ActionExecutor exec = new ActionExecutor();
        exec.AddEffectMove(unit, path);
        exec.AddEffect(eEffectType.Animation, "animanim_002");
        pendingActions.Add(exec);
    }




    #region 常用函数


    public static void DoDamage(int source, int target)
    {

    }

    public static void AddModifier(string modifierName, Dictionary<string,string> param)
    {

    }
    
    public static void AddAbility(string abilityName)
    {

    }

    public List<BaseUnit> DyingQueue = new List<BaseUnit>();

    public void DoDie(BaseUnit unit)
    {
        DyingQueue.Add(unit);
    }

    #endregion






}

