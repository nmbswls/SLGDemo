using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[LuaCallCSharp]
public enum eEventType
{
    Kill = 1,
    Pickup = 2,
    Hit = 3,
    Timeout = 4,
}
[CSharpCallLua]
public class AchvEventData
{
    public eEventType type;

}

[CSharpCallLua]
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

public class AchvFactroy
{
    //整合成一个resloader
    public static Dictionary<string, TextAsset> dict = new Dictionary<string, TextAsset>();

    static AchvFactroy()
    {
        TextAsset ta = Resources.Load("Lua/Achv/chengjiu1.lua") as TextAsset;
        dict["chengjiu1"] = ta;
    }


    public static string getLuaScript(string name)
    {
        if (!dict.ContainsKey(name))
        {
            return null;
        }
        return dict[name].text;
    } 
}

[CSharpCallLua]
public delegate int HandleAchvDlg(AchvEventData data);

[CSharpCallLua]
public class AchvInst
{

    public AchvConfig config;
    public int progress; // 复杂的进度信息
    public int state;

    private LuaTable scriptEnv;
    private HandleAchvDlg onHandle;

    public void Init()
    {

        string scrpt = AchvFactroy.getLuaScript(config.name);
        if (scrpt == null)
        {
            return;
        }

        scriptEnv = LuaMain.luaEnv.NewTable();
        

        LuaTable meta = LuaMain.luaEnv.NewTable();
        meta.Set("__index", LuaMain.luaEnv.Global);
        scriptEnv.SetMetaTable(meta);
        meta.Dispose();

        LuaMain.luaEnv.DoString(scrpt, "chunk", scriptEnv);


        scriptEnv.Set("self", this);
        scriptEnv.Get("onHandle", out onHandle);
    }

    public void Handle(AchvEventData data)
    {
        if(onHandle != null)
        {
            onHandle(data);
        }
    }
}





//同时也负责任务
public class AchieveMgr
{

    public static Dictionary<string, AchvConfig> AchvDict = new Dictionary<string, AchvConfig>();

    public List<AchvInst> AchvList = new List<AchvInst>();
    //public List<>
    public Dictionary<eEventType, List<AchvInst>> ListenDict = new Dictionary<eEventType, List<AchvInst>>();

    public Dictionary<Int64, List<AchvInst>> killerListener = new Dictionary<long, List<AchvInst>>();
    public Dictionary<Int64, List<AchvInst>> killedListener = new Dictionary<long, List<AchvInst>>();

    // Start is called before the first frame update
    public void Start()
    {
        AchvInst testAchv = new AchvInst();
        testAchv.config = new AchvConfig();
        testAchv.config.name = "chengjiu1";
        testAchv.Init();

        AchvEventData testData = new AchvEventData();
        testData.type = eEventType.Kill;
        testAchv.Handle(testData);
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
                        List<AchvInst> toCheck = new List<AchvInst>();

                        if (killerListener.ContainsKey(realEvt.Killer))
                        {
                            toCheck.AddRange(killerListener[realEvt.Killer]);

                            for(int i = 0; i < toCheck.Count; i++)
                            {
                                toCheck[i].progress += 1;
                            }
                        }

                        if (killedListener.ContainsKey(realEvt.Killed))
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


