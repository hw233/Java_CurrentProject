﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BattleModule : ModuleBase
{
    BattleController controller;
    //BattleProcess process;
	WeakPointController  weakPointController;
    BattleUnitAi battleUnitAi;
	PhyDazhaoController phyDazhaoController;
	MagicDazhaoController magicDazhaoController;
    bool startBattle = false;

    void BindListener()
    {
        //GameEventMgr.Instance.AddListener<PbStartBattle>(GameEventList.StartBattle, controller.StartBattle);
        //GameEventMgr.Instance.AddListener(GameEventList.ShowBattleUI, OnShowBattleUI);
    }

    void UnBindListener()
    {
        //GameEventMgr.Instance.RemoveListener<PbStartBattle>(GameEventList.StartBattle, controller.StartBattle);
        //GameEventMgr.Instance.RemoveListener(GameEventList.ShowBattleUI, OnShowBattleUI);
    }

    void Start()
    {
        BattleCamera.Instance.Init();
    }

    //void Update()
    //{
    //    if (startBattle == true)
    //    {
    //        if (ResourceMgr.Instance.GetAssetRequestCount() == 0)
    //        {
    //            startBattle = false;
    //            StartCoroutine(FinishLoad());
    //        }
    //    }
    //}

    public override void OnInit(object param)
    {
        BattleUnitAi.BattleStyle battleStyle;
        EnterInstanceParam enterParam = param as EnterInstanceParam;
        if (enterParam != null)
        {
            battleStyle = BattleUnitAi.BattleStyle.PVE;
        }
        else
        {
            battleStyle = BattleUnitAi.BattleStyle.PVP;
        }

        controller = gameObject.AddComponent<BattleController>();
        //process = gameObject.AddComponent<BattleProcess>();
        controller.Init();

		weakPointController = gameObject.AddComponent<WeakPointController> ();
		weakPointController.Init ();

        battleUnitAi = gameObject.AddComponent<BattleUnitAi>();
		battleUnitAi.Init (battleStyle);

		phyDazhaoController = gameObject.AddComponent<PhyDazhaoController> ();

		magicDazhaoController = gameObject.AddComponent<MagicDazhaoController> ();
		
    }

    public override void OnEnter(object param)
    {
        BindListener();
        EnterInstanceParam enterParam = param as EnterInstanceParam;
        PvpFightParam pvpParam = null;
        GuideLevelParam guideParam = null;
        if (enterParam == null)
        {
            pvpParam = param as PvpFightParam;
            if (pvpParam == null)
            {
                guideParam = param as GuideLevelParam;
            }
        }
        //pve
        if (enterParam != null)
        {
            UILoading loading = UIMgr.Instance.OpenUI_(UILoading.ViewName) as UILoading;
            if (loading != null)
            {
                if (GameDataMgr.Instance.curInstanceType == (int)InstanceType.Normal)
                    loading.SetLoading(LoadingType.loadingFb);
                else if (GameDataMgr.Instance.curInstanceType == (int)InstanceType.Hole)
                    loading.SetLoading(LoadingType.loadingHole);
                else if (GameDataMgr.Instance.curInstanceType == (int)InstanceType.Tower)
                    loading.SetLoading(LoadingType.loadingTower);
                else if (GameDataMgr.Instance.curInstanceType == (int)InstanceType.Guild)
                    loading.SetLoading(LoadingType.loadingGuild);

                UIMgr.Instance.FixBrokenWord();
                controller.StartBattlePrepare(enterParam);
                loading.SetLoadingCallback(LoadResourceFinish);
                loading.UpdateTotalAssetCount();
            }
        }
        //pvp
        else if (pvpParam != null)
        {
            UILoading loading = UIMgr.Instance.OpenUI_(UILoading.ViewName) as UILoading;
            if (loading != null)
            {
                loading.SetLoading(LoadingType.loadingPvp);
                loading.OpenPvploading(pvpParam);
                UIMgr.Instance.FixBrokenWord();
                controller.StartBattlePvpPrepare(pvpParam);
                loading.SetLoadingCallback(LoadResourceFinish);
                loading.UpdateTotalAssetCount();
            }
        }
        //guide
        else if (guideParam != null)
        {
            UILoading loading = UIMgr.Instance.OpenUI_(UILoading.ViewName) as UILoading;
            if (loading != null)
            {
                loading.SetLoading(LoadingType.loadingFb);
                UIMgr.Instance.FixBrokenWord();
                controller.StartBattleGuidePrepare(guideParam);
                loading.SetLoadingCallback(LoadResourceFinish);
                loading.UpdateTotalAssetCount();
            }
        }

        //if in first guide level, no im
        if (guideParam == null)
            UIIm.Instance.UpdateIMPos(true);
    }

    public override void OnExecute()
    {

    }

    public override IEnumerator LoadResourceFinish()
    {
        //wait for ui
        yield return new WaitForFixedUpdate();
        controller.StartBattle();
        UIMgr.Instance.CloseUI_(UILoading.ViewName);
    }

    public override void OnExit()
    {
        UnBindListener();
        Destroy(controller);
        //Destroy(process);
        Destroy(weakPointController);
        Destroy(battleUnitAi);
        phyDazhaoController.DestroyController();
        magicDazhaoController.DestroyController();
		Destroy (phyDazhaoController);
		Destroy (magicDazhaoController);
        //UIMgr.Instance.CloseUI(UIBattle.ViewName);
        //destroy battle camera manual,since camera may attach to gamemain throw ani
        Destroy(BattleCamera.Instance.gameObject);
        startBattle = false;
        if (UIIm.Instance != null)
        {
            UIIm.Instance.UpdateIMPos(false);
        }
    }

#region  Event
    //void OnShowBattleUI()
    //{
    //    var ui = UIMgr.Instance.OpenUI(UIBattle.AssertName, UIBattle.ViewName);
    //    ui.GetComponent<UIBattle>().Init();
    //}
#endregion
}
