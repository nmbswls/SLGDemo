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
    public ModifierEffectConfig config;
}
public class UnitProperty
{
    public BaseUnit owner;
    public bool dirty;

    public ePropertyName name;
    public int idx;

    public Int64 BaseValue;
    public List<EffectNode> extra = new List<EffectNode>();

    public bool isLua = false;
    

    public UnitProperty(BaseUnit owner, ePropertyName name)
    {
        this.owner = owner;
        this.name = name;
    }

    public void AddExtraValue(int buffInstId, ModifierEffectConfig config)
    {
        EffectNode node = new EffectNode();
        node.parentId = buffInstId;
        node.config = config;
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
        //BaseValue = (T)owner.GetBaseProperty<T>(idx);
        return;
    }
    public virtual void CalcTotal()
    {
        return;
    }


}

public class LuaMain
{
    public static LuaEnv luaEnv = new LuaEnv();
}

public class UnitProperty_Lua : UnitProperty
{
    public string luaScript;
    private LuaTable scriptEnv;

    private Action luaCalcBase;
    private Action luaCalcExtra;


    public UnitProperty_Lua(BaseUnit owner, ePropertyName name, string luaScript) : base(owner, name)
    {
        this.luaScript = luaScript;
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

        LuaMain.luaEnv.DoString(luaScript, "script_" + this.name, scriptEnv);


        scriptEnv.Set("self", this);
        scriptEnv.Get("calcBase", out luaCalcBase);
        scriptEnv.Get("calcExtra", out luaCalcExtra);

    }

    public override void CalcBase()
    {
        if (luaCalcBase != null)
        {
            luaCalcBase();
        }
    }

    public override void CalcTotal()
    {
        if (luaCalcExtra != null)
        {
            luaCalcExtra();
        }
    }
}