using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;


public class UnitPropertyFactroy
{
    public static List<UnitPropertyConfig> configList = new List<UnitPropertyConfig>();

    static UnitPropertyFactroy(){


        {
            UnitPropertyConfig config = new UnitPropertyConfig(ePropertyName.Hp, 0, 0, false);
            config.dependencies.Add(2);
            configList.Add(config);
        }
        {
            UnitPropertyConfig config = new UnitPropertyConfig(ePropertyName.MAtk, 0, 0, false);
            
            configList.Add(config);
        }
        {
            UnitPropertyConfig config = new UnitPropertyConfig(ePropertyName.MaxHp, 0, 0, false);
            config.dependencies.Add(5);
            configList.Add(config);
        }
        {
            UnitPropertyConfig config = new UnitPropertyConfig(ePropertyName.PAtk, 0, 0, false);
            configList.Add(config);
        }
        {
            UnitPropertyConfig config = new UnitPropertyConfig(ePropertyName.Speed, 0, 0, false);
            configList.Add(config);
        }
        
        


        for (int i=0;i< configList.Count; i++)
        {
            UnitPropertyConfig config = configList[i];
            if(config.dependencies == null)
            {
                continue;
            }
            for(int j = 0; j < config.dependencies.Count; j++)
            {
                int d0 = (int)config.dependencies[j];
                if (!propertyDependency.ContainsKey(d0))
                {
                    propertyDependency[d0] = new List<int>();
                }

                Debug.Log("add dependency:" + d0 + " trigger " + i);

                propertyDependency[d0].Add((int)config.name);
            }
        }


    }
    public static void Init(BaseUnit target)
    {
        target.PropertyArray = new UnitProperty[(int)ePropertyName.Max];
        for (int i = 0; i < configList.Count; i++)
        {
            UnitProperty property = new UnitProperty(target, configList[i]);
            target.PropertyArray[(int)configList[i].name] = property;
        }
    }

    public static Dictionary<int, List<int>> propertyDependency = new Dictionary<int, List<int>>();
    public static List<int> GetTrigger(int idx)
    {
        if (propertyDependency.ContainsKey(idx))
        {
            return propertyDependency[idx];
        }
        return null;
    }
}

public class UnitConfig
{
    public Vector3 HitPoint;
    public Vector3 ActPoint;

    public Int64[] PropertyArray = new Int64[(int)ePropertyName.Max];
}
public class BaseUnit : MonoBehaviour
{
    public Int64 InstId;
    public UnitConfig Config = new UnitConfig();

    public Vector2Int nowGridPos;

    //放入config中
    public Vector3 HitPoint = Vector3.up * 0.5f;
    public Vector3 ActPoint = Vector3.up * 1.5f;
    public float MovePoint = 0;
    
    public void AdjustPos()
    {
        transform.position = BattleManager.Instance.grid1.grid[nowGridPos.x, nowGridPos.y]._worldPos;
    }

    public Vector2 GetWorldPos2D()
    {
        Vector3 pos = BattleManager.Instance.grid1.grid[nowGridPos.x, nowGridPos.y]._worldPos;
        return new Vector2(pos.x, pos.z);
    }
    public Vector3 GetWorldPos()
    {
        Vector3 pos = BattleManager.Instance.grid1.grid[nowGridPos.x, nowGridPos.y]._worldPos;
        return pos;
    }

    [System.Serializable]
    public class Stats
    {
        public float hp;
        public float maxHp;
        public float atk;
        public Int64 speed;
    }

    SceneClickable listener;
    public void Init()
    {
        HitPoint = Vector3.up * 0.5f;
        ActPoint = Vector3.up * 1.5f;

        InitProperty();
        InitAbility();

        listener = gameObject.AddComponent<SceneClickable>();
        listener.ClickEvent += OnClick;
    }
    public void OnClick(SceneClickData data)
    {
        BattleManager.Instance.OnActorClick(this);
    }

    public AbilityManager mAbilityManager;

