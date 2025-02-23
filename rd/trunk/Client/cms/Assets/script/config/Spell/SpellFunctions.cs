﻿using UnityEngine;
using System.Collections;

public class SpellFunctions  
{
    public static float GetInjuryAdjustNum(int casterLvl, int targetLvl)
    {
        return (casterLvl * 10 + 1000);
    }

    public static float GetHitRatio(int casterLvl, int targetLvl)
    {
        return 0.0f;
    }

    public static float GetBpLvlAdjust(int lvl)
    {
        return 0.000001f;
    }

    public static float GetPropertyDamageRatio(int casterProp, int targetProp, ref bool isKezhi)
    {
        //propertyGold->guang propertyEarth->an
        isKezhi = false;
        float damageRatio = 1.0f;
        if (casterProp == SpellConst.propertyGold)
        {
            if (targetProp == SpellConst.propertyEarth)
            {
                isKezhi = true;
                return SpellConst.propertyEnhance;
            }
        }
        else if (casterProp == SpellConst.propertyWood)
        {
            if (targetProp == SpellConst.propertyWater)
            {
                isKezhi = true;
                return SpellConst.propertyEnhance;
            }
            else if (targetProp == SpellConst.propertyFire)
            {
                return SpellConst.propertyWeaken;
            }
        }
        else if (casterProp == SpellConst.propertyWater)
        {
            if (targetProp == SpellConst.propertyFire)
            {
                isKezhi = true;
                return SpellConst.propertyEnhance;
            }
            else if (targetProp == SpellConst.propertyWood)
            {
                return SpellConst.propertyWeaken;
            }
        }
        else if (casterProp == SpellConst.propertyFire)
        {
            if (targetProp == SpellConst.propertyWood)
            {
                isKezhi = true;
                return SpellConst.propertyEnhance;
            }
            else if (targetProp == SpellConst.propertyWater)
            {
                return SpellConst.propertyWeaken;
            }
        }
        else if (casterProp == SpellConst.propertyEarth)
        {
            if (targetProp == SpellConst.propertyGold)
            {
                isKezhi = true;
                return SpellConst.propertyEnhance;
            }
        }
        return damageRatio;
    }

    public static int IsEnemy(GameUnit caster, GameUnit target)
    {
        //return 1;
        if (caster != null)
        {
            return (caster.pbUnit.camp == UnitCamp.Enemy) ? 1 : 0;
        }

        return 1;
    }

    public static int ContainsBeneficialBuff(GameUnit caster, GameUnit target)
    {
        int count = caster.buffList.Count;
        for (int i = 0; i < count; ++i)
        {
			if (caster.buffList[i].buffProto.category == 5)
				return 1 ;
        }
		return 0;
    }
	
	
	
	
	
	

    public static int Default(GameUnit caster, GameUnit target)
    {
        return 1;
    }

    // buff response validators
    public static int BuffValidatorSample(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        )
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
        //血量低于xx触发示例
        if (damageEffect != null)
        {
            GameUnit target = spellService.GetUnit(triggerEffect.targetID);
            float lifeRatio = target.curLife / (float)target.maxLife;
            if (lifeRatio < 0.05f)
                return 1;
        }
        //受到某个类型伤害触发示例
        if (damageEffect != null && damageEffect.targetID == triggerBuff.targetID)
        {
            EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
            if (damageProto.isHeal == false && damageProto.damageType == SpellConst.damagePhy)//SpellConst.damageMagic)
            {
                return 1;
            }
        }
        //造成某个类型伤害触发示例
        if (damageEffect != null && damageEffect.casterID == triggerBuff.targetID)
        {
            EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
            //造成某类伤害示例
            if (damageProto.isHeal == false && damageProto.damageType == SpellConst.damagePhy)//SpellConst.damageMagic)
            {
                return 1;
            }
        }
        //使用某类技能造成伤害触发示例
        if (triggerEffect != null && triggerEffect.casterID == triggerBuff.targetID)
        {
            Spell ownedSpell = triggerEffect.ownedSpell;
            if (ownedSpell != null && ownedSpell.spellData.category == (int)SpellType.Spell_Type_MgicAttack)
            {
                return 1;
            }
        }

