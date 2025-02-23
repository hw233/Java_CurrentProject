﻿using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class SpellUpLevelPrice
{
    public int level;
    public int coin;
}

[Serializable]
public class SpellProtoType
{
    public string id;
    public string rootEffectID;
    public int actionCount;
    public int channelTime;
    public int energyCost;
    public int energyGenerate;
    public int category;
    public float levelAdjust;
    //public int cdTime;
    //显示用数据
    public string icon;
	public string name;
    public string tips;
    public float baseTipValue;
    public int isAoe;
    public string firstSpell;   
}

public class Spell
{
    public int casterID;
    public int targetID;
    public SpellProtoType spellData;
    public SpellService spellService;
    public int level;
    bool needAddDazhaoDelay = true;

    private float spellLength;

    public Spell(SpellProtoType spellPt, int level)
    {
        spellData = spellPt;
        this.level = level;
    }

    public void Init(SpellService owner)
    {
        spellLength = 0.0f;
        spellService = owner;
        needAddDazhaoDelay = false;
    }

    public void Apply(float triggerTime, string wpID, bool isFirstSpell)
    {
        //generate spell event
        SpellFireArgs args = new SpellFireArgs();
        args.triggerTime = triggerTime;
        args.spellID = spellData.id;
        args.casterID = casterID;
        args.targetID = targetID;
        GameUnit caster = spellService.GetUnit(casterID);
        //args.castResult = SpellConst.spellCastOK;

        //generate energy
        if (spellData.energyGenerate > 0)
        {
            caster.energy += spellData.energyGenerate;
            if (caster.energy > BattleConst.enegyMax)
				caster.energy = BattleConst.enegyMax;

            SpellVitalChangeArgs energyArgs = new SpellVitalChangeArgs();
            energyArgs.vitalType = (int)VitalType.Vital_Type_Default;
            energyArgs.triggerTime = triggerTime;
            energyArgs.casterID = casterID;
            energyArgs.vitalChange = spellData.energyGenerate;
            energyArgs.vitalCurrent = caster.energy;
            energyArgs.vitalMax = 0;
            spellService.TriggerEvent(GameEventList.SpellEnergyChange, energyArgs);
        }

        //take energy if needed
        if (caster.energy < spellData.energyCost)
        {
            Logger.LogWarning("Energy not enough!");
            return;
        }
        if (spellData.energyCost > 0)
        {
            SpellVitalChangeArgs energyArgs = new SpellVitalChangeArgs();
            energyArgs.triggerTime = triggerTime;
            energyArgs.casterID = casterID;
            energyArgs.vitalChange = spellData.energyCost * -1;//minus
            caster.energy = 0;// caster.energy - spellData.energyCost;
            if (caster.energy <= 0)
            {
                caster.energy = 0;
            }
            energyArgs.vitalCurrent = caster.energy;
            energyArgs.vitalMax = 0;
            spellService.TriggerEvent(GameEventList.SpellEnergyChange, energyArgs);
        }

        Effect rootEffect = spellService.GetEffect(spellData.rootEffectID);
        if (rootEffect != null)
        {
            float dazhaoDelay = 0.0f;
            if (spellData.category == (int)SpellType.Spell_Type_MagicDazhao && caster.battleUnit.camp == UnitCamp.Enemy)
            {
                dazhaoDelay = BattleConst.magicDazhaoDelay;
            }
            rootEffect.SetOwnedSpell(this);
            rootEffect.Apply(triggerTime + dazhaoDelay, wpID);
            spellLength = spellLength - triggerTime;
            if (needAddDazhaoDelay)
            {
                spellLength += dazhaoDelay;
            }
        }

        args.aniTime = spellLength;
        //args.firstSpell = (spellData.category == (int)SpellType.Spell_Type_Passive) ? spellData.firstSpell : null;
        args.firstSpell = isFirstSpell;
        args.category = spellData.category;
        //if (caster.pbUnit.camp == UnitCamp.Enemy || spellData.category != (int)SpellType.Spell_Type_MagicDazhao)
        {
            spellService.TriggerEvent(GameEventList.SpellFire, args);
        }
    }

    public void SetSpellEndTime(float delayTime)
    {
        spellLength = delayTime;
        needAddDazhaoDelay = false;
    }
}
