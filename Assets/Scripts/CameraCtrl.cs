using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{


    public GameObject Target;
    public float angle = 45;

    public Camera mainCamera;

    public void Init(Camera mainCamera)
    {
        this.mainCamera = mainCamera;
    }

    private Vector3 target;

    public void MoveDir(Vector3 targetPos)
    {

        this.target = targetPos;

        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, target,0.5f);
    }
}
