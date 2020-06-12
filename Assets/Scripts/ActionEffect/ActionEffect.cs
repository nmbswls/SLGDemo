﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using XLua;

public class ActionEffect
{
    
}

public enum eEffectType
{
    Nonde = 0,
    Move = 1,
    Attack = 2,
    Delay = 3,
    Sleep = 4,
    Damage = 5,
    AddBuff = 6,
    Animation = 7,
    RunScript = 8,
    Max = 9,
}

public class ActionExecutor
{

    public bool isDone;
    public bool isUpdating;
    public bool isError;

    public List<EffectNodeBase> effectList = new List<EffectNodeBase>();


    public void AddEffectNode(EffectNodeBase newNode)
    {
        if (newNode != null)
        {
            effectList.Add(newNode);
        }
    }

    //eEffectType eid, string paramstring
    public void AddEffect(eEffectType eid, string paramstring)
    {
        EffectNodeBase newNode = createNode(eid, paramstring);
        if (newNode != null)
        {
            effectList.Add(newNode);
        }
    }

    public void AddEffectMove(BaseUnit unit, List<Vector2Int> path)
    {
        EffectNodeBase newNode = new EffectNode_Move(this, eEffectType.Move, unit, path);
        if (newNode != null)
        {
            effectList.Add(newNode);
        }
    }

    public void InsertEffect(int idx, eEffectType eid, string paramstring)
    {
        EffectNodeBase newNode = createNode(eid, paramstring);
        if (newNode != null)
        {
            effectList.Insert(idx, newNode);
        }
    }

    private EffectNodeBase createNode(eEffectType eid, string paramstring)
    {
        EffectNodeBase newNode = null;
        string[] paramList = paramstring.Split(',');
        switch (eid)
        {
            case eEffectType.Delay:
                float time = float.Parse(paramstring);
                newNode = new EffectNode_Delay(this, time);
                break;
            case eEffectType.AddBuff:
                newNode = new EffectNode_AddBuff(this,paramstring);
                break;
            case eEffectType.Animation:
                newNode = new EffectNode_Animation(this, paramstring);
                break;
            case eEffectType.Sleep:
                newNode = new EffectNode_Sleep(this,paramstring);
                break;
            case eEffectType.Damage:
                if(paramList.Length != 3)
                {
                    Debug.Log("not 3");
                    break;
                }
                Int64 source = Int64.Parse(paramList[0]);
                Int64 target = Int64.Parse(paramList[1]);
                Int64 Damage = Int64.Parse(paramList[2]);
                newNode = new EffectNode_Damage(this, source, target, Damage);
                break;
            default:
                break;
        }
        return newNode;
    }

   
    

    public void Tick()
    {
        if (isDone || isError)
        {
            return;
        }

        if (!isUpdating)
        {
            return;
        }
        EffectNodeBase first = effectList[0];
        ExecRet ret = first.Tick();
        if (ret != ExecRet.UPDATING)
        {
            isUpdating = false;
            effectList.RemoveAt(0);
            HandleEffect();
        }
    }

    public void HandleEffect()
    {
        if (isDone || isError)
        {
            return;
        }

        EffectNodeBase first;
        while (effectList.Count > 0)
        {
            first = effectList[0];
            ExecRet ret = first.Exec();
            if (ret == ExecRet.UPDATING)
            {
                isUpdating = true;
                return;
            }
            effectList.RemoveAt(0);
            if (ret == ExecRet.FAIL)
            {
                Debug.Log("error ");
                isError = true;
                return;
            }
        }
        isDone = true;
    }
}



public enum ExecRet
{
    UPDATING,
    SUCCESS,
    FAIL,
}

public class EffectNodeBase
{
    public ActionExecutor owner;
    public bool isDone;

    public eEffectType eid;
    //public EffectCallback callback;

    public EffectNodeBase(ActionExecutor owner, eEffectType eid)
    {
        this.owner = owner;
        this.eid = eid;
    }
    public virtual ExecRet Tick()
    {
        return ExecRet.UPDATING;
    }
    public virtual ExecRet Exec()
    {
        return ExecRet.SUCCESS;
    }

    public void SetDone()
    {
        isDone = true;
    }
}

public class EffectNode_Delay : EffectNodeBase
{
    public float delaySec = 0f;
    
    float timer;
    bool triggered;

    public EffectNode_Delay(ActionExecutor owner, float delaySec): base(owner, eEffectType.Delay)
    {
        this.delaySec = delaySec;
    }

    public override ExecRet Tick()
    {
        timer += Time.deltaTime;
        //Debug.Log("delay");
        if (triggered)
        {
            if (timer > delaySec)
            {
                owner.AddEffect(eEffectType.Damage, "1000");
                owner.AddEffect(eEffectType.AddBuff, "10111");
                owner.AddEffect(eEffectType.Animation, "act_01");
                return ExecRet.SUCCESS;
            }
            return ExecRet.UPDATING;
        }
        if (timer > delaySec * 0.5f)
        {
            //ActionExecutor exec = new ActionExecutor();
            //exec.AddEffect(eEffectType.Sleep,"2");

            //BattleManager.Instance.AddEffectImmediate(eEffectType.Sleep, "2");
            triggered = true;
        }
        return ExecRet.UPDATING;
    }

