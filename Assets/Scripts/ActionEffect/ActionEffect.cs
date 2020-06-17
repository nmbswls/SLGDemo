using System;
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
    CallFunc = 9,
    LaunchProj = 10,
    Max = 11,
}


public class ActionNodeFactroy
{



    public static ActionNode CreateFromString(eEffectType eid, string paramstring)
    {
        ActionNode newNode = null;
        string[] paramList = paramstring.Split(',');
        switch (eid)
        {
            case eEffectType.Delay:
                float time = float.Parse(paramstring);
                newNode = new ActionNode_Delay(time);
                break;
            case eEffectType.AddBuff:
                newNode = new EffectNode_AddBuff(paramstring);
                break;
            case eEffectType.Animation:
                break;
            case eEffectType.Sleep:
                newNode = new EffectNode_Sleep(paramstring);
                break;
            case eEffectType.Damage:
                if (paramList.Length != 3)
                {
                    break;
                }
                Int64 source = Int64.Parse(paramList[0]);
                Int64 target = Int64.Parse(paramList[1]);
                Int64 Damage = Int64.Parse(paramList[2]);
                newNode = new ActionNode_Damage(source, target, Damage);
                break;
            default:
                break;
        }
        return newNode;
    }
}

public class ActionExecutor
{

    private float _startTime;

    //public class TimelineNode
    //{
    //    public float timelineTime;
    //    public List<ActionNode> effects;
    //}

    //public List<TimelineNode> TimelineNodes = new List<TimelineNode>();

    public bool isActioning()
    {
        return NodeList.Count > 0;
    }

    public List<ActionNode> NodeList = new List<ActionNode>();

    public void Tick()
    {
        if(NodeList.Count == 0)
        {
            return;
        }

        //if(_startTime + NodeList.Peek().timeline < Time.time)
        //{
        //    return;
        //}
        ActionNode first = NodeList[0];
        if(first.state == eActionNodeState.Updating)
        {
            first.Tick();
            return;
        }
        
        while (NodeList.Count > 0)
        {
            first = NodeList[0];
            if (first.state == eActionNodeState.Default)
            {
                first.Exec();
            }
            if(first.state == eActionNodeState.Updating)
            {
                return;
            }
            NodeList.Remove(first);
        }
    }




    public void AddActionNode(ActionNode newNode)
    {
        if (newNode != null)
        {
            NodeList.Add(newNode);
            newNode.owner = this;
        }
    }

    public void AddImmediateActionNode(int idx, ActionNode newNode)
    {
        if(newNode == null)
        {
            return;
        }
        newNode.owner = this;
        NodeList.Insert(idx, newNode);
    }

}



public enum ExecRet
{
    UPDATING,
    SUCCESS,
    FAIL,
}

public enum eActionNodeState
{
    Default,
    Updating,
    Finished,
    Error,
}

public class ActionNode
{
    public ActionExecutor owner;
    public eActionNodeState state = eActionNodeState.Default; //0 1 2
    public float timeline;

    public eEffectType eid;
    //public EffectCallback callback;

    public ActionNode(eEffectType eid)
    {
        this.eid = eid;
    }
    public virtual void Tick()
    {
        state = eActionNodeState.Finished;
    }
    public virtual void Exec()
    {
        state = eActionNodeState.Finished;
    }

    public void SetDone()
    {
        state = eActionNodeState.Finished;
    }
}






public class ActionNode_Delay : ActionNode
{
    public float delaySec = 0f;
    
    float timer;
    bool triggered;

    public ActionNode_Delay(float delaySec): base(eEffectType.Delay)
    {
        this.delaySec = delaySec;
    }

    public override void Tick()
    {
        timer += Time.deltaTime;
        //Debug.Log("delay");
        if (triggered)
        {
            if (timer > delaySec)
            {
                {
                    ActionNode node = ActionNodeFactroy.CreateFromString(eEffectType.Damage, "1000");
                    owner.AddActionNode(node);
                }
                state = eActionNodeState.Finished;
            }
            return;
        }
        if (timer > delaySec * 0.5f)
        {
            //ActionExecutor exec = new ActionExecutor();
            //exec.AddEffect(eEffectType.Sleep,"2");

            //BattleManager.Instance.AddEffectImmediate(eEffectType.Sleep, "2");
            triggered = true;
        }
    }

    public override void Exec()
    {
        state = eActionNodeState.Updating;
    }


}







public class EffectNode_Move : ActionNode
{
    public List<Vector2Int> path;
    public BaseUnit unit;

    int _pathIdx = 0;
    Grid _grid;

