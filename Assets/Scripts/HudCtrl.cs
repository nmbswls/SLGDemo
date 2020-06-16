using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudCtrl : MonoBehaviour
{

    public Button AttackBtn;
    public Button NextTurnBtn;
    public Button MoveBtn;
    public Text AttackBtnTxt;

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

        AttackBtnTxt = AttackBtn.GetComponentInChildren<Text>();

        NowTurnInfo = transform.Find("NowTurnInfo").GetComponent<Text>();

        AttackBtn.onClick.AddListener(delegate()
        {

            if (isAttackActive)
            {
                SwitchAttackBtn(false);
                return;
            }

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
    private bool isAttackActive = false;
    public void SwitchAttackBtn(bool active)
    {
        isAttackActive = active;
        if (isAttackActive)
        {
            AttackBtnTxt.text = "取消";
        }
        else
        {
            AttackBtnTxt.text = "攻击";
        }
    }
}
