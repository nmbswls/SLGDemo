using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class BaseUnit : MonoBehaviour
{
    public Int64 InstId;

    public Vector2Int nowGridPos;

    public void AdjustPos()
    {
        transform.position = BattleManager.Instance.grid1.grid[nowGridPos.x, nowGridPos.y]._worldPos;
    }

    [System.Serializable]
    public class Stats
    {
        public float hp;
        public float maxHp;
        public float atk;
        public Int64 speed;
    }
    public void Init()
    {
        InitProperty();
        InitAbility();
    }


    public AbilityManager mAbilityManager;

    Dictionary<string, AbilityConfig> AbilityConfigMap = new Dictionary<string, AbilityConfig>();
    public List<Ability> AbilityList = new List<Ability>();
    private void InitAbility()
    {

        AbilityConfig Config = new AbilityConfig();
        Config.effects.Add("7,Anim01 start atk");

        Config.effects.Add("3,2");
        //Config.effects.Add("5 50");
        //Config.effects.Add("6 5");

        {
            Ability newAbility = new Ability();

            newAbility.Config = Config;

            AbilityList.Add(newAbility);

        }


        {
            Ability newAbilityLua = new Ability_Lua("ability_panbian");

            newAbilityLua.Config = Config;
            AbilityList.Add(newAbilityLua);
        }
    }

    public void UseAbility(int slot)
    {
        if(slot < 0 || slot >= AbilityList.Count)
        {
            return;
        }

        AbilityList[slot].UseAbility();
    }

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
        BattleManager.Instance.AddEffect(eEffectType.AddBuff, "100001");
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


    public int extraAtk;




    

    public UnitProperty[] PropertyArray;

    public void InitProperty()
    {
        PropertyArray = new UnitProperty[(int)ePropertyName.Max];
        for(int i=1;i< PropertyArray.Length; i++)
        {
            //if(hasproperty)
            PropertyArray[i] = new UnitProperty(this, (ePropertyName)i);
        }
    }
    public UnitProperty FindProperty(ePropertyName name)
    {
        if(name >= ePropertyName.Max)
        {
            return null;
        }
        return PropertyArray[(int)name];
    }


    public void UpdateProperty()
    {

        List<int> modifiedProperty = new List<int>();

        //第一版 无依赖关系
        for(int i=0; i< PropertyArray.Length; i++)
        {
            if(PropertyArray[i] == null)
            {
                continue;
            }
            if (!PropertyArray[i].dirty)
            {
                continue;
            }

            PropertyArray[i].CalcBase();
            PropertyArray[i].CalcTotal();

            PropertyArray[i].dirty = false;
        }
    }

    private Dictionary<ePropertyName, Int64> FixedValueDict = new Dictionary<ePropertyName, Int64>();
    private Int64 GetFixedValue(ePropertyName name)
    {
        if (FixedValueDict.ContainsKey(name))
        {
            return FixedValueDict[name];
        }
        return -1;
    }

    public List<ModifierInstance> ModifierList = new List<ModifierInstance>();

    public Dictionary<string, ModifierConfig> ModifierConfigMap = new Dictionary<string, ModifierConfig>();

    public void AddModifier(string name)
    {
        //从map里寻找
        if (!ModifierConfigMap.ContainsKey(name))
        {
            return;
        }

        ModifierConfig config = ModifierConfigMap[name];
        ModifierInstance modifier = new ModifierInstance(config);
        List<ModifierEffectConfig> effects = config.ModifierEffects;

        for (int i = 0; i < effects.Count; i++)
        {
            UnitProperty toMod = FindProperty(effects[i].propertyName);
            if (toMod == null)
            {
                continue;
            }
            toMod.AddExtraValue(modifier.InstId, effects[i]);
        }
    }

    public void RemoveModifier(string name)
    {

        ModifierInstance toRemove = null;
        for (int i= ModifierList.Count-1; i >= 0; i--)
        {
            if(ModifierList[i].Config.ModifierName == name)
            {
                toRemove = ModifierList[i];
                ModifierList.RemoveAt(i);
            }
        }
        if(toRemove != null)
        {
            List<ModifierEffectConfig> effects = toRemove.Config.ModifierEffects;
            for (int i = 0; i < effects.Count; i++)
            {
                UnitProperty toMod = FindProperty(effects[i].propertyName);
                if (toMod == null)
                {
                    continue;
                }
                toMod.RemoveExtraValue(toRemove.InstId);
            }
        }
    }
}





public class ModifierInstance
{
    public int InstId;
    public ModifierConfig Config;
    public int Duration;
    public int StackCount;
    public bool isFromAura;



    //public List<ModifierEffectInstance> Effects = new List<ModifierEffectInstance>();

    public ModifierInstance(ModifierConfig config)
    {
        this.Config = config;
        InstId = 100;
    }
}

public class ModifierEffectInstance
{
    public ModifierInstance parent;
    public ModifierEffectConfig config;
}

public class ModifierEffectConfig
{
    public ePropertyName propertyName;
    public Int64 value;
}

public class ModifierConfig
{

    public string ModifierName;
    public List<ModifierEffectConfig> ModifierEffects = new List<ModifierEffectConfig>();



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

    public virtual void OnCreate()
    {

    }

    public virtual void OnDestroy()
    {

    }

    public virtual void OnTick()
    {

    }

    public virtual void OnStackCountChanged()
    {

    }

    
}