    Dictionary<string, AbilityConfig> AbilityConfigMap = new Dictionary<string, AbilityConfig>();
    public List<Ability> AbilityList = new List<Ability>();


    #region property

    public int tmpAtk = 120;
    public int tmpDef = 222;

    public UnitProperty[] PropertyArray;

    public void InitProperty()
    {
        UnitPropertyFactroy.Init(this);
    }
    public void UpdateProperty()
    {
        int cnt = 0;
        while (true)
        {
            bool changed = false;
            //第一版 无依赖关系
            for (int i = 0; i < PropertyArray.Length; i++)
            {
                if (PropertyArray[i] == null)
                {
                    continue;
                }
                if (!PropertyArray[i].dirty)
                {
                    continue;
                }
                changed = true;
                PropertyArray[i].CalcBase();
                PropertyArray[i].CalcTotal();

                Debug.Log("calc " + i);

                List<int> mayChange = UnitPropertyFactroy.GetTrigger(i);
                if (mayChange != null)
                {
                    for (int j = 0; j < mayChange.Count; j++)
                    {
                        Debug.Log("may change "+mayChange[j]);
                        PropertyArray[mayChange[j]].dirty = true;
                    }
                }
                PropertyArray[i].dirty = false;
            }
            if (!changed)
            {
                break;
            }

            if(cnt > 3)
            {
                Debug.LogError("不可能出现三层套娃的业务");
                break;
            }
        }
        
    }
    public UnitProperty FindProperty(ePropertyName name)
    {
        if (name >= ePropertyName.Max)
        {
            return null;
        }
        return PropertyArray[(int)name];
    }

    public Int64 GetFinalProperty(int idx)
    {
        
        if (idx < 0 || idx >= PropertyArray.Length)
        {
            return 0;
        }
        if (PropertyArray[idx] == null)
        {
            return 0;
        }
        return PropertyArray[idx].FinalValue;
    }

    public Int64 GetConfigProperty(int idx)
    {
        if(Config == null)
        {
            return 0;
        }

        if(idx < 0 || idx >= Config.PropertyArray.Length)
        {
            return 0;
        }

        return Config.PropertyArray[idx];
    }



    #endregion


    #region ability

    private void InitAbility()
        {

            AbilityConfig Config = new AbilityConfig();
            Config.RangeInt = 600;
            Config.effects.Add("7,Anim01 start atk");

            Config.effects.Add("5,2");
            Config.effects.Add("10");
            //Config.effects.Add("5 50");
            //Config.effects.Add("6 5");

            {
                Ability newAbility = new Ability();

                newAbility.Config = Config;
                newAbility.owner = this;

                AbilityList.Add(newAbility);

            }


            {
                Ability newAbilityLua = new Ability_Lua("ability_panbian");

                newAbilityLua.Config = Config;
                newAbilityLua.owner = this;

                AbilityList.Add(newAbilityLua);
            }
        }
    #endregion







    public void Tick(float dTime)
    {
        UpdateProperty();
    }

    public Stats stats = new Stats();

    public void DoDamage(float dmg)
    {
        stats.hp -= dmg;
        if(stats.hp <= 0)
        {
            //chain
            Debug.Log("die once");
            BattleManager.Instance.DoDie(this);
        }
    }

    public void OnDie()
    {
        ActionNode node = ActionNodeFactroy.CreateFromString(eEffectType.AddBuff, "100001");
        BattleManager.Instance.actionExecutor.AddActionNode(node);
    }



    public enum eUnitState
    {
        NONE = 0,
        STUNNED = 0x01,
        ROOOTED = 0x02,
        SILENCED = 0x04,
    }


    public UInt64 mUnitState = 0;
    public void SetState(UInt64 state)
    {
        mUnitState |= state;
    }


    #region modifier

    public List<ModifierInstance> ModifierList = new List<ModifierInstance>();

    public Dictionary<string, ModifierConfig> ModifierConfigMap = new Dictionary<string, ModifierConfig>();

