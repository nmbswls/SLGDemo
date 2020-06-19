using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{

    public static float RotSpeed = 180f;
    public bool IsLockToTarget
    {
        get { return LockTarget != null; }
    }
    public GameObject LockTarget = null;

    public Vector3 targetPos;

    private float _offset = -10f;
    private float _defHeight = 10f;
    private float _roll = 45;
    private float Rotation = 0;

    private float _tmprot = 0;

    private Vector3 basePosition;
    private Vector3 RotOffset = Vector3.zero;
    public Camera mainCamera;

    public void Start()
    {
        mainCamera = Camera.main;
        mainCamera.transform.localEulerAngles = new Vector3(_roll, 0, 0);
        CalcOffset();
    }

    public void SetOffset(float offset)
    {
        _offset = offset;
    }

    public void RotNext45()
    {
        Rotation += 45;
        if(Rotation >= 360)
        {
            Rotation = Rotation % 360;
        }


    }

    public void RotPrev45()
    {
        Rotation -= 45;
        if(Rotation < 0)
        {
            Rotation = (Rotation + 360);
        }
        
    }

    public void UpdateRotOffset()
    {

        if(Mathf.Abs(Rotation - _tmprot) < 1e-1)
        {
            return;
        }

        float sign = 0;
        float moveDist;

        if (Rotation > _tmprot)
        {
            if(Rotation - _tmprot > 360 - Rotation + _tmprot)
            {
                sign = -1;
                moveDist = 360 - Rotation + _tmprot;
            }
            else
            {
                sign = 1;
                moveDist = Rotation - _tmprot;
            }
        }
        else
        {
            if (_tmprot - Rotation > 360 - _tmprot + Rotation)
            {
                sign = 1;
                moveDist = 360 - _tmprot + Rotation;
            }
            else
            {
                sign = -1;
                moveDist = _tmprot - Rotation;
            }
        }

        

        if(moveDist <= RotSpeed * Time.deltaTime)
        {
            _tmprot = Rotation;
        }
        else
        {
            _tmprot += sign * RotSpeed * Time.deltaTime;
        }
        _tmprot = (_tmprot + 360) % 360;
        mainCamera.transform.localEulerAngles = new Vector3(_roll, _tmprot, 0);

        CalcOffset();
    }

    public void CalcOffset()
    {
        Vector3 forward = mainCamera.transform.forward;
        forward.y = 0;
        forward.Normalize();
        RotOffset = forward * -10f + Vector3.up * 10f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RotPrev45();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            RotNext45();
        }
        UpdateRotOffset();
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if(IsLockToTarget)
        {
            basePosition = LockTarget.transform.position;
            mainCamera.transform.position = basePosition + RotOffset;
            return;
        }
        else
        {
            basePosition = Vector3.Lerp(basePosition, targetPos, 0.3f);
            mainCamera.transform.position = basePosition + RotOffset;
            //mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos + RotOffset,0.4f);
            //mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos + new Vector3(0, _defHeight, _offset) + RotOffset, 0.3f);
        }
        
        
    }

    public void LookTo(Transform target)
    {
        this.targetPos = target.position;
    }

    public void OnDragScreen(Vector2 delta)
    {
        //可以通过rot直接计算


        Vector3 forward = mainCamera.transform.forward;
        forward.y = 0;
        forward.Normalize();
        forward *= -delta.y * 0.015f;

        Vector3 right = mainCamera.transform.right;
        right.y = 0;
        right.Normalize();
        right *= -delta.x * 0.015f;


        //forward.x *= -delta.x * 0.015f;
        //forward.z *= -delta.y * 0.015f;

        targetPos += (forward + right);

        Debug.Log(forward);

        //targetPos += new Vector3(-delta.x * 0.015f, 0 , -delta.y*0.018f);
    }
}
