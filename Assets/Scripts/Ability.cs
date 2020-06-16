using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class AbilityConfig
{
    public int id;
    public int cd;
    public int atk;
    public int ChannelTurn;
    public float PointTime; //动画前摇时间
    public float TotalTime; //动画总时间
    public List<string> effects = new List<string>();
    public string animString;

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
                    EffectNodeBase animNode = new EffectNode_Animation(null, Caster, Config.animString);
                    BattleManager.Instance.AddEffect(animNode);
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
        ActionExecutor exec = new ActionExecutor();

        for (int i = 0; i < Config.effects.Count; i++)
        {
            string param = Config.effects[i];

            int splitIdx = param.IndexOf(',');
            if(splitIdx == -1)
            {
                continue;
            }
            int typeInt = int.Parse(param.Substring(0, splitIdx));
            if( typeInt <= (int)eEffectType.Nonde || typeInt >= (int)eEffectType.Max)
            {
                continue;
            }
            exec.AddEffect((eEffectType)typeInt, param.Substring(splitIdx + 1));
        }
        BattleManager.Instance.pendingActions.Add(exec);
    }
    //其函数都是执行器，接收一个实例作为参数
    public virtual void OnSpellStart()
    {
        ExecEffectes();
    }
    public virtual void OnSpellEnd()
    {

    }
    public void UseAbility()
    {
        //check cost 
        //失败则返回
        SwitchPhase(eAbilityPhase.BeforeStart);
        //OnSpellStart();
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