    public void AddModifierAtkBig()
    {
        if (!ModifierConfigMap.ContainsKey("big atk"))
        {
            ModifierConfig config = new ModifierConfig();
            {
                ModifierEffect eee = new ModifierEffect();
                eee.propertyName = ePropertyName.MAtk;
                eee.value = 10000;
                config.ModifierEffects.Add(eee);
            }
            ModifierConfigMap["big atk"] = config;
        }
        AddModifier("big atk");
    }

    public void AddModifierAtkSmall()
    {
        if (!ModifierConfigMap.ContainsKey("small atk"))
        {
            ModifierConfig config = new ModifierConfig();
            {
                ModifierEffect eee = new ModifierEffect();
                eee.propertyName = ePropertyName.MAtk;
                eee.value = 100;
                config.ModifierEffects.Add(eee);
            }
            ModifierConfigMap["small atk"] = config;
        }
        AddModifier("small atk");
    }


    public void AddModifier(string name)
    {
        //从map里寻找
        if (!ModifierConfigMap.ContainsKey(name))
        {
            return;
        }

        ModifierConfig config = ModifierConfigMap[name];
        ModifierInstance modifier = new ModifierInstance(this, config);
        List<ModifierEffect> effects = config.ModifierEffects;

        ModifierList.Add(modifier);

        for (int i = 0; i < effects.Count; i++)
        {
            UnitProperty toMod = FindProperty(effects[i].propertyName);
            if (toMod == null)
            {
                continue;
            }
            toMod.AddExtraValue(modifier.InstId, effects[i]);
        }
        UpdateProperty();
    }



    private bool isDoingRemove = false;
    private bool isDoingAdd = false;
    List<ModifierInstance> toRemoveList = new List<ModifierInstance>();
    List<ModifierInstance> toAddList = new List<ModifierInstance>();

    public void RemoveAllModifier()
    {
        //List<ModifierInstance>
        for (int i = ModifierList.Count - 1; i >= 0; i--)
        {
            //toRemove = ModifierList[i];
            toRemoveList.Add(ModifierList[i]);
            ModifierList[i].isRemoved = true;
            ModifierList.RemoveAt(i);
        }

        if (!isDoingRemove)
        {
            DoHandleRemoveList();
        }

        //while (toRemoveList.Count > 0)
        //{
        //    ModifierInstance inst = toRemoveList[0];
        //    inst.RemoveEffects();
        //    inst.OnDestroy();
        //    toRemoveList.RemoveAt(0);
        //}
        
    }

    public void DoHandleRemoveList()
    {
        isDoingRemove = true;
        while (toRemoveList.Count > 0)
        {
            ModifierInstance inst = toRemoveList[0];
            inst.RemoveEffects();
            inst.OnDestroy();
            toRemoveList.RemoveAt(0);
        }
        UpdateProperty();
        isDoingRemove = false;
    }

    public void RemoveModifier(string name)
    {

        ModifierInstance toRemove;
        for (int i = ModifierList.Count - 1; i >= 0; i--)
        {
            if (ModifierList[i].Config.ModifierName == name)
            {
                if (ModifierList[i].isRemoved)
                {
                    continue;
                }
                toRemove = ModifierList[i];
                toRemove.isRemoved = true;
                toRemoveList.Add(toRemove);
                ModifierList.RemoveAt(i);
            }
        }

        if (!isDoingRemove)
        {
            DoHandleRemoveList();

        }

        //destoy flag while while1 while2
    }

    #endregion

    public void DestroyModifer()
    {
        for (int i = ModifierList.Count - 1; i >= 0; i--)
        {
            if (ModifierList[i].Config.ModifierName == name)
            {
                if (ModifierList[i].isRemoved)
                {
                    continue;
                }
                ModifierList[i].needDestroy = true;
            }
        }
    }


