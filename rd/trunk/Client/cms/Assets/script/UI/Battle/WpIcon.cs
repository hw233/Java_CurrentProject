﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WpIcon : MonoBehaviour
{
    //正常状态类别
    public enum NormalStateType
    {
        BeneficialForSelf = 0,//对本体有加成
        HarmfulForEnemy,//对地方有压制
        ProtectedWpForSelf,//保护弱点
        HightInjuryRatio,//受伤比高
        LowInjuryRatio,//受伤比低
        NormalInjuryRatio//受伤比正常
    }
    public Image iconImage;
    public Image deadMask;
    public UIProgressbar progressBar;
    public Image buttonImage;
    public GameObject bloodBrokenObject;
    private ShakeUi shakeUi;
    private bool isArmor = false;//是否是铠甲
    private WeakPointRuntimeData wpRealData;

    // Use this for initialization
    void Start ()
    {
        bloodBrokenObject.SetActive(false);
        EventTriggerListener.Get(buttonImage.gameObject).onEnter = OnTouchEnter;
        EventTriggerListener.Get(buttonImage.gameObject).onExit = OnTouchExit;
        shakeUi = progressBar.GetComponent<ShakeUi>();
    }

    public  string GetWpId()
    {
        if(null != wpRealData)
        {
            return wpRealData.id;
        }
        return null;
    }

    public  void    RefreshWithWp(WeakPointRuntimeData wpRealData)
    {
        if (bloodBrokenObject != null)
        {
            bloodBrokenObject.SetActive(false);
        }
        this.wpRealData = wpRealData;
        iconImage.transform.localScale = new Vector3(wpRealData.staticData.scaleX, wpRealData.staticData.scaleY, 1);
       // progressBar.gameObject.SetActive(false);
        deadMask.gameObject.SetActive(false);

        isArmor = false;
        string wpIconname = null;
        switch (wpRealData.wpState)
        {
            case WeakpointState.Ice:
                return;
            case WeakpointState.Dead:
                deadMask.gameObject.SetActive(true);
                if(progressBar.gameObject.activeInHierarchy)
                {
                    //破碎
                    if(bloodBrokenObject != null)
                    {
                        bloodBrokenObject.SetActive(true);
                    }
                }
                break;
            case WeakpointState.Hide:
                wpIconname = wpRealData.staticData.state0Icon;
                break;
            case WeakpointState.Normal1:
                //wpIconname = GetWpIconName(wpRealData.staticData.stat1WpType);
                wpIconname = wpRealData.staticData.state1Icon;
                isArmor = wpRealData.staticData.stat1WpType >= 0 && wpRealData.staticData.stat1WpType <= 2;
                break;
            case WeakpointState.Normal2:
                //  wpIconname = GetWpIconName(wpRealData.staticData.state2WpType);
                wpIconname = wpRealData.staticData.state2Icon;
                isArmor = wpRealData.staticData.state2WpType >= 0 && wpRealData.staticData.state2WpType <= 2;
                break;
        }
        Sprite iconSp = null;
        if(!string.IsNullOrEmpty(wpIconname))
        {
            iconSp = ResourceMgr.Instance.LoadAssetType<Sprite>(wpIconname);
        }
        if(null != iconSp)
        {
            iconImage.sprite = iconSp;
        }

        progressBar.gameObject.SetActive(isArmor);
        if(isArmor)
        {
            RefreshProgress(false);
        }
    }

    string GetWpIconName(int type)
    {
        return string.Format("ruodianIcon_{0}", (int)type);
    }

    public void RefreshProgress(bool shake = true)
    {
        float ratio = (float)wpRealData.HpAttr / (float)wpRealData.maxHp;
        progressBar.SetTargetRatio(ratio);
        if(shake && null != shakeUi)
        {
            shakeUi.SetShake();
        }
    }

    public  void    ShowoffIcon(bool showoff)
    {
        float scaleX = wpRealData.staticData.scaleX;
        float scaleY = wpRealData.staticData.scaleY;
        if(showoff)
        {
            iconImage.transform.localScale = new Vector3(1.2f*scaleX , 1.2f * scaleY, 1.2f);
        }
        else
        {
            iconImage.transform.localScale = new Vector3(1.0f*scaleX, 1.0f*scaleY, 1.0f);
        }
    }

    void OnTouchEnter(GameObject go)
    {
        string tips = null;
        bool wpShowOff = false;
        switch(wpRealData.wpState)
        {
            case WeakpointState.Hide:
                tips = wpRealData.staticData.state0Tips;
                break;
            case WeakpointState.Normal1:
                tips = wpRealData.staticData.state1Tips;
                wpShowOff = true;
                break;
            case WeakpointState.Normal2:
                tips = wpRealData.staticData.state2Tips;
                wpShowOff = true;
                break;
            default:
                return;
        }

        string tipsmsg = "";
        if(!string.IsNullOrEmpty(tips))
        {
            tipsmsg = StaticDataMgr.Instance.GetTextByID(tips);
        }

        string wpName = null;
        if(wpRealData.wpState != WeakpointState.Hide)
        {
            wpName = StaticDataMgr.Instance.GetTextByID(wpRealData.staticData.name);
        }

        ShowoffIcon(true);
        if (!string.IsNullOrEmpty(tipsmsg) || !string.IsNullOrEmpty(wpName))
        {
            BattleController.Instance.GetUIBattle().wpUI.ShowTips(this, tipsmsg, wpName);
        }

        if (wpShowOff)
        {
            wpRealData.showoffEffect.Show(true);
        }

    }

    void OnTouchExit(GameObject go)
    {
        ShowoffIcon(false);
        BattleController.Instance.GetUIBattle().wpUI.HideTips();
        wpRealData.showoffEffect.Show(false);
        //test 3fat
       //BattleController.Instance.GetUIBattle().wpUI.ChangeBatch(2.0f);
    }

    
}
