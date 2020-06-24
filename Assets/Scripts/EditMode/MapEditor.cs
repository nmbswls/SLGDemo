using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapEditor : MonoBehaviour
{
    public Grid grid0;

    Camera mainCamera;



    public void LoadFromFile()
    {
        //length = 1000;
        //width = 1000;
        string path = Application.dataPath + "/Json/MyTxtByFileStream.txt";
        // 文件流创建一个文本文件
        FileStream file = new FileStream(path, FileMode.Create);
        //得到字符串的UTF8 数据流
        byte[] bts = System.Text.Encoding.UTF8.GetBytes("input");
        // 文件写入数据流
        file.Write(bts, 0, bts.Length);
        if (file != null)
        {
            //清空缓存
            file.Flush();
            // 关闭流
            file.Close();
            //销毁资源
            file.Dispose();
        }

    }

    Ray ray;

    public void Calculate()
    {
        Vector2Int siz = grid0.getMaxGridXY();
        for (int x = 0; x < siz.x; x++)
        {
            for(int y = 0; y < siz.y; y++)
            {
                Vector3 _worldPos = grid0.grid[x,y]._worldPos;
                RaycastHit hit;
                ray = new Ray();
                bool ret = Physics.BoxCast(_worldPos + Vector3.up * 100f,new Vector3(grid0.nodeRadius, grid0.nodeRadius, grid0.nodeRadius),Vector3.down,out hit, Quaternion.identity, 100f, (1 << LayerMask.NameToLayer("Obc")));
                //Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (ret)
                {
                    grid0.grid[x, y].blocked = true;
                }
            }
        }

        //写入文件
        //下次不做碰撞测试

    }


    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            Calculate();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            LoadFromFile();
        }
    }
}
