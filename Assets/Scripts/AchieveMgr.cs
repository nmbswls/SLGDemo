using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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

public enum eQuestState
{
    UnStart,
    InProgress,
    Finished,
    Fail,
}


[CSharpCallLua]
public class QuestEventData
{
    public eEventType type;
}

[CSharpCallLua]
public class QuestEventData_Kill : QuestEventData
{
    public Int64 Killer;
    public Int64 Killed;
    public Int64 How;
}


//优化监听，关闭不遍历
//startCondition

public class QuestConfig
{
    public string name;
    public string condition;
    public List<QuestListenType> listenList;
    //除了listenType 还要细分
    public LogicTree logicTree;
    public int ProgressCnt; //任务有几个进度值
}

public class QuestListenType
{
    public eEventType type;
    public eEventDetailType detailType;
    public string extra; //监听几号怪物死，等
    public int respIdx; //决定使用哪个progress值来记录变化 -1时说明不care输入，只check
    public QuestListenType(eEventType type)
    {
        this.type = type;
    }
}



public enum eEventDetailType
{
    KillerId,
    KillerType,
    KilledId,
    KilledTag,

    //成就累加
}



public class QuestLuaLoader
{
    //整合成一个resloader
    public static Dictionary<string, TextAsset> dict = new Dictionary<string, TextAsset>();

    static QuestLuaLoader()
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
public delegate int HandleAchvDlg(QuestEventData data);

[CSharpCallLua]
public class QuestInst
{

    public QuestConfig config;
    public List<Int64> progress; // 复杂的进度信息
    public eQuestState state= eQuestState.UnStart;

    public int stageIdx = 0;

    


    private LuaTable scriptEnv;
    private HandleAchvDlg onHandle;
    private Action checkFinish;

    public QuestInst(QuestConfig config)
    {
        this.config = config;
        InitLua();
        progress = new List<Int64>();
    }

    public void InitLua()
    {

        string scrpt = QuestLuaLoader.getLuaScript(config.name);
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
        scriptEnv.Get("checkFinish", out checkFinish);
        
    }

    public void Handle(QuestEventData data)
    {
        
        Debug.Log("handle " + config.name);
        if(config.name == "sharen1")
        {
            progress[0] += 1;
        }
        //if(data.type == eEventType.Kill)
        //{
        //遍历检查监听表，如果有监听杀id 监听杀type 监听杀tag等，则将progress累加 progress对应关系？
        //}

        //如果是复杂业务，则写入lua中
        if (onHandle != null)
        {
            onHandle(data);
        }
    }

    public void CheckFinish()
    {
        //do something
        bool finished = false;
        ///use lua to handle  complex
        if (finished)
        {

        }
        else
        {

        }
        if (checkFinish != null)
        {
            checkFinish();
        }

    }

    public void RegisterListner()
    {
        
        
    }
}


public class AchvChecker : LogicTreeChecker
{
    public override bool CheckNode(string paramstring)
    {
        string funcName;
        string[] args;
        int idxArgLeft = paramstring.IndexOf('(');
        if (idxArgLeft == -1)
        {
            funcName = paramstring;
            args = null;
        }
        else
        {
            if(paramstring[paramstring.Length-1] != ')')
            {
                Debug.Log("check param string error");
                return false;
            }
            funcName = paramstring.Substring(0, idxArgLeft);
            string argstring = paramstring.Substring(idxArgLeft+1, paramstring.Length - idxArgLeft -2);
            args = argstring.Split(',');
        }

        switch (funcName)
        {
            case "p1":
                int v = int.Parse(args[0]);
                int opt = int.Parse(args[1]);
                break;

            default:
                break;
        }

        return true;
    }
}


//同时也负责任务
public class AchieveMgr
{

    public static Dictionary<string, QuestConfig> AchvDict = new Dictionary<string, QuestConfig>();

    public List<QuestInst> AchvList = new List<QuestInst>();
   
