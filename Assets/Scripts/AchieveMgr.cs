using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum eEventType
{
    Kill = 1,
    Pickup = 2,
    Hit = 3,
    Timeout = 4,
}

public class AchvEventData
{
    public eEventType type;

}

public class AchvEventData_Kill : AchvEventData
{
    public Int64 Killer;
    public Int64 Killed;
    public Int64 How;
}

public class AchvConfig
{
    public string name;
    public string condition;
    public List<eEventType> listenType = new List<eEventType>();
}

public class AchvInst
{
    public AchvConfig config;
    public int progress; // 复杂的进度信息
    public int state;
}
//同时也负责任务
public class AchieveMgr
{

    public static Dictionary<string, AchvConfig> AchvDict = new Dictionary<string, AchvConfig>();

    public List<AchvInst> AchvList = new List<AchvInst>();
    //public List<>
    public Dictionary<eEventType, List<AchvInst>> ListenDict = new Dictionary<eEventType, List<AchvInst>>();

    public Dictionary<Int64, List<string>> killerListener = new Dictionary<long, List<string>>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCheckTrigger(AchvEventData data)
    {
        

        switch (data.type)
        {
            case eEventType.Kill:
                {
                    AchvEventData_Kill realEvt = data as AchvEventData_Kill;
                    if(realEvt != null)
                    {
                        if (killerListener.ContainsKey(realEvt.Killer))
                        {

                        }
                    }
                    

                }
                break;

            default:
                break;
        }



        //List<AchvInst> ll = ListenDict[evtType];
        //for (int i = 0; i < ll.Count; i++)
        //{
        //    //if(check)
        //    //gengxin AchvInst progress
        //    //if finish
        //    for(int j = 0; j < ll[i].config.listenType.Count; j++)
        //    {
        //        //listenType 取消注册
        //    }
        //}

        //for(int i=0;i< AchvList.Count; i++)
        //{
        //    //if(AchvList[i])
        //    //start jianting
        //}
    }
}
