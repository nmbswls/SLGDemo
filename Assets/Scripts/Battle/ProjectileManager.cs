using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SlgProjectile
{
    public Transform projObj;
    public Vector3 targetPos;
    public long targetId;
    public long sourceId;
    public int speedInt;
    public bool reached;
}

public class ProjectileManager
{
    //pool循环
    public List<SlgProjectile> ProjList = new List<SlgProjectile>();

    public static ProjectileManager Instance = new ProjectileManager();
    private ProjectileManager()
    {
        InitResource();
    }

    public Dictionary<string, GameObject> prefabDict = new Dictionary<string, GameObject>();
    public void Tick(float dTime)
    {
        //运行project
        for(int i = 0; i < ProjList.Count; i++)
        {
            Vector3 diff = (ProjList[i].targetPos - ProjList[i].projObj.position);
            Vector3 moveDir = diff.normalized * dTime *ProjList[i].speedInt * 0.01f;

            if(diff.magnitude <= dTime * ProjList[i].speedInt * 0.01f)
            {
                ProjList[i].reached = true;
            }
            else
            {
                ProjList[i].projObj.position += moveDir;
            }
        }

        for(int i= ProjList.Count-1; i >= 0; i--)
        {
            if (ProjList[i].reached)
            {
                GameObject.Destroy(ProjList[i].projObj.gameObject);
                ProjList.RemoveAt(i);
            }
        }
    }

    private void InitResource()
    {
        GameObject p1 = Resources.Load("Proj_01") as GameObject;
        if(p1 == null)
        {
            Debug.LogError("res miss");
            return;
        }
        prefabDict["p1"] = p1;
    }

    public SlgProjectile createProjectile(BaseUnit target, BaseUnit origin, int speedInt)
    {
        GameObject inst = GameObject.Instantiate(prefabDict["p1"]);
        SlgProjectile proj = new SlgProjectile();
        proj.projObj = inst.transform;
        proj.speedInt = speedInt;
        proj.targetPos = target.GetWorldPos() + target.HitPoint;
        Debug.Log(target.HitPoint);
        proj.projObj.position = origin.GetWorldPos() + origin.ActPoint;
        return proj;
    }

    public void AddProj(SlgProjectile proj)
    {
        ProjList.Add(proj);
    }



}
