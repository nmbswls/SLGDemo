using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudCtrl : MonoBehaviour
{

    public Button AttackBtn;
    public Button NextTurnBtn;
    public Button MoveBtn;

    public Text NowTurnInfo;

    // Start is called before the first frame update
    void Start()
    {
        BindView();
    }

    public void BindView()
    {
        AttackBtn = transform.Find("AttackBtn").GetComponent<Button>();
        NextTurnBtn = transform.Find("NextTurnBtn").GetComponent<Button>();
        MoveBtn = transform.Find("MoveBtn").GetComponent<Button>();

        NowTurnInfo = transform.Find("NowTurnInfo").GetComponent<Text>();

        AttackBtn.onClick.AddListener(delegate()
        {
            BattleManager.Instance.StartChooseTarget();

        });

        NextTurnBtn.onClick.AddListener(delegate ()
        {
            BattleManager.Instance.PlayerFinishTurn(); 

        });

        MoveBtn.onClick.AddListener(delegate ()
        {

            BattleManager.Instance.ConfirmMove();
        });
    }
}
