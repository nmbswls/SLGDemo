using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public delegate void EffectCallback(EffectNode ctx);
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            DoAction("atk");
        }
    }

    public class ActionNode
    {
        public int battleActorIdx;
        public int priority = 999;
    }

    List<ActionNode> NowTurnActionSeq = new List<ActionNode>();
    List<ActionNode> NextTurnActionSeq = new List<ActionNode>();

    public int nowActorIdx = -1;

    public void NextAction()
    {

        ActionNode frontNode = NowTurnActionSeq[0];
        NowTurnActionSeq.RemoveAt(0);
        NextTurnActionSeq.Add(frontNode);
        if (NowTurnActionSeq.Count == 0)
        {
            NextTurn();
            return;
        }

        //更新视图
        if(frontNode.battleActorIdx == -1)
        {
            //ai
        }
    }

    public void NextTurn()
    {

        NowTurnActionSeq = NextTurnActionSeq;
        NextTurnActionSeq = new List<ActionNode>();
    }

    public void DoAction(string name)
    {
        effectList.Clear();
        effectList.Add(new EffectNode());

    }

    public class EffectNode
    {
        public bool isDone;
        public EffectCallback callback;

    }
    public List<EffectNode> effectList = new List<EffectNode>();
    private int idx = -1;
    public void HandleEffect()
    {
        if(idx == -1)
        {
            //no effect
            return;
        }
        if(idx >= effectList.Count)
        {
            //next action
            return;
        }
        EffectNode nowNode = effectList[idx];
        if (nowNode.isDone)
        {
            idx += 1;


        }
    }

    public void NextEffectNode()
    {

    }

    public void EffectEnd(EffectNode node)
    {
        node.isDone = true;

    }

    private float animEndTime;
    private EffectNode nodeNode;
    private void TickEffect()
    {
        if(nodeNode == null)
        {
            return;
        }
        if(Time.time  > animEndTime)
        {
            //callback
            nodeNode.callback(nodeNode);
            nodeNode.isDone = true;
        }
    }
}
