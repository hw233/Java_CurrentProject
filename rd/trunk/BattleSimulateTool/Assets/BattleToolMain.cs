﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
public class InputUnitData
{
    public string unitID;
    public int unitLvl;
    public int unitCharacter;
}
public class OperationData
{
    public string[] weaknessName;//弱点ID
    public string[] xgName;
    public int[] duijuNum;//对局数
    public int[] roundNum;//回合数
    public int[] IntervalRoundNum;//间隔回合数
}
public class BattleToolMain : MonoBehaviour
{
    static BattleToolMain mInstance;
    public static BattleToolMain Instance
    {
        get { return mInstance; }
    }
    public List<InputUnitData> mMainUnitDataList = new List<InputUnitData>();
    private int mCurSimulateCount;
    public InputField mInstanceID;//副本ID
    public GameObject startButton;//开始按钮
    public InputField operationID;//玩家操作id
    public List<GameObject> gwList;//怪物列表
    public InputField mSimulateCount;//循环次数//hpx1531v
    AttData attData;
    public OperationData operationData;
    public string attWpName;
    public int bureauNum;//对局数
    public int roundNum = 0;//回合数
    public List<BattleObject> mMainUnitList = new List<BattleObject>();
    //---------------------------------------------------------------------------------------------
    void Awake()
    {
        mInstance = this;
        GameEventMgr.Instance.Init();
        //ResourceMgr.Instance.Init();
        StaticDataMgr.Instance.Init();
        ObjectDataMgr.Instance.Init();
        LayerConst.Init();
        //UIMgr.Instance.Init();
        //GameMain.Instance.Init();
        SpellService.Instance.Init();
        //GameSpeedService.Instance.Init();
        mCurSimulateCount = 0;
        BattleController bc = gameObject.GetComponent<BattleController>();
        bc.Init();
    }
    //---------------------------------------------------------------------------------------------
    void Start()
    {
        EventTriggerListener.Get(startButton).onClick = StartClick;       
    }
    void StartClick(GameObject but)
    {
        LogResult.Instance.xhNumber = 0;
        mCurSimulateCount = 0;
        int count = int.Parse(mSimulateCount.text);
        LogResult.Instance.logData = new LogData[count];
        for (int i = 0; i < count; ++i)
        {
            LogResult.Instance.logData[i] = new LogData();
        }
        StartSimulate();
    }
    //---------------------------------------------------------------------------------------------
    public void StartSimulate()
    {
        InitMainUnitList();
        BattleController.Instance.StartSimulate();       
    }
    //---------------------------------------------------------------------------------------------
    public void OnSimulateEnd()
    {
        ++mCurSimulateCount;
        if (mCurSimulateCount < int.Parse(mSimulateCount.text))
        {
            StartSimulate();
        }
    }
    //---------------------------------------------------------------------------------------------
    public void InitMainUnitList()
    {
        //test only
        for (int i = 0; i < 5; ++i)
        {
            attData = gwList[i].GetComponent<AttData>();
            InputUnitData curUnitData = new InputUnitData();
            curUnitData.unitID = attData.gwID.text;
            curUnitData.unitLvl = int.Parse(attData.gwLv.text);
            curUnitData.unitCharacter = int.Parse(attData.gwCharacter.text);
            //curUnitData.unitID = "xgLangren";
            //curUnitData.unitLvl = 40;
            //curUnitData.unitCharacter = 1;
            mMainUnitDataList.Add(curUnitData);
        }
		operationData = StaticDataMgr.Instance.GetPlayerBehaviorData(operationID.text);//"minghe18_1");//minghe18_1");//
        //end test
        int playerUnitStartID = 100;
        mMainUnitList.Clear();
        int count = mMainUnitDataList.Count;
        for (int i = 0; i < count; ++i)
        {
            InputUnitData inputUnitData = mMainUnitDataList[i];
            PbUnit pbUnit = new PbUnit();
            pbUnit.guid = ++playerUnitStartID;
            pbUnit.id = inputUnitData.unitID;
            pbUnit.level = inputUnitData.unitLvl;
            pbUnit.camp = UnitCamp.Player;
            pbUnit.slot = i;
            pbUnit.lazy = BattleConst.defaultLazy;
            pbUnit.character = inputUnitData.unitCharacter;
            GameUnit curUnit = GameUnit.FromPb(pbUnit, true);
            BattleObject bo = ObjectDataMgr.Instance.CreateBattleObject(curUnit, null, Vector3.zero, Quaternion.identity);
            mMainUnitList.Add(bo);
        }
        mMainUnitDataList.Clear();
    }    
}
