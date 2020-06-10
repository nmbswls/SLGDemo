using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinishTurnBtn : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(delegate ()
        {
            BattleManager.Instance.PlayerFinishTurn();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
