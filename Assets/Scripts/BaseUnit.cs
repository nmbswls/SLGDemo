using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public class Stats
    {
        public float hp;
        public float maxHp;
        public float atk;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Stats stats = new Stats();

    public void DoDamage(float dmg)
    {
        stats.hp -= dmg;
        if(stats.hp <= 0)
        {
            //chain
            OnDie();
        }
    }

    public void OnDie()
    {

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


    List<ModifierInstance> modifierList = new List<ModifierInstance>();


    public void UpdateModifier()
    {
        for(int i=0; i< properties.Count; i++)
        {
            if (!properties[i].dirty)
            {
                return;
            }

            if (properties[i])
            {

            }

           

        }
    }

    private void GetBase(UnitProperty property)
    {
        if(property.name == "armor")
        {
            int ba = 0;
            ba = 2; //获取敏捷
        }
    }

    List<UnitProperty> properties = new List<UnitProperty>();


    public void AddModifier(string name)
    {
        //从map里寻找
        ModifierConfig config = new ModifierConfig();
        ModifierInstance modifier = new ModifierInstance(config);
        List<ModifierEffectConfig> effects = config.ModifierEffects;
        for (int i = 0; i < effects.Count; i++)
        {
            ModifierEffectInstance effectInst = new ModifierEffectInstance();
            effectInst.config = effects[i];
            effectInst.parent = modifier;
            modifier.Effects.Add(effectInst);

            properties[i].extra.Add(effectInst);
            properties[i].dirty = true;
        }
    }

}


public class UnitProperty
{
    public string name;
    public int idx;
    public List<ModifierEffectInstance> extra = new List<ModifierEffectInstance>();
    public bool dirty;
    public int CalcType; //0 加法 1 乘法 2 线性累乘
    public float calculatedValue;
}

public class UnitIntProperty : UnitProperty
{

}


public class ModifierInstance
{
    public ModifierConfig Config;
    public int Duration;
    public int StackCount;
    public bool isFromAura;

    public List<ModifierEffectInstance> Effects = new List<ModifierEffectInstance>();

    public ModifierInstance(ModifierConfig config)
    {
        this.Config = config;
    }
}

public class ModifierEffectInstance
{
    public ModifierInstance parent;
    public ModifierEffectConfig config;
}

public class ModifierEffectConfig
{
    public int type;
    public string value;
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
