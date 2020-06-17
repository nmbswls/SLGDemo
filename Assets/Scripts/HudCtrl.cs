using System;
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

    public Transform InfoPanel;
    public Text UnitName;
    public Text UnitAtkText;
    public Text UnitDefText;

    // Start is called before the first frame update
    void Start()
    {
        BindView();
    }

    public void BindView()
    {
        AttackBtn = transform.Find("AttackBtn").GetComponent<Button>();
        NextTurnBtn = transform.Find("NextTurnBtn").GetComponent<Button>();

        AttackBtnTxt = AttackBtn.GetComponentInChildren<Text>();

        NowTurnInfo = transform.Find("NowTurnInfo").GetComponent<Text>();


        InfoPanel = transform.Find("InfoPanel");
        UnitName = InfoPanel.Find("Name").GetComponent<Text>();
        UnitAtkText = InfoPanel.Find("Atk_value").GetComponent<Text>();
        UnitDefText = InfoPanel.Find("Def_value").GetComponent<Text>();

        AttackBtn.onClick.AddListener(delegate()
        {
            bool changed;
            if (isAttacking)
            {
                changed = BattleManager.Instance.HandleCancelAttack();
            }
            else
            {
                changed = BattleManager.Instance.HandleStartAttack();
            }
            if (changed)
            {
                SwitchAttackBtn();
            }
        });

        NextTurnBtn.onClick.AddListener(delegate ()
        {
            BattleManager.Instance.PlayerFinishTurn(); 

        });

        
    }
    private bool isAttacking = false;
    public void SwitchAttackBtn()
    {
        isAttacking = !isAttacking;
        if (isAttacking)
        {
            AttackBtnTxt.text = "取消";
        }
        else
        {
            AttackBtnTxt.text = "攻击";
        }
    }

    private BaseUnit nowShowUnit = null;

    public void ShowInfo(BaseUnit target)
    {
        UnitName.text = target.name;
        Int64 v = target.PropertyArray[(int)ePropertyName.MAtk].FinalValue;
        UnitAtkText.text = v + "";
        UnitDefText.text = target.tmpDef + "";
        if (!InfoPanel.gameObject.activeSelf)
        {
            InfoPanel.gameObject.SetActive(true);
        }
        nowShowUnit = target;
    }

    public void HideInfo()
    {
        InfoPanel.gameObject.SetActive(false);
        nowShowUnit = null;
    }

    public void UpdateInfo()
    {
        if(nowShowUnit != null)
        {
            ShowInfo(nowShowUnit);
        }
    }
}
