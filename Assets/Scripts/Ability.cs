using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;


public enum eBattleEventType
{
    USE_SKILL,
    TRIGGER
}

public class BattleEvent
{
    public eBattleEventType type;
    public bool isDone;
    public object data;
}

public class EventHandler
{
    List<BattleEvent> evtList = new List<BattleEvent>();

    public void Tick()
    {
        if(evtList.Count == 0)
        {
            return;
        }
        if (evtList[0].isDone)
        {
            evtList.RemoveAt(0);
        }
        //启动
        //将事件拼装成timeline
    }
}

public enum eAbilityTargetType
{
    Invalid = 0,
    NoTarget = 1,
    Point = 2,
    Target = 4,
    Enemy = 8,
    Friend = 16,
    Netrual = 32,
}
public class AbilityConfig
{
    public int id;
    public int cd;
    public int atk;
    public int RangeInt; //施法距离多少码
    public int ChannelTurn;
    public float PointTime; //动画前摇时间
    public float TotalTime; //动画总时间
    public List<string> effects = new List<string>();
    public string animString;
    public int targetType = 4;
}

public enum eAbilityPhase
{
    UnUsed,
    BeforeStart,
    Spelling,
}

public class Ability
{
    public Int64 InstId;

    public BaseUnit owner;
    public string AbilityName;
    public int CoolDownBattle;

    public float CoolDownExplore;
    public Dictionary<string, string> extraParams;

    public bool isLua;

    public AbilityConfig Config;

    public BaseUnit Caster;
    public BaseUnit Target;
    public Vector2 TargetPosition; //带有小数的格子点

    public int ChannelCnt = 0;
    public float Timer;

    private eAbilityPhase _phase;
    public eAbilityPhase NowPhase
    {
        get { return _phase; }
    }

    public float UseSkillTimer; //要计算暂停时间

    public void SwitchPhase(eAbilityPhase newPhase)
    {
        if(newPhase == _phase)
        {
            return;
        }

        _phase = newPhase;

        switch (newPhase)
        {
            case eAbilityPhase.BeforeStart:
                if(Config != null && Config.animString != null && Config.animString != "")
                {
                    ActionNode animNode = new ActionNode_Animation(Caster, Config.animString);
                    BattleManager.Instance.actionExecutor.AddActionNode(animNode);
                    UseSkillTimer = Time.time;
                }
                break;
            case eAbilityPhase.Spelling:
                OnSpellStart();
                break;
            case eAbilityPhase.UnUsed:
                //统计一哈？写攻击事件？同步下属性？
                break;
            default:
                break;
        }
    }

    
    

    public void ExecEffectes()
    {
        ActionExecutor exec = BattleManager.Instance.actionExecutor;

        for (int i = 0; i < Config.effects.Count; i++)
        {
            string param = Config.effects[i];

            int splitIdx = param.IndexOf(',');
            int typeInt;
            if (splitIdx == -1)
            {
                typeInt = int.Parse(param);
            }
            else
            {
                typeInt = int.Parse(param.Substring(0, splitIdx));
            }

            if( typeInt <= (int)eEffectType.Nonde || typeInt >= (int)eEffectType.Max)
            {
                continue;
            }
            ActionNode node;
            if (typeInt == (int)eEffectType.Animation)
            {
                node = new ActionNode_Animation(owner, param.Substring(splitIdx + 1));
            }
            else if (typeInt == (int)eEffectType.Damage)
            {
                node = new ActionNode_Damage(-1,-1, long.Parse(param.Substring(splitIdx + 1)));
            }
            else if (typeInt == (int)eEffectType.LaunchProj)
            {
                node = new ActionNode_LaunchProj(Target, Caster);
            }
            else
            {
                node = ActionNodeFactroy.CreateFromString((eEffectType)typeInt, param.Substring(splitIdx+1));
            }
            Debug.Log("new node type:" + node.eid);
            //exec.AddActionNode(node);
            exec.AddImmediateActionNode(i,node);
        }
    }

    public virtual void OnSpellStart()
    {
        Debug.Log("spell start");
        ExecEffectes();
    }
    public virtual void OnSpellEnd()
    {

    }


    public bool TryUseAbility()
    {
        //cost
        Caster = owner;
        Target = null;
        UseAbility();
        return true;
    }

    public bool TryUseAbility(BaseUnit target)
    {
        if (target == null)
        {
            return false;
        }
        float dist = (target.GetWorldPos2D() - owner.GetWorldPos2D()).magnitude;
        //cost
        if (dist * 100 > Config.RangeInt)
        {
            Debug.Log("chaochu juli");
            return false;
        }

        Caster = owner;
        Target = target;
        UseAbility();
        return true;
    }

    public bool TryUseAbility(Vector2 targetPos)
    {
        Caster = owner;
        Target = null;
        UseAbility();
        return true;
    }

    public void UseAbility()
    {

        SwitchPhase(eAbilityPhase.BeforeStart);
        ActionExecutor exec = BattleManager.Instance.actionExecutor;

        ActionNode anim = new ActionNode_Animation(owner,"act animation 01");
        ActionNode pre_delay = new ActionNode_Delay(0.5f);

        exec.AddActionNode(anim);
        exec.AddActionNode(pre_delay);

        ActionNode_CallFunc func = new ActionNode_CallFunc(OnSpellStart);
        exec.AddActionNode(func);
        ActionNode post_delay = new ActionNode_Delay(1.5f);
        exec.AddActionNode(post_delay);
        //OnSpellStart();
        Debug.Log("use skill");
    }

    public virtual void ParentTest()
    {
        Debug.Log("parent test");
    }
}

public class Ability_Lua : Ability
{
    public string luaScript;
    private LuaTable scriptEnv;

    private Action luaOnSpellStart;

    public Ability_Lua(string filename)
    {
        TextAsset ta = Resources.Load<TextAsset>("Lua/"+ filename + ".lua");
        this.luaScript = ta.text;
        this.isLua = true;
        InitLua();
    }

    private void InitLua()
    {

        if (luaScript == null)
        {
            return;
        }

        scriptEnv = LuaMain.luaEnv.NewTable();

        // 为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
        LuaTable meta = LuaMain.luaEnv.NewTable();
        meta.Set("__index", LuaMain.luaEnv.Global);
        scriptEnv.SetMetaTable(meta);
        meta.Dispose();

        LuaMain.luaEnv.DoString(luaScript, "script_" + this.AbilityName, scriptEnv);


        scriptEnv.Set("self", this);
        scriptEnv.Get("onSpellStart", out luaOnSpellStart);

    }

    public override void OnSpellStart()
    {
        if (luaOnSpellStart != null)
        {
            luaOnSpellStart();
        }
    }

    public override void ParentTest()
    {
        Debug.Log("childiren test");
    }

}


//public class AbilityInstance
//{
//    public Int64 InstId;
//    public Ability Template;
//    public int CoolDownBattle;
//    public float CoolDownExplore; //神界原罪 回合外cd

//    public void UseAbility()
//    {
//        ActionExecutor exec = new ActionExecutor();

//        for (int i = 0; i > Template.effects.Count; i++)
//        {
//            string param = Template.effects[i];
//        }
        
//        BattleManager.Instance.pendingActions.Add(exec);
//    }
//}