using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using XLua.LuaDLL;

public enum ePropertyName
{
    Hp = 1,
    MaxHp = 2,
    Speed = 3,
    PAtk = 4,
    MAtk = 5,
    Max = 6,
}
public class EffectNode
{
    public int parentId;
    public Int64 value;
}



public class UnitPropertyConfig
{
    public ePropertyName name;
    public bool isLua;
    public int calcMode; //1 add 2 multi 3 or
    public int valueType; //0 int 2 float 3 bit
    public List<ePropertyName> dependencies;
    public string method; 

    public UnitPropertyConfig(ePropertyName name, int calcMode, int valueType, bool isLua)
    {
        this.name = name;
        this.isLua = isLua;
        this.calcMode = calcMode;
        this.valueType = valueType;
    }

 
}


public class UnitProperty
{
    public BaseUnit owner;
    public bool dirty;

    //public ePropertyName name;
    //public int idx;

    public UnitPropertyConfig Config;

    public Int64 BaseValue;
    public List<EffectNode> extra = new List<EffectNode>();

    //public bool isLua = false;
    public Int64 FinalValue;
    //public int calcMode; //1 add 2 multi 3 or

    public UnitProperty(BaseUnit owner, UnitPropertyConfig config)
    {
        this.owner = owner;
        this.Config = config;
    }

    public void AddExtraValue(int buffInstId, ModifierEffect config)
    {
        EffectNode node = new EffectNode();
        node.parentId = buffInstId;
        node.value = config.value;
        extra.Add(node);

        dirty = true;
    }

    public void RemoveExtraValue(int buffInstId)
    {
        for(int i = extra.Count - 1; i >= 0; i--)
        {
            if(extra[i].parentId == buffInstId)
            {
                extra.RemoveAt(i);
            }
        }
        dirty = true;
    }

    public virtual void CalcBase()
    {
        BaseValue = owner.GetConfigProperty((int)Config.name);
    }
    public virtual void CalcTotal()
    {
        FinalValue = 0;
        if (Config.calcMode == 0)
        {
            for(int i = 0; i < extra.Count; i++)
            {
                FinalValue += extra[i].value;
            }
            FinalValue += BaseValue;
        }
        return;
    }


}

public class LuaMain
{
    public static LuaEnv luaEnv = new LuaEnv();
}

//public class UnitProperty_Lua : UnitProperty
//{
//    public string luaScript;
//    private LuaTable scriptEnv;

//    private Action luaCalcBase;
//    private Action luaCalcExtra;


//    public UnitProperty_Lua(BaseUnit owner, ePropertyName name, string luaScript) : base(owner, name)
//    {
//        this.luaScript = luaScript;
//        InitLua();
//    }


//    private void InitLua()
//    {

//        if (luaScript == null)
//        {
//            return;
//        }

//        scriptEnv = LuaMain.luaEnv.NewTable();

//        // 为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
//        LuaTable meta = LuaMain.luaEnv.NewTable();
//        meta.Set("__index", LuaMain.luaEnv.Global);
//        scriptEnv.SetMetaTable(meta);
//        meta.Dispose();

//        LuaMain.luaEnv.DoString(luaScript, "script_" + this.name, scriptEnv);


//        scriptEnv.Set("self", this);
//        scriptEnv.Get("calcBase", out luaCalcBase);
//        scriptEnv.Get("calcExtra", out luaCalcExtra);

//    }

//    public override void CalcBase()
//    {
//        if (luaCalcBase != null)
//        {
//            luaCalcBase();
//        }
//    }

//    public override void CalcTotal()
//    {
//        if (luaCalcExtra != null)
//        {
//            luaCalcExtra();
//        }
//    }
//}