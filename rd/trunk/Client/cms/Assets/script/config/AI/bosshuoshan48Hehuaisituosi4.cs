﻿

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class bosshuoshan48Hehuaisituosi4 : BossAi {

	// Use this for initialization
	void Start () 
	{
	
	}
	int jishu = 0 ;
	int jishu1 = 0 ;
	int jishu2 = 0 ;
	int wp02 = 0 ;
	int wp03 = 0 ;
	int wp04 = 0 ;
	int wp05 = 0 ;


	public override BattleUnitAi.AiAttackResult GetAiAttackResult(GameUnit Hehuaisituosi4Unit)
	{
		BattleUnitAi.AiAttackResult attackResult = new BattleUnitAi.AiAttackResult ();

		//attackStyle todo
		attackResult.attackStyle = BattleUnitAi.AiAttackStyle.PhysicsAttack;

		//spell todo
		Dictionary<string,Spell> Hehuaisituosi4SpellDic = GetUnitSpellList (Hehuaisituosi4Unit);

		Spell useSpell = null;
		Hehuaisituosi4SpellDic.TryGetValue ("bosshuoshan48Hehuaisituosi41", out useSpell);

		attackResult.attackTarget = GetAttackRandomTarget(Hehuaisituosi4Unit);

		if (NormalScript.GetTotalRound() <= 15 ) 
		{
		
			if (GetAttackCount(Hehuaisituosi4Unit) % 3 == 0 && GetAttackCount(Hehuaisituosi4Unit) != 0) 
			{
				Hehuaisituosi4SpellDic.TryGetValue ("bosshuoshan48Hehuaisituosi42", out useSpell);

			}
		}
		else 
		{
			if (jishu1 == 0 && jishu2 == 0)
			{		
				if (GetAttackCount(Hehuaisituosi4Unit) % 3 == 0 && GetAttackCount(Hehuaisituosi4Unit) != 0) 
				{
					Hehuaisituosi4SpellDic.TryGetValue ("bosshuoshan48Hehuaisituosi42", out useSpell);

				}
			}
			else if (jishu1 == 0 && jishu2 != 0)
			{		
				if (GetAttackCount(Hehuaisituosi4Unit) % 7 == 0 && GetAttackCount(Hehuaisituosi4Unit) != 0) 
				{
					Hehuaisituosi4SpellDic.TryGetValue ("bosshuoshan48Hehuaisituosi44", out useSpell);

				}
				else if(GetAttackCount(Hehuaisituosi4Unit) % 3 == 0 && GetAttackCount(Hehuaisituosi4Unit) != 0) 
				{
					Hehuaisituosi4SpellDic.TryGetValue ("bosshuoshan48Hehuaisituosi42", out useSpell);

				}
			}
			else if (jishu1 != 0 && jishu2 == 0)
			{		
				if (GetAttackCount(Hehuaisituosi4Unit) % 7 == 0 && GetAttackCount(Hehuaisituosi4Unit) != 0) 
				{
					Hehuaisituosi4SpellDic.TryGetValue ("bosshuoshan48Hehuaisituosi43", out useSpell);

				}
				else if(GetAttackCount(Hehuaisituosi4Unit) % 3 == 0 && GetAttackCount(Hehuaisituosi4Unit) != 0) 
				{
					Hehuaisituosi4SpellDic.TryGetValue ("bosshuoshan48Hehuaisituosi42", out useSpell);

				}
			}
			else
			{		
				if (GetAttackCount(Hehuaisituosi4Unit) % 7 == 0 && GetAttackCount(Hehuaisituosi4Unit) != 0) 
				{
					Hehuaisituosi4SpellDic.TryGetValue ("bosshuoshan48Hehuaisituosi45", out useSpell);

				}
				else if(GetAttackCount(Hehuaisituosi4Unit) % 3 == 0 && GetAttackCount(Hehuaisituosi4Unit) != 0) 
				{
					Hehuaisituosi4SpellDic.TryGetValue ("bosshuoshan48Hehuaisituosi42", out useSpell);

				}
			}

		}

		if (NormalScript.GetTotalRound() > 15 && jishu==0) 
		{
			Hehuaisituosi4Unit.battleUnit.TriggerEvent("hehuaisituosi4_wp01_state3to0", Time.time, null);
			BattleController.Instance.GetUIBattle().wpUI.ChangeBatch(1.5f);
			jishu ++;
			List<string> wpList = null;
			wpList = GetAliveWeakPointList (Hehuaisituosi4Unit);
			for(int n = wpList.Count -1 ;n > 0;n--)
			{
				if (wpList[n] == "bosshuoshan48Hehuaisituosi4wp02")
				{
					jishu1++;
					wp02++;
				}
				if (wpList[n] == "bosshuoshan48Hehuaisituosi4wp03")
				{
					jishu2++;
					wp03++;
				}
				if (wpList[n] == "bosshuoshan48Hehuaisituosi4wp04")
				{
					jishu1++;
					wp04++;
				}
				if (wpList[n] == "bosshuoshan48Hehuaisituosi4wp05")
				{
					jishu2++;
					wp05++;
				}
			}
			if(jishu1==1 && wp02==1)
			{
				Hehuaisituosi4Unit.battleUnit.TriggerEvent("hehuaisituosi4_wp02_state0to3", Time.time, null);
			}
			
			if(jishu2==1 && wp03==1)
			{
				Hehuaisituosi4Unit.battleUnit.TriggerEvent("hehuaisituosi4_wp03_state0to3", Time.time, null);
			}

			if(jishu2==2)
			{
				Hehuaisituosi4Unit.battleUnit.TriggerEvent("hehuaisituosi4_wp0305_state0to3", Time.time, null);
			}
			
			if(jishu1==1 && wp04==1)
			{
				Hehuaisituosi4Unit.battleUnit.TriggerEvent("hehuaisituosi4_wp04_state0to3", Time.time, null);
			}
			
			if(jishu2==1 && wp05==1)
			{
				Hehuaisituosi4Unit.battleUnit.TriggerEvent("hehuaisituosi4_wp05_state0to3", Time.time, null);
			}

			if(jishu1==2)
			{
				Hehuaisituosi4Unit.battleUnit.TriggerEvent("hehuaisituosi4_wp0204_state0to3", Time.time, null);
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
		if (args.wpID == "bosshuoshan48Hehuaisituosi4wp02")
		{
			target.TriggerEvent("hehuaisituosi4_wp02dead", Time.time, null);
		}
		if (args.wpID == "bosshuoshan48Hehuaisituosi4wp03")
		{
			target.TriggerEvent("hehuaisituosi4_wp03dead", Time.time, null);
		}
		if (args.wpID == "bosshuoshan48Hehuaisituosi4wp04")
		{
			target.TriggerEvent("hehuaisituosi4_wp04dead", Time.time, null);
		}
		if (args.wpID == "bosshuoshan48Hehuaisituosi4wp05")
		{
			target.TriggerEvent("hehuaisituosi4_wp05dead", Time.time, null);
		}

	}
	//---------------------------------------------------------------------------------------------
}
