using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeIndicator: MonoBehaviour
{

    private Projector projector;
    private bool isActive;

    
    // Start is called before the first frame update
    public void Init()
    {
        projector = GetComponent<Projector>();
        SetActive(false);
        SetRange(100);
    }

    public void SetActive(bool isActive)
    {
        this.isActive = isActive;
        if (isActive)
        {
            projector.gameObject.SetActive(true);
        }
        else
        {
            projector.gameObject.SetActive(false);
        }
    }

    public void SetRange(int range)
    {
        projector.orthographicSize = range * 1.0f / 100;
    }

    public void SetPosition(Vector3 posXY)
    {
        projector.transform.position = new Vector3(posXY.x, 50f, posXY.z);
    }
}
