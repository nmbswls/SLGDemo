using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudCtrl : MonoBehaviour
{

    public GameObject HeadProfilePrefab;

    public Transform ActorContainer;

    public Button AttackBtn;
    public Button NextTurnBtn;
    public Button MoveBtn;
    public Text AttackBtnTxt;

    public Text NowTurnInfo;

    public Transform InfoPanel;
    public Text UnitName;
    public Text UnitAtkText;
    public Text UnitDefText;
    public Text UnitHpText;

    public Button DebugBtn1;
    public Button DebugBtn2;
    public Button DebugBtn3;

    public Button RotLeftBtn;
    public Button RotRightBtn;

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
        UnitHpText = InfoPanel.Find("Hp_value").GetComponent<Text>();

        DebugBtn1 = transform.Find("TestBtn1").GetComponent<Button>();
        DebugBtn2 = transform.Find("TestBtn2").GetComponent<Button>();
        DebugBtn3 = transform.Find("TestBtn3").GetComponent<Button>();

        RotLeftBtn = transform.Find("RotLeft").GetComponent<Button>();
        RotRightBtn = transform.Find("RotRight").GetComponent<Button>();

        ActorContainer = transform.Find("TurnActors");
        for(int i = 0; i < 3; i++)
        {
            GameObject go = GameObject.Instantiate(HeadProfilePrefab, ActorContainer);
        }

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

        DebugBtn1.onClick.AddListener(delegate ()
        {

            BattleManager.Instance.nowTurnActor.AddModifierAtkBig();
            UpdateInfo();
            //BattleManager.Instance.PlayerFinishTurn();

        });

        DebugBtn2.onClick.AddListener(delegate ()
        {
            BattleManager.Instance.nowTurnActor.AddModifierAtkSmall();
            UpdateInfo();

        });
        DebugBtn3.onClick.AddListener(delegate ()
        {
            BattleManager.Instance.nowTurnActor.RemoveAllModifier();
            UpdateInfo();
        });

        RotLeftBtn.onClick.AddListener(delegate ()
        {
            BattleManager.Instance.cameraCtrl.RotPrev45();
        });
        RotRightBtn.onClick.AddListener(delegate ()
        {
            BattleManager.Instance.cameraCtrl.RotNext45();
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
        Int64 v = target.GetFinalProperty((int)ePropertyName.MAtk);
        UnitAtkText.text = v + "";
        UnitDefText.text = target.tmpDef + "";

        UnitHpText.text = target.GetFinalProperty((int)ePropertyName.MaxHp) + "";

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