    //入口 移除 回合结束
    public void TestDestroy()
    {
        for (int i = 0; i <= ModifierList.Count; i--)
        {
            ModifierInstance modifier = ModifierList[i];

            modifier.OnTurnEnd();

            modifier.Duration -= 1;
            if (modifier.Duration <= 0)
            {
                modifier.needDestroy = true;
            }
        }

        while (true)
        {
            bool changed = false;
            for (int i = ModifierList.Count - 1; i >= 0; i--)
            {
                if (ModifierList[i].needDestroy)
                {
                    ModifierList[i].isRemoved = true;
                    toRemoveList.Add(ModifierList[i]);
                    ModifierList.RemoveAt(i);
                    changed = true;
                }
            }

            while(toRemoveList.Count > 0)
            {
                ModifierInstance inst = toRemoveList[0];
                inst.RemoveEffects();
                inst.OnDestroy();
                toRemoveList.RemoveAt(0);
            }

            if (!changed)
            {
                break;
            }

        }

        UpdateProperty();
    }

    
    public void OnTurnFinish()
    {

        for (int i = ModifierList.Count-1; i >= 0; i--)
        {
            ModifierInstance modifier = ModifierList[i];

            modifier.OnTurnEnd();

            modifier.Duration -= 1;
            if (modifier.Duration <= 0)
            {
                modifier.isRemoved = true;
                toRemoveList.Add(modifier);
            }
        }

        //for(int i=0;i< toRemoveList.Count; i++)
        //{
        //    ModifierList.Remove(toRemoveList[i]);
        //}
    }

    public void OnTurnBegin()
    {
        MovePoint = 5.0f;
    }

}





public class ModifierInstance
{
    public int InstId;
    public BaseUnit owner;
    public ModifierConfig Config;
    public int Duration;
    public int StackCount;
    public bool isFromAura;

    public bool needDestroy = false;
    public bool isRemoved = false;

    //public List<ModifierEffectInstance> Effects = new List<ModifierEffectInstance>();

    public ModifierInstance(BaseUnit owner, ModifierConfig config)
    {
        this.owner = owner;
        this.Config = config;
        InstId = 100;
    }

    public virtual void OnCreate()
    {

    }

    public virtual void OnDestroy()
    {
        if(Config.ModifierName == "big atk")
        {
            owner.RemoveModifier("small atk");
        }
    }


    public void RemoveEffects()
    {
        List<ModifierEffect> effects = Config.ModifierEffects;
        for (int i = 0; i < effects.Count; i++)
        {
            UnitProperty toMod = owner.FindProperty(effects[i].propertyName);
            if (toMod == null)
            {
                continue;
            }
            toMod.RemoveExtraValue(InstId);
        }
    }

    public virtual void OnTurnBegin()
    {

    }

    public virtual void OnTurnEnd()
    {
        for(int i=0;i< Config.TurnEndEffects.Count; i++)
        {
            Debug.Log(Config.TurnEndEffects[i]);
        }
    }

    public virtual void OnStackCountChanged()
    {

    }
}

//public class ModifierEffectInstance
//{
//    public ModifierInstance parent;
//    public ModifierEffectConfig config;
//}

public class ModifierEffect
{
    public ePropertyName propertyName;
    public Int64 value;
}

public class ModifierConfig
{

    public string ModifierName;
    public List<ModifierEffect> ModifierEffects = new List<ModifierEffect>();



    public void GetSpeedBonus()
    {

    }

    private bool _isHidden;
    public bool IsHidden{
        get { return _isHidden; }
        set { _isHidden = value; }
    }
    private bool _removeOnDeath;
    public bool RemoveOnDeath
    {
        get { return _removeOnDeath; }
        set { _removeOnDeath = value; }
    }

    private bool _isAura;
    public bool IsAura
    {
        get { return _isAura; }
        set { _isAura = value; }
    }

    private bool _isDebuff;
    public bool IsDebuff
    {
        get { return _isDebuff; }
        set { _isDebuff = value; }
    }

    private string _textureName;
    public string TextureName
    {
        get { return _textureName; }
        set { _textureName = value; }
    }

    public List<string> TurnEndEffects = new List<string>();
    public List<string> CreateEffects = new List<string>();

}
