﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class UISumReward : UIBase
{
    public static string ViewName = "UISumReward";
    public GameObject sunlight;
    public GameObject moon;
    public GameObject okButton;
    public GameObject tenReward;
    public GameObject onceReward;
    public GameObject skipButton;
    public Text title;
    private UIGainPet mGainPetUI;
    public List<GameObject> rewardImage = new List<GameObject>();
    //-------------------------------------------------------
    public void OpenSumReward(SummonItem summonData, ConsumeType consime, bool isTen = false)
    {
        StartCoroutine(ShowReward(summonData, consime, isTen));
    }
    //-------------------------------------------------------
    void SetEffect(ConsumeType consime, bool skip, bool isTen = false)
    {
        if (consime == ConsumeType.Consume_jinbi || consime == ConsumeType.Consume_Free_jinbi)
            sunlight.SetActive(true);
        else
            moon.SetActive(true);
        if (isTen && !skip)
            okButton.SetActive(false);
        else
            okButton.SetActive(true);
        if (isTen && skip)
            skipButton.SetActive(false);
        else if (!skip)
            skipButton.SetActive(false);
        else if (isTen)
            skipButton.SetActive(true);
    }
    //-------------------------------------------------------
    IEnumerator ShowReward(SummonItem summonData, ConsumeType consime, bool isTen = false)
    {
        SetEffect(consime,false, isTen);
        if (summonData.item.type == (int)PB.itemType.ITEM)//判断奖励
        {
            onceReward.SetActive(true);
            ItemIcon icon = ItemIcon.CreateItemIcon(
                ItemData.valueof(summonData.item.itemId, (int)summonData.item.count));
            icon.transform.SetParent(onceReward.transform,false);
            rewardImage.Add(icon.transform.gameObject);
            if (isTen)
            {
                yield return new WaitForSeconds(2f);
                Close();
            }
        }
        else if (summonData.item.type == (int)PB.itemType.EQUIP)
        {
            EquipData equipData = EquipData.valueof(summonData.item.id, summonData.item.itemId, summonData.item.stage,
                summonData.item.level, BattleConst.invalidMonsterID, null);
            ItemIcon icon = ItemIcon.CreateItemIcon(equipData, true, false);
            icon.transform.SetParent(onceReward.transform);
            rewardImage.Add(icon.transform.gameObject);
            if (isTen)
            {
                yield return new WaitForSeconds(2f);
                Close();
            }
        }
        else if (summonData.item.type == (int)PB.itemType.MONSTER)
        {
            okButton.SetActive(false);
            AddGainMonster(summonData.item.itemId);
        }
    }
    //-------------------------------------------------------
    public void OpenSumReward(ConsumeType consime, bool isTen = false)
    {
        SetEffect(consime,true, isTen);
        Transform rewardParent = null;
        if (isTen)
        {
            rewardParent = onceReward.transform;
            tenReward.SetActive(true);
        }
        else
        {
            rewardParent = tenReward.transform;
            onceReward.SetActive(true);
        }
        for (int i = 0; i < UISummon.Instance.summonList.Count; i++)
        {
            if (UISummon.Instance.summonList[i].item.type == (int)PB.itemType.ITEM)//判断奖励
            {
                ItemIcon icon = ItemIcon.CreateItemIcon(
               ItemData.valueof(UISummon.Instance.summonList[i].item.itemId, (int)UISummon.Instance.summonList[i].item.count));
                icon.transform.SetParent(tenReward.transform, false);
                ItemStaticData itemData = StaticDataMgr.Instance.GetItemData(UISummon.Instance.summonList[i].item.itemId);
                SetEffect(itemData.grade, icon.transform.gameObject);
                rewardImage.Add(icon.transform.gameObject);
            }
            else if (UISummon.Instance.summonList[i].item.type == (int)PB.itemType.MONSTER)
            {
                PB.HSMonster monster = UISummon.Instance.summonList[i].item.monster;
                MonsterIcon icon = MonsterIcon.CreateIcon();
                icon.transform.SetParent(tenReward.transform, false);
                icon.SetMonsterStaticId(monster.cfgId);
                icon.SetLevel(monster.level);
                icon.SetStage(monster.stage);
                icon.ShowXiyouduImage();
                SetEffect((StaticDataMgr.Instance.GetUnitRowData(monster.cfgId).rarity + 1), icon.transform.gameObject);
                rewardImage.Add(icon.transform.gameObject);
            }
            else if (UISummon.Instance.summonList[i].item.type == (int)PB.itemType.EQUIP)
            {
                EquipData equipData = EquipData.valueof(UISummon.Instance.summonList[i].item.id,
                    UISummon.Instance.summonList[i].item.itemId, UISummon.Instance.summonList[i].item.stage,
                    UISummon.Instance.summonList[i].item.level, BattleConst.invalidMonsterID, null);
                ItemIcon icon = ItemIcon.CreateItemIcon(equipData, true, false);
                icon.transform.SetParent(tenReward.transform);
                SetEffect(UISummon.Instance.summonList[i].item.stage, icon.transform.gameObject);
                rewardImage.Add(icon.transform.gameObject);
            }
        }
    }
    //-------------------------------------------------------
    void SetEffect(int stage,GameObject go)
    {
        if (stage > 3)
        {
            GameObject effect = ResourceMgr.Instance.LoadAsset("tubiao_effect");
            effect.transform.parent = go.transform;
            effect.transform.localScale = go.transform.localScale;
        }
    }
    //-------------------------------------------------------
    private void AddGainMonster(string monsterID)
    {
        mGainPetUI = UIMgr.Instance.OpenUI_(UIGainPet.ViewName) as UIGainPet;
        mGainPetUI.transform.SetParent(transform, false);
        mGainPetUI.ShowGainPet(monsterID);
        mGainPetUI.SetConfirmCallback(ConfirmGainPet);
    }
    //-------------------------------------------------------
    private void ConfirmGainPet(GameObject go)
    {
        UIMgr.Instance.DestroyUI(mGainPetUI);
        Close();
    }
    //-------------------------------------------------------
    void okClick(GameObject go)
    {
        Close();
    }
    //-------------------------------------------------------
    void SkipClick(GameObject go)
    { 
        
    }
    //-------------------------------------------------------
    public void Close()
    {
        onceReward.SetActive(false);
        sunlight.SetActive(false);
        tenReward.SetActive(false);
        moon.SetActive(false);
        for (int i = 0; i < rewardImage.Count; i++)
        {
            Destroy(rewardImage[i]);
        }
        rewardImage.Clear();
        UIMgr.Instance.CloseUI_(UISumReward.ViewName);        
        UISummon.Instance.endTime = Time.time;
        UISummon.Instance.summonNum++;
        if (UISummon.Instance.summonList.Count > 1)
        {
            if (UISummon.Instance.summonNum == UISummon.Instance.summonList.Count)
                UISummon.Instance.showUI(true);
        }
        else
        {
            UISummon.Instance.showUI(true);
        }
    }
	//-------------------------------------------------------
	void Start () {
        EventTriggerListener.Get(okButton).onClick = okClick;
        EventTriggerListener.Get(skipButton).onClick = SkipClick;
        title.text = StaticDataMgr.Instance.GetTextByID("summon_result");
	}

    public override void Clean()
    {
        UIMgr.Instance.DestroyUI(mGainPetUI);
    }
}
