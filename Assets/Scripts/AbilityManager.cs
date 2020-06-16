using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AbilityFactory { 


}

public class AbilityManager
{
    public static int MAX_ABILITY_COUNT = 30;
    public List<Ability> AbilityList = new List<Ability>();

    public void Tick()
    {
        for(int i=0; i< AbilityList.Count; i++)
        {
            if (AbilityList[i].NowPhase == eAbilityPhase.BeforeStart)
            {
                if (AbilityList[i].UseSkillTimer + AbilityList[i].Config.PointTime >= Time.time)
                {
                    AbilityList[i].SwitchPhase(eAbilityPhase.Spelling);
                }
            }else if (AbilityList[i].NowPhase == eAbilityPhase.Spelling)
            {
                if(AbilityList[i].UseSkillTimer + AbilityList[i].Config.TotalTime >= Time.time)
                {
                    AbilityList[i].SwitchPhase(eAbilityPhase.UnUsed);
                }
            }
        }
    }

    public Ability FindAbility(string name)
    {
        for(int i=0;i< AbilityList.Count; i++)
        {
            if(AbilityList[i].AbilityName == name)
            {
                return AbilityList[i];
            }
        }
        return null;
    }
}