    public override ExecRet Exec()
    {
        return ExecRet.UPDATING;
    }


}


public class EffectNode_Move : EffectNodeBase
{
    public List<Vector2Int> path;
    public BaseUnit unit;

    int _pathIdx = 0;
    Grid _grid;

    public EffectNode_Move(ActionExecutor owner, eEffectType eid, BaseUnit unit, List<Vector2Int> path) : base(owner, eid)
    {
        this.unit = unit;
        this.path = path;
    }

    public override ExecRet Tick()
    {
        if(_pathIdx >= path.Count)
        {
            BattleManager.Instance.Unlock();
            return ExecRet.SUCCESS;
        }


        Vector3 targetGridWorldPos = _grid.GetWorldPos(path[_pathIdx].x, path[_pathIdx].y);

        Vector3 diff = (targetGridWorldPos - unit.transform.position);
        Vector3 moveDist = diff.normalized * 3f * Time.deltaTime;


        if (moveDist.magnitude >= diff.magnitude)
        {
            unit.transform.position = targetGridWorldPos;
            ++_pathIdx;
        }
        else
        {
            unit.transform.position += moveDist;
        }
        return ExecRet.UPDATING;

    }

    public override ExecRet Exec()
    {

        if (unit == null || path == null || path.Count == 0)
        {
            return ExecRet.FAIL;
        }

        //if (unit.CanMove())
        //{

        //}

        BattleManager.Instance.Lock();
        _pathIdx = 0;
        _grid = BattleManager.Instance.grid1;

        return ExecRet.UPDATING;
    }


}




public class EffectNode_Damage : EffectNodeBase
{

    public Int64 Damage;
    public Int64 Source;
    public Int64 Target;

    public EffectNode_Damage(ActionExecutor owner, Int64 source, Int64 target, Int64 damage) : base(owner, eEffectType.Damage)
    {

        Damage = damage;
        Source = source;
        Target = target;
    }

    public override ExecRet Tick()
    {
        return base.Tick();
    }

    public override ExecRet Exec()
    {
        Debug.Log("damage: " + Damage);

        BattleManager.DoDamage(-1, -1, Damage);

        return ExecRet.SUCCESS;
    }
}

public class EffectNode_AddBuff : EffectNodeBase
{
    public int BuffId;

    public EffectNode_AddBuff(ActionExecutor owner, string paramstring) : base(owner, eEffectType.AddBuff)
    {

        string[] parray = paramstring.Split(',');
        BuffId = int.Parse(parray[0]);
    }

    public override ExecRet Tick()
    {
        return base.Tick();
    }

    public override ExecRet Exec()
    {
        Debug.Log("add buff: " + BuffId);
        return ExecRet.SUCCESS;
    }
}

public class EffectNode_Animation : EffectNodeBase
{
    public string AnimClipName;

    public EffectNode_Animation(ActionExecutor owner, string paramstring) : base(owner, eEffectType.Animation)
    {

        string[] parray = paramstring.Split(',');
        AnimClipName = parray[0];
    }

    public override ExecRet Tick()
    {
        return base.Tick();
    }

    public override ExecRet Exec()
    {
        Debug.Log("name: " + AnimClipName);
        return ExecRet.SUCCESS;
    }
}

public class EffectNode_Sleep : EffectNodeBase
{
    public float SleepTime;

    float _timer;

    public EffectNode_Sleep(ActionExecutor owner, string paramstring) : base(owner, eEffectType.Sleep)
    {

        string[] parray = paramstring.Split(',');
        SleepTime = 5f;
    }

    public override ExecRet Tick()
    {
        _timer += Time.deltaTime;
        //Debug.Log("sleeping");
        
        if (_timer > SleepTime)
        {
            Debug.Log("sleep end");
            return ExecRet.SUCCESS;
        }
        return ExecRet.UPDATING;
    }

    public override ExecRet Exec()
    {
        return ExecRet.UPDATING;
    }
}

public class EffectNode_RunScript : EffectNodeBase
{
    public string ScriptName;

    private LuaTable scriptEnv;
    private Action luaExec;
    public EffectNode_RunScript(ActionExecutor owner, string paramstring) : base(owner,eEffectType.RunScript)
    {

        ScriptName = paramstring;

        scriptEnv = LuaMain.luaEnv.NewTable();

        LuaTable meta = LuaMain.luaEnv.NewTable();
        meta.Set("__index", LuaMain.luaEnv.Global);
        scriptEnv.SetMetaTable(meta);
        meta.Dispose();

        LuaFunction func = LuaMain.luaEnv.LoadString(ScriptName, "script_" + ScriptName, scriptEnv);
        //LuaDataMgr.setfenv(func, scriptEnv);
        func.Call();

        scriptEnv.Get("Exec", out luaExec);
    }

    public override ExecRet Tick()
    {
        return ExecRet.UPDATING;
    }

    public override ExecRet Exec()
    {
        if(luaExec != null)
        {
            luaExec();
        }
        return ExecRet.SUCCESS;
    }
}