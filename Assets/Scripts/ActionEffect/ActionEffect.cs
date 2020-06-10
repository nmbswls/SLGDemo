using System.Collections;
using System.Collections.Generic;
using UnityEditor.MemoryProfiler;
using UnityEngine;

public class ActionEffect
{
    
}

public enum eEffectType
{
    Move,
    Attack,
    Delay,
    Sleep,
    Damage,
    AddBuff,
    Animation,
}

public class ActionExecutor
{

    public bool isDone;
    public bool isUpdating;
    public bool isError;

    public List<EffectNodeBase> effectList = new List<EffectNodeBase>();
    //eEffectType eid, string paramstring
    public void AddEffect(eEffectType eid, string paramstring)
    {
        EffectNodeBase newNode = createNode(eid, paramstring);
        if (newNode != null)
        {
            effectList.Add(newNode);
        }
    }
    public void InsertEffect<T>(int idx, eEffectType eid, string paramstring)
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
        switch (eid)
        {
            case eEffectType.Delay:
                newNode = new EffectNode_Delay(this, eid, paramstring);
                break;
            case eEffectType.AddBuff:
                newNode = new EffectNode_AddBuff(this, eid, paramstring);
                break;
            case eEffectType.Animation:
                newNode = new EffectNode_Animation(this, eid, paramstring);
                break;
            case eEffectType.Sleep:
                newNode = new EffectNode_Sleep(this, eid, paramstring);
                break;
            case eEffectType.Damage:
                newNode = new EffectNode_Damage(this, eid, paramstring);
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
    public string ename = "";
    string paramstring = "";
    //public EffectCallback callback;

    public EffectNodeBase(ActionExecutor owner, eEffectType eid, string paramstring)
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
    public float delaySeconds = 0f;
    
    float timer;
    bool triggered;

    public EffectNode_Delay(ActionExecutor owner, eEffectType eid, string paramstring): base(owner, eid, paramstring)
    {

        string[] parray = paramstring.Split(';');
        delaySeconds = float.Parse(parray[0]);
    }

    public override ExecRet Tick()
    {
        timer += Time.deltaTime;
        Debug.Log("delay");
        if (triggered)
        {
            if (timer > delaySeconds)
            {
                owner.AddEffect(eEffectType.Damage, "1000");
                owner.AddEffect(eEffectType.AddBuff, "10111");
                owner.AddEffect(eEffectType.Animation, "act_01");
                return ExecRet.SUCCESS;
            }
            return ExecRet.UPDATING;
        }
        if (timer > delaySeconds * 0.5f)
        {
            ActionExecutor exec = new ActionExecutor();
            exec.AddEffect(eEffectType.Sleep,"2");
            BattleManager.Instance.AddActionExecutor(exec);
            triggered = true;
        }
        return ExecRet.UPDATING;
    }

    public override ExecRet Exec()
    {
        return ExecRet.UPDATING;
    }


}

public class EffectNode_Damage : EffectNodeBase
{
    public float Damage;

    public EffectNode_Damage(ActionExecutor owner, eEffectType eid, string paramstring) : base(owner, eid, paramstring)
    {

        string[] parray = paramstring.Split(';');
        Damage = float.Parse(parray[0]);
    }

    public override ExecRet Tick()
    {
        return base.Tick();
    }

    public override ExecRet Exec()
    {
        Debug.Log("damage: " + Damage);
        return ExecRet.SUCCESS;
    }
}

public class EffectNode_AddBuff : EffectNodeBase
{
    public int BuffId;

    public EffectNode_AddBuff(ActionExecutor owner, eEffectType eid, string paramstring) : base(owner, eid, paramstring)
    {

        string[] parray = paramstring.Split(';');
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

    public EffectNode_Animation(ActionExecutor owner, eEffectType eid, string paramstring) : base(owner, eid, paramstring)
    {

        string[] parray = paramstring.Split(';');
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

    public EffectNode_Sleep(ActionExecutor owner, eEffectType eid, string paramstring) : base(owner, eid, paramstring)
    {

        string[] parray = paramstring.Split(';');
        SleepTime = 5f;
    }

    public override ExecRet Tick()
    {
        _timer += Time.deltaTime;
        Debug.Log("sleeping");
        
        if (_timer > SleepTime)
        {
            return ExecRet.SUCCESS;
        }
        return ExecRet.UPDATING;
    }

    public override ExecRet Exec()
    {
        return ExecRet.UPDATING;
    }
}