        //触发几率示例
        float randNum = Random.Range(0.0f, 1.0f);
        if (randNum > 0.5f)
        {
            return 1;
        }

        return 0;
    }
	//无条件
	public static int uncondition(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        )
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
		
		return 1;
	}
    //使用物理攻击(未包含大招)
    public static int UsingPhysicalRate_40(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        )
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;

        //使用某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
        if (triggerEffect != null && triggerEffect.casterID == triggerBuff.targetID)
        {
            Spell ownedSpell = triggerEffect.ownedSpell;
            if (ownedSpell != null && ownedSpell.spellData.category == (int)SpellType.Spell_Type_PhyAttack && randNum <= 0.4f)
            {
                return 1;
            }
        }

        return 0;
    }
    public static int UsingPhysicalRate_35(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        ) 
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;

        //使用某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
		if (damageEffect != null && damageEffect.casterID == triggerBuff.targetID)
		{
			EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
			if (damageProto.isHeal == false)
			{
				Spell ownedSpell = triggerEffect.ownedSpell;
				if (ownedSpell != null && ownedSpell.spellData.category == (int)SpellType.Spell_Type_PhyAttack && randNum <= 0.35f)
				{
					return 1;
				}
			}				
		}	
		return 0;
    } 
	public static int UsingPhysicalRate_100(
		Buff triggerBuff,
		Effect triggerEffect,
		SpellService spellService
		)
	{
		EffectDamage damageEffect = triggerEffect as EffectDamage;
		EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
		
		//使用某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
		if (triggerEffect != null && triggerEffect.casterID == triggerBuff.targetID)
		{
			Spell ownedSpell = triggerEffect.ownedSpell;
			if (ownedSpell != null && ownedSpell.spellData.category == (int)SpellType.Spell_Type_PhyAttack && randNum <= 1.0f)
			{
				return 1;
			}
		}
		
		return 0;
	}
	public static int UsingPhysicalRate_15(
		Buff triggerBuff,
		Effect triggerEffect,
		SpellService spellService
		)
	{
		EffectDamage damageEffect = triggerEffect as EffectDamage;
		EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
		
		//使用某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
		if (triggerEffect != null && triggerEffect.casterID == triggerBuff.targetID)
		{
			Spell ownedSpell = triggerEffect.ownedSpell;
			if (ownedSpell != null && ownedSpell.spellData.category == (int)SpellType.Spell_Type_PhyAttack && randNum <= 0.15f)
			{
				return 1;
			}
		}
		
		return 0;
	}
	
		public static int UsingPhysicalRate_10(
		Buff triggerBuff,
		Effect triggerEffect,
		SpellService spellService
		)
	{
		EffectDamage damageEffect = triggerEffect as EffectDamage;
		EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
		
		//使用某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
		if (triggerEffect != null && triggerEffect.casterID == triggerBuff.targetID)
		{
			Spell ownedSpell = triggerEffect.ownedSpell;
			if (ownedSpell != null && ownedSpell.spellData.category == (int)SpellType.Spell_Type_PhyAttack && randNum <= 0.1f)
			{
				return 1;
			}
		}
		
		return 0;
	}
    //使用物理攻击(未包含大招)
    public static int UsingPhysicalRate_50(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        ) 
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;

        //使用某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
		if (damageEffect != null && damageEffect.casterID == triggerBuff.targetID)
		{
			EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
			if (damageProto.isHeal == false)
			{
				Spell ownedSpell = triggerEffect.ownedSpell;
				if (ownedSpell != null && ownedSpell.spellData.category == (int)SpellType.Spell_Type_PhyAttack && randNum <= 0.5f)
				{
					return 1;
				}
			}				
		}	
		return 0;
    }      
    //使用魔法攻击(未包含大招)
    public static int UsingMagicRate_20(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        )
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;

        //使用某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
		if (damageEffect != null && damageEffect.casterID == triggerBuff.targetID)
		{
			EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
			if (damageProto.isHeal == false)
			{
				Spell ownedSpell = triggerEffect.ownedSpell;
				if (ownedSpell != null && ownedSpell.spellData.category == (int)SpellType.Spell_Type_MgicAttack && randNum <= 0.2f)
				{
					return 1;
				}
			}				
		}	
		return 0;
    }   
	public static int UsingPhysicalMagicRate_20(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        )
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;

        //使用某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
		if (damageEffect != null && damageEffect.casterID == triggerBuff.targetID)
		{
			EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
			if (damageProto.isHeal == false)
			{
				Spell ownedSpell = triggerEffect.ownedSpell;
				if (ownedSpell != null && (ownedSpell.spellData.category == (int)SpellType.Spell_Type_MgicAttack || ownedSpell.spellData.category == (int)SpellType.Spell_Type_PhyAttack) && randNum <= 0.2f)
				{
					return 1;
				}
			}				
		}	
		return 0;
    }   
	public static int UsingPhysicalMagicRate_90(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        )
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;

        //使用某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
		if (damageEffect != null && damageEffect.casterID == triggerBuff.targetID)
		{
			EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
			if (damageProto.isHeal == false)
			{
				Spell ownedSpell = triggerEffect.ownedSpell;
				if (ownedSpell != null && (ownedSpell.spellData.category == (int)SpellType.Spell_Type_MgicAttack || ownedSpell.spellData.category == (int)SpellType.Spell_Type_PhyAttack) && randNum <= 0.9f)
				{
					return 1;
				}
			}				
		}	
		return 0;
    }   
	
	    public static int UsingMagicRate_10(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        )
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;

        //使用某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
		if (damageEffect != null && damageEffect.casterID == triggerBuff.targetID)
		{
			EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
			if (damageProto.isHeal == false)
			{
				Spell ownedSpell = triggerEffect.ownedSpell;
				if (ownedSpell != null && ownedSpell.spellData.category == (int)SpellType.Spell_Type_MgicAttack && randNum <= 0.1f)
				{
					return 1;
				}
			}				
		}	
		return 0;
    }
	//使用魔法攻击(未包含大招)
    public static int UsingMagicRate_50(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        )
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;

        //使用某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
        if (triggerEffect != null && triggerEffect.casterID == triggerBuff.targetID)
        {
            Spell ownedSpell = triggerEffect.ownedSpell;
            if (ownedSpell != null && ownedSpell.spellData.category == (int)SpellType.Spell_Type_MgicAttack && randNum <= 0.5f)
            {
                return 1;
            }
        }
        return 0;
    }
	//使用魔法攻击(未包含大招)
	public static int UsingMagicRate_100(
		Buff triggerBuff,
		Effect triggerEffect,
		SpellService spellService
		)
	{
		EffectDamage damageEffect = triggerEffect as EffectDamage;
		EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
		
		//使用某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
		if (triggerEffect != null && triggerEffect.casterID == triggerBuff.targetID)
		{
			Spell ownedSpell = triggerEffect.ownedSpell;
			if (ownedSpell != null && ownedSpell.spellData.category == (int)SpellType.Spell_Type_MgicAttack && randNum <= 1.0f)
			{
				return 1;
			}
		}
		return 0;
	}
	//使用治疗攻击(未包含大招)
    public static int UsingCureRate_10(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        )
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;

        //使用某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
		if (triggerEffect != null && triggerEffect.casterID == triggerBuff.targetID)
        {
            Spell ownedSpell = triggerEffect.ownedSpell;
            if (ownedSpell != null && ownedSpell.spellData.category == (int)SpellType.Spell_Type_Cure && randNum <= 0.1f)
            {
                return 1;
            }
        }
        return 0;
    }
	public static int UsingCureRate_33(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        )
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;

        //使用某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
		if (triggerEffect != null && triggerEffect.casterID == triggerBuff.targetID)
        {
            Spell ownedSpell = triggerEffect.ownedSpell;
            if (ownedSpell != null && ownedSpell.spellData.category == (int)SpellType.Spell_Type_Cure && randNum <= 0.333f)
            {
                return 1;
            }
        }
        return 0;
    }
	public static int UsingCureRate_100(
		Buff triggerBuff,
		Effect triggerEffect,
		SpellService spellService
		)
	{
		EffectDamage damageEffect = triggerEffect as EffectDamage;
		EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
		
		//使用某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
		if (triggerEffect != null && triggerEffect.casterID == triggerBuff.targetID)
		{
			Spell ownedSpell = triggerEffect.ownedSpell;
			if (ownedSpell != null && ownedSpell.spellData.category == (int)SpellType.Spell_Type_Cure && randNum <= 1.0f)
			{
				return 1;
			}
		}
		return 0;
	}


    //受到物理或魔法攻击（不包含大招等）
    public static int HurtbyPhysicalMagicRate_80(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        )
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
        //受到某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
        if (damageEffect != null && damageEffect.targetID == triggerBuff.targetID)
        {
            EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
            if (damageProto.isHeal == false && (damageProto.damageType == SpellConst.damagePhy || damageProto.damageType == SpellConst.damageMagic) && randNum <= 0.8f)
            {
                return 1;
            }
			
        }
        return 0;
    }
	
	
	
	public static int HurtbyPhysicalMagicRate_25(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        )
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
        //受到某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
        if (damageEffect != null && damageEffect.targetID == triggerBuff.targetID)
        {
            EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
            if (damageProto.isHeal == false && (damageProto.damageType == SpellConst.damagePhy || damageProto.damageType == SpellConst.damageMagic) && randNum <= 0.25f)
            {
                return 1;
            }
			
        }
        return 0;
    }
	
	
	//受到物理或魔法攻击（不包含大招等）
	public static int HurtbyPhysicalMagicRate_100(
		Buff triggerBuff,
		Effect triggerEffect,
		SpellService spellService
		)
	{
		EffectDamage damageEffect = triggerEffect as EffectDamage;
		EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
		//受到某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
		if (damageEffect != null && damageEffect.targetID == triggerBuff.targetID)
		{
			EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
			if (damageProto.isHeal == false && (damageProto.damageType == SpellConst.damagePhy || damageProto.damageType == SpellConst.damageMagic) && randNum <= 1.0f)
			{
				return 1;
			}
			
		}
		return 0;
	}
	public static int HurtbyPhysicalMagicRate_10(
		Buff triggerBuff,
		Effect triggerEffect,
		SpellService spellService
		)
	{
		EffectDamage damageEffect = triggerEffect as EffectDamage;
		EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
		//受到某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
		if (damageEffect != null && damageEffect.targetID == triggerBuff.targetID)
		{
			EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
			if (damageProto.isHeal == false && (damageProto.damageType == SpellConst.damagePhy || damageProto.damageType == SpellConst.damageMagic) && randNum <= 0.1f)
			{
				return 1;
			}
			
		}
		return 0;
	}
	public static int HurtbyPhysicalMagicRate_50(
		Buff triggerBuff,
		Effect triggerEffect,
		SpellService spellService
		)
	{
		EffectDamage damageEffect = triggerEffect as EffectDamage;
		EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
		//受到某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
		if (damageEffect != null && damageEffect.targetID == triggerBuff.targetID)
		{
			EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
			if (damageProto.isHeal == false && (damageProto.damageType == SpellConst.damagePhy || damageProto.damageType == SpellConst.damageMagic) && randNum <= 0.5f)
			{
				return 1;
			}
			
		}
		return 0;
	}
	public static int HurtbyPhysicalRate_80(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        )
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
        //受到某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
        if (damageEffect != null && damageEffect.targetID == triggerBuff.targetID)
        {
            EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
            if (damageProto.isHeal == false && damageProto.damageType == SpellConst.damagePhy && randNum <= 0.8f)
            {
                return 1;
            }
        }
        return 0;
    }


    //生命值低于0.25
    public static int LifeLower_25(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        )
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;

        if (damageEffect != null &&
		    damageEffect.targetID == triggerBuff.targetID)
        {
            GameUnit target = spellService.GetUnit(triggerEffect.targetID);
            float lifeRatio = target.curLife / (float)target.maxLife;
            if (lifeRatio < 0.25f)
                return 1;
        }
        return 0;
    }
	public static int LifeLower_75_100(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        )
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;

        if (damageEffect != null &&
		    damageEffect.targetID == triggerBuff.targetID)
        {
            GameUnit target = spellService.GetUnit(triggerEffect.targetID);
            float lifeRatio = target.curLife / (float)target.maxLife;
            if (lifeRatio < 1.00f && lifeRatio >= 0.75f)
                return 1;
        }
        return 0;
    }
	public static int LifeLower_50_75(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        )
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;

        if (damageEffect != null &&
		    damageEffect.targetID == triggerBuff.targetID)
        {
            GameUnit target = spellService.GetUnit(triggerEffect.targetID);
            float lifeRatio = target.curLife / (float)target.maxLife;
            if (lifeRatio < 0.75f && lifeRatio >= 0.50f)
                return 1;
        }
        return 0;
    }
	public static int LifeLower_25_50(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        )
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;

        if (damageEffect != null &&
		    damageEffect.targetID == triggerBuff.targetID)
        {
            GameUnit target = spellService.GetUnit(triggerEffect.targetID);
            float lifeRatio = target.curLife / (float)target.maxLife;
            if (lifeRatio < 0.50f && lifeRatio >= 0.25f)
                return 1;
        }
        return 0;
    }
	public static int LifeLower_0_25(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        )
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;

        if (damageEffect != null &&
		    damageEffect.targetID == triggerBuff.targetID)
        {
            GameUnit target = spellService.GetUnit(triggerEffect.targetID);
            float lifeRatio = target.curLife / (float)target.maxLife;
            if (lifeRatio < 0.25f && lifeRatio > 0.00f)
                return 1;
        }
        return 0;
    }
	

	
	
	
	
	
	
	public static int UsingBuffRate_125(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        ) 
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;

        //使用某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
		if (damageEffect != null && damageEffect.casterID == triggerBuff.targetID)
		{
			EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
			if (damageProto.isHeal == false)
			{
				Spell ownedSpell = triggerEffect.ownedSpell;
				if (ownedSpell != null && ownedSpell.spellData.category == (int)SpellType.Spell_Type_Negative && randNum <= 0.125f)
				{
					return 1;
				}
			}				
		}	
		return 0;
    } 
	public static int UsingBuffRate_25(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        ) 
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;

        //使用某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
		if (damageEffect != null && damageEffect.casterID == triggerBuff.targetID)
		{
			EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
			if (damageProto.isHeal == false)
			{
				Spell ownedSpell = triggerEffect.ownedSpell;
				if (ownedSpell != null && ownedSpell.spellData.category == (int)SpellType.Spell_Type_Negative && randNum <= 0.25f)
				{
					return 1;
				}
			}				
		}	
		return 0;
    } 


	public static int UsingCureRate_50(
			Buff triggerBuff,
			Effect triggerEffect,
			SpellService spellService
			)
		{
			EffectDamage damageEffect = triggerEffect as EffectDamage;
			EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;

			//使用某类技能造成伤害触发示例
			float randNum = Random.Range(0.0f, 1.0f);
			if (triggerEffect != null && triggerEffect.casterID == triggerBuff.targetID)
			{
				Spell ownedSpell = triggerEffect.ownedSpell;
				if (ownedSpell != null && ownedSpell.spellData.category == (int)SpellType.Spell_Type_Cure && randNum <= 0.5f)
				{
					return 1;
				}
			}
			return 0;
		}
		
	//使用魔法攻击(未包含大招)
    public static int UsingMagicRateContainsdebuff_50(
          Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        )
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;

        //使用某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
        if (triggerEffect != null && triggerEffect.casterID == triggerBuff.targetID)
        {
            Spell ownedSpell = triggerEffect.ownedSpell;
			BattleObject target = ObjectDataMgr.Instance.GetBattleObject(triggerEffect.targetID);
			int count = target.unit.buffList.Count;
            if (ownedSpell != null && ownedSpell.spellData.category == (int)SpellType.Spell_Type_MgicAttack && randNum <= 0.5f)
            {
               for (int i = 0; i < count; ++i)
				{
					if (target.unit.buffList[i].buffProto.category == 4)
						return 1 ;
				}
            }
        }
        return 0;
    }	
	
	public static int UsingMagicRateContainsbeneficialbuff_50(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        )
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;

        //使用某类技能造成伤害触发示例
		float randNum = Random.Range(0.0f, 1.0f);
        if (triggerEffect != null && triggerEffect.casterID == triggerBuff.targetID)
        {
			Spell ownedSpell = triggerEffect.ownedSpell;
            BattleObject target = ObjectDataMgr.Instance.GetBattleObject(triggerEffect.targetID);
			int count = target.unit.buffList.Count;
			if (ownedSpell != null && ownedSpell.spellData.category == (int)SpellType.Spell_Type_MgicAttack && randNum <= 0.5f)
			{
               for (int i = 0; i < count; ++i)
				{
					if (target.unit.buffList[i].buffProto.category == 5)
						return 1 ;
				}
            }
        }
        return 0;
    }	
	public static int HurtbyStun_100(
        Buff triggerBuff,
        Effect triggerEffect,
        SpellService spellService
        )
    {
        EffectDamage damageEffect = triggerEffect as EffectDamage;
        EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
        //受到某类技能造成伤害触发示例
		if (buffEffect != null && buffEffect.targetID == triggerBuff.targetID)
        {
			EffectApplyBuffPrototype buffStun = buffEffect.protoEffect as EffectApplyBuffPrototype;
			BuffPrototype buffPt = StaticDataMgr.Instance.GetBuffProtoData(buffStun.buffID);
			if (buffPt != null && buffPt.category == 13)
            {
                return 1;
            }
        }
        return 0;
    }
	public static int HurtbyPhysicalMagicRateDispelpsv_100(
		Buff triggerBuff,
		Effect triggerEffect,
		SpellService spellService
		)
	{
		EffectDamage damageEffect = triggerEffect as EffectDamage;
		EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
		//受到某类技能造成伤害触发示例
		if (damageEffect != null && damageEffect.targetID == triggerBuff.targetID)
		{
			EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
			BattleObject target = ObjectDataMgr.Instance.GetBattleObject(damageEffect.targetID);
			if (damageProto.isHeal == false && (damageProto.damageType == SpellConst.damagePhy || damageProto.damageType == SpellConst.damageMagic) && NormalScript.GetWpLifeLeft(target,"bossdajie13Cike4wp02") == 0)
			{
				return 1;

			}
			
		}
		return 0;
	}

	public static int HurtbyPhysicalMagicRateDispelpsvLonggongtongzi_100(
		Buff triggerBuff,
		Effect triggerEffect,
		SpellService spellService
		)
	{
		EffectDamage damageEffect = triggerEffect as EffectDamage;
		EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
		//受到某类技能造成伤害触发示例
		if (damageEffect != null && damageEffect.targetID == triggerBuff.targetID)
		{
			EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
			BattleObject target = ObjectDataMgr.Instance.GetBattleObject(damageEffect.targetID);
			if (damageProto.isHeal == false && (damageProto.damageType == SpellConst.damagePhy || damageProto.damageType == SpellConst.damageMagic) && NormalScript.GetWpLifeLeft(target,"bossxiaoxiang26Longgongtongzi3wp03") == 0)
			{
				return 1;

			}
			
		}
		return 0;
	}

	public static int HurtbyPhysicalMagicRateAddbuffHetong_100(
		Buff triggerBuff,
		Effect triggerEffect,
		SpellService spellService
		)
	{
		EffectDamage damageEffect = triggerEffect as EffectDamage;
		EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
		//受到某类技能造成伤害触发示例
		if (damageEffect != null && damageEffect.targetID == triggerBuff.targetID)
		{
			EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
			BattleObject target = ObjectDataMgr.Instance.GetBattleObject(damageEffect.targetID);
			if (damageProto.isHeal == false && (damageProto.damageType == SpellConst.damagePhy || damageProto.damageType == SpellConst.damageMagic) && NormalScript.GetWpLifeLeft(target,"bossxiaoxiang27Hetong2wp03") == 0)
			{
				return 1;

			}
			
		}
		return 0;
	}
	public static int HurtbyPhysicalMagicRateDispelbuffLongnv_100(
		Buff triggerBuff,
		Effect triggerEffect,
		SpellService spellService
		)
	{
		EffectDamage damageEffect = triggerEffect as EffectDamage;
		EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
		//受到某类技能造成伤害触发示例
		if (damageEffect != null && damageEffect.targetID == triggerBuff.targetID)
		{
			EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
			BattleObject target = ObjectDataMgr.Instance.GetBattleObject(damageEffect.targetID);
			BossAi curAi = target.gameObject.GetComponent<BossAi>();
			bosssenlin32Longnv3 actualAi = curAi as bosssenlin32Longnv3;
			if (damageProto.isHeal == false && (damageProto.damageType == SpellConst.damagePhy || damageProto.damageType == SpellConst.damageMagic) && NormalScript.GetWpLifeLeft(target,"bosssenlin32Longnv3wp03") == 0 && NormalScript.GetWpLifeLeft(target,"bosssenlin32Longnv3wp04") == 0 && actualAi.i == 1)
			{
				actualAi.i --;
				return 1;
			}
			
		}
		return 0;
	}

	public static int HurtbyPhysicalMagicRateAddbuffAmute_100(
		Buff triggerBuff,
		Effect triggerEffect,
		SpellService spellService
		)
	{
		EffectDamage damageEffect = triggerEffect as EffectDamage;
		EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
		//受到某类技能造成伤害触发示例
		if (damageEffect != null && damageEffect.targetID == triggerBuff.targetID)
		{
			EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
			BattleObject target = ObjectDataMgr.Instance.GetBattleObject(damageEffect.targetID);
			if (damageProto.isHeal == false && (damageProto.damageType == SpellConst.damagePhy || damageProto.damageType == SpellConst.damageMagic) && NormalScript.GetWpLifeLeft(target,"bosssenlin34Amute3wp03") == 0)
			{
				return 1;

			}
			
		}
		return 0;
	}
	public static int HurtbyPhysicalMagicRateDispelpsvMomo_100(
		Buff triggerBuff,
		Effect triggerEffect,
		SpellService spellService
		)
	{
		EffectDamage damageEffect = triggerEffect as EffectDamage;
		EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
		//受到某类技能造成伤害触发示例
		if (damageEffect != null && damageEffect.targetID == triggerBuff.targetID)
		{
			EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
			BattleObject target = ObjectDataMgr.Instance.GetBattleObject(damageEffect.targetID);
			if (damageProto.isHeal == false && (damageProto.damageType == SpellConst.damagePhy || damageProto.damageType == SpellConst.damageMagic) && NormalScript.GetWpLifeLeft(target,"bosssenlin36Momo1wp02") == 0)
			{
				return 1;

			}
			
		}
		return 0;
	}
	public static int HurtbyPhysicalMagicRateHurtselfMeidusha1_100(
		Buff triggerBuff,
		Effect triggerEffect,
		SpellService spellService
		)
	{
		EffectDamage damageEffect = triggerEffect as EffectDamage;
		EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
		//受到某类技能造成伤害触发示例
		if (damageEffect != null && damageEffect.targetID == triggerBuff.targetID)
		{
			EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
			BattleObject target = ObjectDataMgr.Instance.GetBattleObject(damageEffect.targetID);
			if (damageProto.isHeal == false && (damageProto.damageType == SpellConst.damagePhy || damageProto.damageType == SpellConst.damageMagic) && NormalScript.GetWpLifeLeft(target,"bosssenlin38Meidusha2wp02") == 0)
			{
				return 1;

			}
			
		}
		return 0;
	}
	public static int HurtbyPhysicalMagicRateHurtselfMeidusha2_100(
		Buff triggerBuff,
		Effect triggerEffect,
		SpellService spellService
		)
	{
		EffectDamage damageEffect = triggerEffect as EffectDamage;
		EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
		//受到某类技能造成伤害触发示例
		if (damageEffect != null && damageEffect.targetID == triggerBuff.targetID)
		{
			EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
			BattleObject target = ObjectDataMgr.Instance.GetBattleObject(damageEffect.targetID);
			if (damageProto.isHeal == false && (damageProto.damageType == SpellConst.damagePhy || damageProto.damageType == SpellConst.damageMagic) && NormalScript.GetWpLifeLeft(target,"bosssenlin38Meidusha2wp03") == 0)
			{
				return 1;

			}
			
		}
		return 0;
	}
	public static int HurtbyPhysicalMagicRateHurtselfMeidusha3_100(
		Buff triggerBuff,
		Effect triggerEffect,
		SpellService spellService
		)
	{
		EffectDamage damageEffect = triggerEffect as EffectDamage;
		EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
		//受到某类技能造成伤害触发示例
		if (damageEffect != null && damageEffect.targetID == triggerBuff.targetID)
		{
			EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
			BattleObject target = ObjectDataMgr.Instance.GetBattleObject(damageEffect.targetID);
			if (damageProto.isHeal == false && (damageProto.damageType == SpellConst.damagePhy || damageProto.damageType == SpellConst.damageMagic) && NormalScript.GetWpLifeLeft(target,"bosssenlin38Meidusha2wp04") == 0)
			{
				return 1;

			}
			
		}
		return 0;
	}
	public static int HurtbyPhysicalMagicRateHurtselfMeidusha4_100(
		Buff triggerBuff,
		Effect triggerEffect,
		SpellService spellService
		)
	{
		EffectDamage damageEffect = triggerEffect as EffectDamage;
		EffectApplyBuff buffEffect = triggerEffect as EffectApplyBuff;
		//受到某类技能造成伤害触发示例
		if (damageEffect != null && damageEffect.targetID == triggerBuff.targetID)
		{
			EffectDamageProtoType damageProto = damageEffect.protoEffect as EffectDamageProtoType;
			BattleObject target = ObjectDataMgr.Instance.GetBattleObject(damageEffect.targetID);
			if (damageProto.isHeal == false && (damageProto.damageType == SpellConst.damagePhy || damageProto.damageType == SpellConst.damageMagic) && NormalScript.GetWpLifeLeft(target,"bosssenlin38Meidusha2wp05") == 0)
			{
				return 1;

			}
			
		}
		return 0;
	}

	


	
		
}		