    //不分粒度
    public Dictionary<eEventType, List<QuestInst>> ListenDict = new Dictionary<eEventType, List<QuestInst>>();

    //
    //public List<AchvInst> KillEventListener = new List<AchvInst>();
    public Dictionary<Int64, List<QuestInst>> killerListener = new Dictionary<long, List<QuestInst>>();
    public Dictionary<Int64, List<QuestInst>> killedListener = new Dictionary<long, List<QuestInst>>();

    // Start is called before the first frame update
    public void Start()
    {
        //AchvInst testAchv = new AchvInst();
        //testAchv.config = new AchvConfig();
        //testAchv.config.name = "chengjiu1";
        //testAchv.Init();
        LoadConfig();

        ListenDict[eEventType.Kill] = new List<QuestInst>();
        ListenDict[eEventType.Hit] = new List<QuestInst>();
        ListenDict[eEventType.Pickup] = new List<QuestInst>();
        ListenDict[eEventType.Timeout] = new List<QuestInst>();


        foreach (var config in AchvDict.Values)
        {
            QuestInst testAchv = new QuestInst(config);
            //load progress
            if(testAchv.state != eQuestState.InProgress)
            {
                continue;
            }
            //register listener
            for(int i=0;i< config.listenList.Count; i++)
            {
                ListenDict[config.listenList[i].type].Add(testAchv);
            }
        }


        LogicTreeNode header = LogicTree.ConstructFromString("a&b&(c|d)|(e&f)");

        Debug.Log("");

    }

    public void LoadConfig()
    {
        {
            QuestConfig config = new QuestConfig();
            config.name = "sharen1";
            config.listenList = new List<QuestListenType>() { new QuestListenType(eEventType.Kill)};
            AchvDict.Add(config.name, config);
        }
        {
            QuestConfig config = new QuestConfig();
            config.name = "sharen2";
            config.listenList = new List<QuestListenType>() { new QuestListenType(eEventType.Kill) };
            AchvDict.Add(config.name, config);
        }
        {
            QuestConfig config = new QuestConfig();
            config.name = "sharen3";
            config.listenList = new List<QuestListenType>() { new QuestListenType(eEventType.Kill) };
            AchvDict.Add(config.name, config);
        }
        {
            QuestConfig config = new QuestConfig();
            config.name = "sharen4";
            config.listenList = new List<QuestListenType>() { new QuestListenType(eEventType.Kill) };
            AchvDict.Add(config.name, config);
        }

    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCheckTrigger(QuestEventData data)
    {
        //粗粒度
        List<QuestInst> toCheck = ListenDict[data.type];

        for (int i = 0; i < toCheck.Count; i++)
        {
            toCheck[i].Handle(data);
        }

        //switch (data.type)
        //{
        //    case eEventType.Kill:
        //        {
        //            AchvEventData_Kill realEvt = data as AchvEventData_Kill;
        //            if(realEvt == null)
        //            {
        //                break;
        //            }


        //            List<AchvInst> toCheck = ListenDict[data.type];


        //            for(int i = 0; i < toCheck.Count; i++)
        //            {
        //                toCheck[i].Handle(data);
        //            }

        //            //List<AchvInst> toCheck = new List<AchvInst>();


        //            //if (killerListener.ContainsKey(realEvt.Killer))
        //            //{
        //            //    toCheck.AddRange(killerListener[realEvt.Killer]);
        //            //}

        //            //if (killedListener.ContainsKey(realEvt.Killed))
        //            //{
        //            //    toCheck.AddRange(killedListener[realEvt.Killed]);
        //            //}
                    
        //            //for(int i = 0; i < toCheck.Count; i++)
        //            //{
        //            //    toCheck[i].Handle(data);
        //            //}
        //        }
        //        break;
        //    case eEventType.Pickup:
        //        {

        //        }
        //        break;
        //    default:
        //        break;
        //}



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


