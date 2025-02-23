﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIUtil  
{

	public static Vector3 GetSpacePos(RectTransform rect, Canvas canvas, Camera camera)
	{
		if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
		{
			return rect.position;
		}
		return camera.WorldToScreenPoint(rect.position);
		
	}

	public static void GetSpaceCorners(RectTransform rect, Canvas canvas, Vector3[] corners,Camera camera)
	{
		if (camera == null)
		{
			camera = Camera.main;
		}
		rect.GetWorldCorners(corners);
		if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
		{
			
		}
		else
		{
			for (var i = 0; i < corners.Length; i++)
			{
				corners[i] = camera.WorldToScreenPoint(corners[i]);
			}
		}
	}

	public static Rect GetSpaceRect(Canvas canvas, RectTransform rect, Camera camera)
	{
		Rect spaceRect = rect.rect;
		Vector3 spacePos = GetSpacePos(rect, canvas, camera);
		//lossyScale
		spaceRect.x = spaceRect.x * rect.lossyScale.x + spacePos.x;
		spaceRect.y = spaceRect.y * rect.lossyScale.y + spacePos.y;
		spaceRect.width = spaceRect.width * rect.lossyScale.x;
		spaceRect.height = spaceRect.height * rect.lossyScale.y;
		return spaceRect;
	}
	
	public static bool RectContainsScreenPoint(Vector3 point, Canvas canvas, RectTransform rect, Camera camera)
	{
		if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
		{
			return RectTransformUtility.RectangleContainsScreenPoint(rect, point, camera);
		}
		
		return GetSpaceRect(canvas, rect, camera).Contains(point);
	}

    //装换属性名称
    public static string ConvertProperty(int property)
    {
        if (property == SpellConst.propertyGold)
            return "金";
        else if (property == SpellConst.propertyWood)
            return "木";
        else if (property == SpellConst.propertyWater)
            return "水";
        else if (property == SpellConst.propertyFire)
            return "火";
        else if (property == SpellConst.propertyEarth)
            return "土";

        return "无";
    }

    public static bool NeedChangeGrade(int stage)
    {
        if (stage == 0 || stage == 2 || stage == 5 || stage == 9 || stage == 14)
        {
            return true;
        }

        return false;
    }

    public static void CalculationQuality(int stage, out int quality, out int plusQuality)
    {
        quality = 1;
        plusQuality = 0;

        if (stage < 0 || stage > 15)
            return;

        if (stage == 0)
        {
            quality = 1;
        }
        else if (stage < 3)
        {
            quality = 2;
            plusQuality = stage - 1;
        }
        else if (stage < 6)
        {
            quality = 3;
            plusQuality = stage - 3;
        }
        else if (stage < 10)
        {
            quality = 4;
            plusQuality = stage - 6;
        }
        else if (stage < 15)
        {
            quality = 5;
            plusQuality = stage - 10;
        }
        else
        {
            quality = 6;
        }
    }

    public static bool CheckIsEnoughMaterial(GameUnit unit)
    {
        if (unit.pbUnit.stage == GameConfig.MaxMonsterStage)
        {
            return false;
        }

        if (CheckIsEnoughLevel(unit) == false)
        {
            return false;
        }

        UnitStageData unitStageData = StaticDataMgr.Instance.getUnitStageData(unit.pbUnit.stage + 1);

        ItemData itemData = null;

        for (int i = 0; i < unitStageData.demandItemList.Count; i++)
        {
            itemData = GameDataMgr.Instance.PlayerDataAttr.gameItemData.getItem(unitStageData.demandItemList[i].itemId);
            if (itemData == null || itemData.count < unitStageData.demandItemList[i].count)
            {
                return false;
            }
        }

        //金钱判断
        if (GameDataMgr.Instance.PlayerDataAttr.coin <= unitStageData.demandCoin)
        {
            return false;
        }


        if (NeedChangeGrade(unit.pbUnit.stage) == false)
        {
            return true;
        }

        ItemInfo itemInfo = null;
        List<GameUnit> petList = new List<GameUnit>();

        for (int i = 0; i < unitStageData.demandMonsterList.Count; i++)
        {
            itemInfo = unitStageData.demandMonsterList[i];
            GameDataMgr.Instance.PlayerDataAttr.GetAllPet(
                (itemInfo.itemId.Equals(BattleConst.stageSelfId)?unit.pbUnit.id:itemInfo.itemId),
                itemInfo.stage,
                ref petList
                );

            petList.Remove(unit);
            if (petList.Count < unitStageData.demandMonsterList[i].count)
            {
                return false;
            }
        }
        return true;
    }

    public static bool CheckIsEnoughLevel(GameUnit unit)
    {
        if (unit.pbUnit.stage == GameConfig.MaxMonsterStage)
        {
            return true;
        }

        UnitStageData unitStageData = StaticDataMgr.Instance.getUnitStageData(unit.pbUnit.stage + 1);
        return unit.pbUnit.level >= unitStageData.demandLevel;
    }

    public static bool CheckIsEnoughMaterial(List<ItemInfo> itemInfos)
    {
        ItemData mineItem = null;
        for (int i = 0; i < itemInfos.Count; i++)
        {
            if (itemInfos[i].type == (int)PB.itemType.ITEM)
            {
                mineItem = GameDataMgr.Instance.PlayerDataAttr.gameItemData.getItem(itemInfos[i].itemId);
                if (mineItem==null||itemInfos[i].count>mineItem.count)
                {
                    return false;
                }
            }
            else if (itemInfos[i].type == (int)PB.itemType.PLAYER_ATTR)
            {
                if (itemInfos[i].itemId.Equals(((int)PB.changeType.CHANGE_COIN).ToString()))
                {
                    if (itemInfos[i].count>GameDataMgr.Instance.PlayerDataAttr.coin)
                    {
                        return false;
                    }
                }
                else
                {
                    Logger.LogError("xiao hao jin bi pei zhi cuo wu !!!!!!!!!!!!!!!!!!");
                    return false;
                }
            }
            else
            {
                Logger.LogError("xiao hao wu pin pei zhi cuo wu!!!!!!!!!!");
                return false;
            }
        }
        return true;
    }

    public static bool CheckIsEnoughPlayerLevel(int level)
    {
        if (level<=GameDataMgr.Instance.PlayerDataAttr.LevelAttr)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool CheckIsEnoughEquip(GameUnit unit, PartType part)
    {
        UnitData petData = StaticDataMgr.Instance.GetUnitRowData(unit.pbUnit.id);
        return GameDataMgr.Instance.PlayerDataAttr.CheckEquipTypePart(petData.equip, (int)part);
    }

    public static bool CheckIsEnoughEquipLevelUP(EquipData data)
    {
        EquipData curData = data;
        EquipData nextData = null;
        if (curData.level >= BattleConst.maxEquipLevel && curData.stage >= BattleConst.maxEquipStage)
        {
            return false;
        }
        else if (curData.level < BattleConst.maxEquipLevel)
        {
            nextData = EquipData.valueof(curData.id, curData.equipId, curData.stage, curData.level + 1, BattleConst.invalidMonsterID, null);
        }
        else if (curData.stage < BattleConst.maxEquipStage)
        {
            nextData = EquipData.valueof(curData.id, curData.equipId, curData.stage + 1, BattleConst.minEquipLevel, BattleConst.invalidMonsterID, null);
        }
        EquipForgeData nextForge = StaticDataMgr.Instance.GetEquipForgeData(nextData.stage, nextData.level);
        if (!CheckIsEnoughPlayerLevel(nextForge.playerlevelDemand))
        {
            return false;
        }
        List<ItemInfo> curDemand = new List<ItemInfo>();
        nextForge.GetLevelDemand(ref curDemand);
        return UIUtil.CheckIsEnoughMaterial(curDemand);
    }

    public static int CheckPetIsMaxLevel(int level)
    {
        //版本最大等级
        if (level>=GameConfig.MaxMonsterLevel)
        {
            return 1;
        }

        //达到玩家等级
        if (level >= GameDataMgr.Instance.PlayerDataAttr.LevelAttr)
        {
            return 2;
        }

        return 0;
    }

    public static bool CheckHaveNewMail()
    {
        Dictionary<int, PB.HSMail> mailDict = GameDataMgr.Instance.PlayerDataAttr.gameMailData.mailList;
        foreach (var item in mailDict)
        {
            if (item.Value.state==(int)PB.mailState.UNREAD)
            {
                return true;
            }
        }
        return false;
    }


    public static bool CheckIsComposeOpened()
    {
        return true;
    }
    public static bool CheckIsDecomposeOpened()
    {
        return true;
    }

    public static bool CheckIsStoryQuestOpened()
    {
        if (GameDataMgr.Instance.PlayerDataAttr.LevelAttr >= BattleConst.storyQuest)
        {
            return true;
        }
        return false;
    }
    public static bool CheckIsDailyQuestOpened()
    {
        if (GameDataMgr.Instance.PlayerDataAttr.LevelAttr >= BattleConst.dailyQuest)
        {
            return true;
        }
        return false;
    }
    public static string GetBaoshidituByType(int type)
    {
        switch (type)
        {
            case 1:
                return "baoshiditu_BloodStone";
            case 2:
                return "baoshiditu_SageStone";
            case 3:
                return "baoshiditu_VitalityStone";
            case 4:
                return "baoshiditu_BastionStone";
            case 5:
                return "baoshiditu_HasteStone";
            case 6:
                return "baoshiditu_MendingStone";
            case 7:
                return "baoshiditu_VolatileStone";
            case 8:
                return "baoshiditu_SourceStone";
            case 9:
                return "baoshiditu_MorbidStone";
            default:
                return "other";
        }
    }

    public static string Convert_hh_mm_ss(int time)
    {
        return string.Format("{0:D2}:{1:D2}:{2:D2}", time / 3600, (time % 3600) / 60, time % 60);
    }
    public static void SetStageColor(Text label, GameUnit unit)
    {
        int quallity = 0;
        int plusQuality = 0;
        UIUtil.CalculationQuality(unit.pbUnit.stage, out quallity, out plusQuality);

        label.color = ColorConst.GetStageTextColor(quallity);
        Outline outline = label.GetComponent<Outline>();
        if (outline == null)
        {
            outline = label.gameObject.AddComponent<Outline>();
        }
        outline.effectColor = ColorConst.GetStageOutLineColor(quallity);

        if (plusQuality == 0)
        {
            label.text = unit.name;
        }
        else
        {
            label.text = string.Format("{0} +{1}", StaticDataMgr.Instance.GetUnitRowData(unit.pbUnit.id).NickNameAttr, plusQuality);
        }

    }

    public static void SetStageName(Text label, GameUnit unit)
    {
        int quallity = 0;
        int plusQuality = 0;
        UIUtil.CalculationQuality(unit.pbUnit.stage, out quallity, out plusQuality);
        
        if (plusQuality == 0)
        {
            label.text = unit.name;
        }
        else
        {
            label.text = string.Format("{0} +{1}", StaticDataMgr.Instance.GetUnitRowData(unit.pbUnit.id).NickNameAttr, plusQuality);
        }

    }
    public static void SetStageColor(Text label, ItemStaticData itemData)
    {
        label.color = ColorConst.GetStageTextColor(itemData.grade);
        Outline outline = label.GetComponent<Outline>();
        if (outline == null)
        {
            outline = label.gameObject.AddComponent<Outline>();
        }
        outline.effectColor = ColorConst.GetStageOutLineColor(itemData.grade);
        label.text = itemData.NameAttr;
    }

    public static void SetStageColor(Text label, string txtMsg, int stage, int level = 0)
    {
        label.color = ColorConst.GetStageTextColor(stage);

        Outline outline = label.GetComponent<Outline>();
        if (outline==null)
        {
            outline = label.gameObject.AddComponent<Outline>();
        }
        outline.effectColor = ColorConst.GetStageOutLineColor(stage);

        if (level == 0)
        {
            label.text = txtMsg;
        }
        else
        {
            label.text = string.Format("{0} +{1}", txtMsg, level);
        }

    }

    public static void SetEquipType(Text label, int type)
    {
        label.text = UnitData.GetEquipAttr(type);
    }
    public static void SetEquipPart(Text label, int part)
    {
        label.text = ItemStaticData.GetEquipPart(part);
    }

    public static void GetAttrValue(GameUnit unit, int stage, out int health, out int strength, out int inteligence, out int defence, out int speed)
    {
        //int grade = StaticDataMgr.Instance.GetUnitRowData(unit.pbUnit.id).grade;
        UnitStageData unitStageData = StaticDataMgr.Instance.getUnitStageData(stage);
        UnitData unitData = StaticDataMgr.Instance.GetUnitRowData(unit.pbUnit.id);
        UnitBaseData unitBase = StaticDataMgr.Instance.GetUnitBaseRowData(unit.pbUnit.level);

        health = (int)((1 + unitStageData.modifyRate) * unitData.healthModifyRate * unitBase.health + unitStageData.health * unitData.healthModifyRate);
        strength = (int)((1 + unitStageData.modifyRate) * unitData.strengthModifyRate * unitBase.strength + unitStageData.strength * unitData.strengthModifyRate);
        inteligence = (int)((1 + unitStageData.modifyRate) * unitData.intelligenceModifyRate * unitBase.intelligence + unitStageData.intelligence * unitData.intelligenceModifyRate);
        defence = (int)((1 + unitStageData.modifyRate) * unitData.defenseModifyRate * unitBase.defense + unitStageData.defense * unitData.defenseModifyRate);
        speed = (int)((1 + unitStageData.modifyRate) * unitData.speedModifyRate * unitBase.speed + unitStageData.speed * unitData.speedModifyRate);
    }

    public static void SetParentReset(Transform child, Transform parent)
    {
        child.SetParent(parent, false);
        child.localPosition = Vector3.zero;
        child.localScale = Vector3.one;
    }
    public static void SetParentReset(Transform child, Transform parent,Vector3 localPos)
    {
        child.SetParent(parent, false);
        child.localPosition = localPos;
        child.localScale = Vector3.one;
    }

    public  static  void    SetButtonTitle(Transform btn,string text)
    {
        Text tx = btn.GetComponentInChildren<Text>();
        if(tx != null)
        {
            tx.text = text;
        }
    }

    public static void SetDisPlayAttr(EquipLevelData data, Text attr1, Text value1, Text attr2, Text value2)
    {
        attr2.text = "";
        value2.text = "";
        if (data==null)
        {
            return;
        }
        int index = 0;
        if (data.strength!=0)
        {
            attr1.text = StaticDataMgr.Instance.GetTextByID("common_attr_strenth");
            value1.text = "+" + data.strength.ToString();
            index++;
        }
        if (data.intelligence != 0)
        {
            if (index == 0)
            {
                attr1.text = StaticDataMgr.Instance.GetTextByID("common_attr_intelligence");
                value1.text = "+" + data.intelligence.ToString();
            }
            else
            {
                attr2.text = StaticDataMgr.Instance.GetTextByID("common_attr_intelligence");
                value2.text = "+" + data.intelligence.ToString();
            }
            index++;
        }
        if (data.speed != 0)
        {
            if (index == 0)
            {
                attr1.text = StaticDataMgr.Instance.GetTextByID("common_attr_speed");
                value1.text = "+" + data.speed.ToString();
            }
            else
            {
                attr2.text = StaticDataMgr.Instance.GetTextByID("common_attr_speed");
                value2.text = "+" + data.speed.ToString();
            }
            index++;
        }
        if (data.defense != 0)
        {
            if (index == 0)
            {
                attr1.text = StaticDataMgr.Instance.GetTextByID("common_attr_defence");
                value1.text = "+" + data.defense.ToString();
            }
            else
            {
                attr2.text = StaticDataMgr.Instance.GetTextByID("common_attr_defence");
                value2.text = "+" + data.defense.ToString();
            }
            index++;
        }
        if (data.health != 0)
        {
            if (index == 0)
            {
                attr1.text = StaticDataMgr.Instance.GetTextByID("common_attr_health");
                value1.text = "+" + data.health.ToString();
            }
            else
            {
                attr2.text = StaticDataMgr.Instance.GetTextByID("common_attr_health");
                value2.text = "+" + data.health.ToString();
            }
            index++;
        }
        if (data.energy != 0)
        {
            if (index == 0)
            {
                attr1.text = StaticDataMgr.Instance.GetTextByID("common_attr_energy");
                value1.text = "+" + data.energy.ToString();
            }
            else
            {
                attr2.text = StaticDataMgr.Instance.GetTextByID("common_attr_energy");
                value2.text = "+" + data.energy.ToString();
            }
            index++;
        }
        if (data.criticalRatio != 0)
        {
            if (index == 0)
            {
                attr1.text = StaticDataMgr.Instance.GetTextByID("common_attr_critical_ratio");
                value1.text = "+" + data.criticalRatio.ToString("0%");
            }
            else
            {
                attr2.text = StaticDataMgr.Instance.GetTextByID("common_attr_critical_ratio");
                value2.text = "+" + data.criticalRatio.ToString("0%");
            }
            index++;
        }
        if (data.criticalDmg != 0)
        {
            if (index == 0)
            {
                attr1.text = StaticDataMgr.Instance.GetTextByID("common_attr_critical_dmg");
                value1.text = "+" + data.criticalDmg.ToString("0%");
            }
            else
            {
                attr2.text = StaticDataMgr.Instance.GetTextByID("common_attr_critical_dmg");
                value2.text = "+" + data.criticalDmg.ToString("0%");
            }
            index++;
        }
        if (data.healRatio != 0)
        {
            if (index == 0)
            {
                attr1.text = StaticDataMgr.Instance.GetTextByID("common_attr_heal_ratio");
                value1.text = "+" + data.healRatio.ToString("0%");
            }
            else
            {
                attr2.text = StaticDataMgr.Instance.GetTextByID("common_attr_heal_ratio");
                value2.text = "+" + data.healRatio.ToString("0%");
            }
            index++;
        }

        switch (index)
        {
            case 0:
                attr1.gameObject.SetActive(false);
                value1.gameObject.SetActive(false);
                attr2.gameObject.SetActive(false);
                value2.gameObject.SetActive(false);
                break;
            case 1:
                attr1.gameObject.SetActive(true);
                value1.gameObject.SetActive(true);
                attr2.gameObject.SetActive(false);
                value2.gameObject.SetActive(false);
                break;
            case 2:
                attr1.gameObject.SetActive(true);
                value1.gameObject.SetActive(true);
                attr2.gameObject.SetActive(true);
                value2.gameObject.SetActive(true);
                break;
        }

    }

    public static void SubstringByText(Text text, string content)
    {
        text.text = content;
        RectTransform textRt = text.transform as RectTransform;
        if (text.preferredWidth > textRt.rect.width)
        {
            string temp = content;
            while (text.preferredWidth > textRt.rect.width)
            {
                temp = temp.Substring(0, temp.Length - 1);
                text.text = temp + "…";
            }
        }
    }

    public static string GetRareImg(int rarity)
    {
        string assetname = "";
        switch (rarity)
        {
            case 0:
                assetname = "c";
                break;
            case 1:
                assetname = "b";
                break;
            case 2:
                assetname = "a";
                break;
            case 3:
                assetname = "s";
                break;
            case 4:
                assetname = "ss";
                break;
            case 5:
                assetname = "sss";
                break;
            default:
                assetname = "c";
                break;
        }
        assetname = "chongwu_xiyoudu_" + assetname;

        return assetname;
    }

}