    public EffectNode_Move(BaseUnit unit, List<Vector2Int> path) : base(eEffectType.Move)
    {
        this.unit = unit;
        this.path = path;
    }

    public override void Tick()
    {
        if(_pathIdx >= path.Count)
        {
            BattleManager.Instance.Unlock();
            state = eActionNodeState.Finished;
            return;
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

    }

    public override void Exec()
    {

        if (unit == null || path == null || path.Count == 0)
        {
            state = eActionNodeState.Error;
            return;
        }

        //if (unit.CanMove())
        //{

        //}

        BattleManager.Instance.Lock();
        _pathIdx = 0;
        _grid = BattleManager.Instance.grid1;
        state = eActionNodeState.Updating;
    }
}




public class ActionNode_Damage : ActionNode
{

    public Int64 Damage;
    public Int64 Source;
    public Int64 Target;

    public ActionNode_Damage(Int64 source, Int64 target, Int64 damage) : base(eEffectType.Damage)
    {

        Damage = damage;
        Source = source;
        Target = target;
    }

    public override void Tick()
    {
        base.Tick();
    }

    public override void Exec()
    {
        Debug.Log("damage: " + Damage);

        BattleManager.DoDamage(-1, -1, Damage);
        state = eActionNodeState.Finished;
    }
}

public class EffectNode_AddBuff : ActionNode
{
    public int BuffId;

    public EffectNode_AddBuff(string paramstring) : base(eEffectType.AddBuff)
    {

        string[] parray = paramstring.Split(',');
        BuffId = int.Parse(parray[0]);
    }

    public override void Tick()
    {
        base.Tick();
    }

    public override void Exec()
    {
        Debug.Log("add buff: " + BuffId);
        state = eActionNodeState.Finished;
    }
}

public class ActionNode_Animation : ActionNode
{
    public string AnimClipName;
    public BaseUnit animTarget;

    public ActionNode_Animation(BaseUnit animTarget, string animName) : base(eEffectType.Animation)
    {

        AnimClipName = animName;
        this.animTarget = animTarget;
    }

    public override void Tick()
    {
        base.Tick();
    }

    public override void Exec()
    {
        Debug.Log("name: " + AnimClipName);
        state = eActionNodeState.Finished;
    }
}

public class EffectNode_Sleep : ActionNode
{
    public float SleepTime;

    float _timer;

    public EffectNode_Sleep(string paramstring) : base(eEffectType.Sleep)
    {

        string[] parray = paramstring.Split(',');
        SleepTime = 5f;
    }

    public override void Tick()
    {
        _timer += Time.deltaTime;
        //Debug.Log("sleeping");
        
        if (_timer > SleepTime)
        {
            Debug.Log("sleep end");
            state = eActionNodeState.Finished;
        }
    }

    public override void Exec()
    {
        state = eActionNodeState.Updating;
    }
}

public class EffectNode_RunScript : ActionNode
{
    public string ScriptName;

    private LuaTable scriptEnv;
    private Action luaExec;
    public EffectNode_RunScript(string paramstring) : base(eEffectType.RunScript)
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

    public override void Tick()
    {
        base.Tick();
    }

    public override void Exec()
    {
        if(luaExec != null)
        {
            luaExec();
        }
        state = eActionNodeState.Finished;
    }
}



public class ActionNode_CallFunc : ActionNode
{
    public Action Func;

    public ActionNode_CallFunc(Action Func) : base(eEffectType.CallFunc)
    {
        this.Func = Func;
    }

    public override void Tick()
    {
        base.Tick();
    }

    public override void Exec()
    {
        if(Func != null)
            Func();
        state = eActionNodeState.Finished;
    }


}

public class ActionNode_Timeline : ActionNode
{

    public ActionNode_Timeline() : base(eEffectType.Delay)
    {
    }

    public override void Tick()
    {
        base.Tick();
    }

    public override void Exec()
    {
        
        state = eActionNodeState.Finished;
    }


}

public class ActionNode_LaunchProj : ActionNode
{
    private SlgProjectile proj;

    public ActionNode_LaunchProj(BaseUnit target, BaseUnit source) : base(eEffectType.LaunchProj)
    {
        proj = ProjectileManager.Instance.createProjectile(target, source, 800);
    }

    public override void Tick()
    {
        if (proj.reached)
        {
            state = eActionNodeState.Finished;
        }
    }

    public override void Exec()
    {
        Debug.Log("launch");
        if (proj == null)
        {
            state = eActionNodeState.Finished;
            Debug.Log("launch fail");

            return;
        }
        ProjectileManager.Instance.AddProj(proj);
        state = eActionNodeState.Updating;
    }
}