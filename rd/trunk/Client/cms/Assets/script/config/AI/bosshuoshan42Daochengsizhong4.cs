﻿

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class bosshuoshan42Daochengsizhong4 : BossAi {

	// Use this for initialization
	void Start () 
	{
	
	}
	int jishu = 0 ;
	int i = 1 ;

	public override BattleUnitAi.AiAttackResult GetAiAttackResult(GameUnit Daochengsizhong4Unit)
	{
		BattleUnitAi.AiAttackResult attackResult = new BattleUnitAi.AiAttackResult ();

		//attackStyle todo
		attackResult.attackStyle = BattleUnitAi.AiAttackStyle.PhysicsAttack;

		//spell todo
		Dictionary<string,Spell> Daochengsizhong4SpellDic = GetUnitSpellList (Daochengsizhong4Unit);

		Spell useSpell = null;
		Daochengsizhong4SpellDic.TryGetValue ("bosshuoshan42Daochengsizhong42", out useSpell);

		attackResult.attackTarget = GetAttackRandomTarget(Daochengsizhong4Unit);
		if (jishu == 0)
		{
			if (GetAttackCount(Daochengsizhong4Unit) % 7 == 0 && GetAttackCount(Daochengsizhong4Unit) != 0) 
			{
				Daochengsizhong4SpellDic.TryGetValue ("bosshuoshan42Daochengsizhong43", out useSpell);
			}
		}
		else 
		{
			if (i == 1)
			{
				Daochengsizhong4SpellDic.TryGetValue ("dispelPassive", out useSpell);
				i--;
			}		
			else if (GetAttackCount(Daochengsizhong4Unit) % 7 == 0 && GetAttackCount(Daochengsizhong4Unit) != 0) 
			{
				Daochengsizhong4SpellDic.TryGetValue ("bosshuoshan42Daochengsizhong44", out useSpell);
			}
		}

		
		attackResult.useSpell = useSpell;

		return attackResult;
    }
    //---------------------------------------------------------------------------------------------
    
    //---------------------------------------------------------------------------------------------
	public override void OnWpDead(WeakPointDeadArgs args)
	{
		BattleObject target = ObjectDataMgr.Instance.GetBattleObject(args.targetID);
		if (args.wpID == "bosshuoshan42Daochengsizhong4wp02")
		{
			target.TriggerEvent("daochengsizhong4_wp02dead", Time.time, null);
			BattleController.Instance.GetUIBattle().wpUI.ChangeBatch(1.5f);
			jishu ++;
		}
	}
	//---------------------------------------------------------------------------------------------
}
