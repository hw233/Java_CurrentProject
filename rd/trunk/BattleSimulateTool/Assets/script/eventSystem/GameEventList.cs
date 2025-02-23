using UnityEngine;
using System.Collections;

public class GameEventList
{
    //Login
    public static string LoginClick = "LoginClick";//no param
    public static string ServerClick = "ServerClick";//param hashtable
    public static string createPlayerClick = "CreatePlayerClick";//no param

    //Build
    public static string BattleBtnClick = "BattleBtnClick";

    //////////////////////////////////////////////////////////////////////////
    //Battle
    public static string StartBattle = "StartBattle";  //param: PbStartBattle  
	//public static string LoadBattleObjectFinished = "LoadBattleObjectFinished";//param GameUnitData
	public static string ChangeUIBattleState = "ChangeUIBattleState";// param UIBattle.UiState

	//Mirror in the Battle
	public static string SetMirrorModeState = "SetMirrorModeState"; //param bool 
	public static string FindWeakPoint = "FindWeakPoint";//param List<MirrorTarget>
	public static string FindFinishedWeakPoint = "FindFinishedWeakPoint";//param  List<MirrorTarget>
	public static string MirrorOutWeakPoint = "MirrorOutWeakPoint";//param  List<MirrorTarget>
	public static string MirrorClicked = "MirrorClicked";//param vector3
	public static string ShowFindMonsterInfo = "ShowFindMonsterInfo";//MirrorTarget
	public static string HideFindMonsterInfo = "HideFindMonsterInfo";//
    public static string ShowHideMonster = "ShowHideMonster";//find the hide monster in battle


    //process
    public static string SwitchPet = "SwitchPet";//param <int,int>
	public static string ShowSwitchPetUI = "ShowSwitchPetUI";//param  int
    public static string HideSwitchPetUI = "HideSwitchPetUI";//param  int -1则无条件关闭换宠UI，其他则和当前被换宠物id相同才关闭
    public static string ChangeTarget = "ChangeTarget";
    public static string ShowDazhaoTip = "ShowDazhaoTip";
    public static string HideDazhaoTip = "HideDazhaoTip";
	public static string ShowFireFocus = "ShowFireFocus";//param GameUnit
	public static string HideFireFocus = "HideFireFocus";// no param
	public static string DazhaoActionOver = "DazhaoActionOver";//param BattleObject

	public static string WeakpoingDead = "WeakpointDead";//param GameUnit,string

	//数据统计
	public static string SpellAttackStatistics = "SpellAttackStatistics";//

	//Dazhao
	public static string DazhaoBtnClicked = "DazhaoBtnClicked";
	public static string ExitDazhaoByPhyAttacked = "ExitDazhaoByPhyAttacked";//param int(unit.guid)
	public static string RemoveDazhaoAction = "RemoveDazhaoAction";

	public static string OverMagicShifaWithResult = "OverMagicShifaWithResult";//0 failed,  1 succ
	public static string MonsterShowoffOver = "MonsterShowoffOver";

    //副本 
	public static	string	FinishedInstance = "FinishedInstance";//param(int ,string) star instanceid


    //Spell
    public static string SpellFire = "SpellFire";
    public static string SpellLifeChange = "SpellLifeChange";
    public static string SpellEnergyChange = "SpellEnergyChange";
    public static string SpellUnitDead = "SpellUnitDead";
    public static string spellUnitRevive = "SpellUnitRevive";
    public static string SpellBuff = "SpellBuff";
    public static string SpellEffect = "SpellEffect";
    //public static string SpellMiss = "SpellMiss";
    public static string SpellImmune = "SpellImmune";
    public static string SpellStun = "SpellStun";
    public static string SpellAbsrobed = "SpellAbsorbed";

    public static string NormalHit = "NormalHit";
    public static string BashHit = "BashHit";

	//GameDataChange for ui
	public static	string LevelChanged = "LeveleChanged";//param int
	public static   string CoinChanged  = "CoinChanged";//param int jinbi
	public static   string ZuanshiChanged = "ZuanshiChanged";
    public static string PlayerExpChanged = "PlayerExpChanged";//param int int (oldexp，newexp)

    public static string QuestChanged = "QuestChanged";

    //Mail
    public static string MailAdd = "MailAdd";
    public static string MailRead = "MailRead";

    public static string PreUnit = "PreUnit";//param gameUnit
    public static string NextUnit = "NextUnit";//param gameUnit

	//Shop
	public	static	string	RefreshShopUi = "RefreshShopUi";//parama no
	public	static	string	RefreshShopUiAfterBuy = "RefreshShopUiAfterBuy";

    //UI--Pet/Equip
    public static string ReloadPetListNotify = "reloadPetListNotify";
    public static string ReloadPetStageNotify = "reloadPetStageNotify";
    public static string ReloadPetEquipNotify = "reloadPetEquipNotify";
    public static string ReloadEquipGemNotify = "reloadEquipGemNotify";

	//buyItem
	public	static	string	BuyItemFinished = "BuyItemFinished";
	//open box
	public	static	string	OpenBoxFinished = "OpenBoxFinished";

    //speed service
    public const string SpeedChangeEvent = "EventSpeedChange";

}