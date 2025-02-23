﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public	enum BagType:int
{
	BaoXiang = 0,
	Xiaohao,
	Baoshi,
	Cailiao,
	Count,
	Unknow
}

public	enum BagState
{
	Norml = 0,
	Sell
}

public class UIBag : UIBase,TabButtonDelegate
{
    public static string ViewName = "UIBag";
	public	static	UIBag Instance = null;

    public Button m_closeButton;
	public	TabButtonGroup	tabBtnGroup;
	public	ScrollView	scrollView;
	public	Button	cancelSellButton;
	public	Button	conformSellButton;
	public	Button	sellButton;
	public	Text	bagTitleText;

	private BagType curBagType = BagType.BaoXiang;

	private BagState curBagState = BagState.Norml;
	public BagState  BagStateAttr 
	{
		get{ return curBagState;}
		set
		{
			sellItemsDic.Clear ();
			if(value != curBagState)
			{
				curBagState = value;
				RefreshBag();
			}
		}
	}

	private	List<PB.HSRewardInfo> listRewardForBox = new List<PB.HSRewardInfo>();
	private	Dictionary<string,int>  sellItemsDic = new Dictionary<string, int>();
	private	ItemData	curUseItemData;
	private	bool	isCanSell = false;

    public static UIBag OpenWith(BagType bagType = BagType.BaoXiang)
    {
        UIBag bag = (UIBag)UIMgr.Instance.OpenUI_(ViewName);
        bag.InitWith(bagType);
        return bag;
    }

    void Start()
    {
		Instance = this;
        
    }
	bool isFirst = true;

    public void InitWith(BagType bagType = BagType.BaoXiang)
    {
        if (isFirst)
        {
            isFirst = false;
            FirsInit();
        }
        tabBtnGroup.OnChangeItem((int)bagType);
        curBagState = BagState.Norml;
        RefreshBag();
    }

	void FirsInit()
	{
		EventTriggerListener.Get(m_closeButton.gameObject).onClick = CloseBagButtonDown;
		EventTriggerListener.Get (cancelSellButton.gameObject).onClick = OnSellCancleClick;
		EventTriggerListener.Get (conformSellButton.gameObject).onClick = OnSellConformClick;
		EventTriggerListener.Get (sellButton.gameObject).onClick = OnSellClick;
		tabBtnGroup.InitWithDelegate (this);

		bagTitleText.text = StaticDataMgr.Instance.GetTextByID ("bag_title");

		tabBtnGroup.tabButtonList [0].SetButtonTitleName (StaticDataMgr.Instance.GetTextByID ("bag_tag_box"));
		tabBtnGroup.tabButtonList [1].SetButtonTitleName (StaticDataMgr.Instance.GetTextByID ("bag_tag_use"));
		tabBtnGroup.tabButtonList [2].SetButtonTitleName (StaticDataMgr.Instance.GetTextByID ("bag_tag_gem"));
		tabBtnGroup.tabButtonList [3].SetButtonTitleName (StaticDataMgr.Instance.GetTextByID ("bag_tag_item"));

		sellButton.GetComponentInChildren<Text> ().text = StaticDataMgr.Instance.GetTextByID ("ui_chushou");
		cancelSellButton.GetComponentInChildren<Text> ().text = StaticDataMgr.Instance.GetTextByID ("ui_quxiao");
		conformSellButton.GetComponentInChildren<Text> ().text = StaticDataMgr.Instance.GetTextByID ("ui_queding");

	}

    void OnEnable()
    {
        BindListener();
    }
    void OnDisable()
    {
        UnBindListener();
    }


	void	BindListener()
	{
		GameEventMgr.Instance.AddListener (GameEventList.BuyItemFinished, RefreshBag);
		GameEventMgr.Instance.AddListener (GameEventList.OpenBoxFinished, OnOpenBoxFinished);
		GameEventMgr.Instance.AddListener<ProtocolMessage>(PB.code.PLAYER_REWARD_S.GetHashCode().ToString(), OnReward);

		GameEventMgr.Instance.AddListener<ProtocolMessage> (PB.code.ITEM_SELL_BATCH_C.GetHashCode ().ToString (), OnRequestSellFinished);
		GameEventMgr.Instance.AddListener<ProtocolMessage> (PB.code.ITEM_SELL_BATCH_S.GetHashCode ().ToString (), OnRequestSellFinished);

		GameEventMgr.Instance.AddListener<ProtocolMessage> (PB.code.ITEM_USE_C.GetHashCode ().ToString (), OnUseItemFinished);
		GameEventMgr.Instance.AddListener<ProtocolMessage> (PB.code.ITEM_USE_S.GetHashCode ().ToString (), OnUseItemFinished);
	}

	void UnBindListener()
	{
		GameEventMgr.Instance.RemoveListener (GameEventList.BuyItemFinished, RefreshBag);
		GameEventMgr.Instance.RemoveListener (GameEventList.OpenBoxFinished, OnOpenBoxFinished);
		GameEventMgr.Instance.RemoveListener<ProtocolMessage>(PB.code.PLAYER_REWARD_S.GetHashCode().ToString(), OnReward);
		
		GameEventMgr.Instance.RemoveListener<ProtocolMessage> (PB.code.ITEM_SELL_BATCH_C.GetHashCode ().ToString (), OnRequestSellFinished);
		GameEventMgr.Instance.RemoveListener<ProtocolMessage> (PB.code.ITEM_SELL_BATCH_S.GetHashCode ().ToString (), OnRequestSellFinished);

		GameEventMgr.Instance.RemoveListener<ProtocolMessage> (PB.code.ITEM_USE_C.GetHashCode ().ToString (), OnUseItemFinished);
		GameEventMgr.Instance.RemoveListener<ProtocolMessage> (PB.code.ITEM_USE_S.GetHashCode ().ToString (), OnUseItemFinished);
	}

    void CloseBagButtonDown(GameObject go)
    {
        RequestCloseUi();
    }

	public void OnTabButtonChanged (int index, TabButtonGroup tab)
	{
		if ((int)curBagType == index)
			return;

		curBagType = (BagType)index;
		BagStateAttr = BagState.Norml;
		RefreshBag ();
	}

	private void RefreshBag()
	{
		isCanSell = false;
		cancelSellButton.gameObject.SetActive (curBagState == BagState.Sell);
		conformSellButton.gameObject.SetActive (curBagState == BagState.Sell);
		sellButton.gameObject.SetActive (curBagState == BagState.Norml);

		scrollView.ClearAllElement ();

		List<ItemData> items = BagDataMgr.Instance.GetBagItemsWithType (curBagType,curBagState);
		ItemData subItemData;
		for (int i =0; i< items.Count; ++i)
		{
			subItemData = items[i];
 			BagItem subBagItme = BagItem.Create(subItemData,curBagType,curBagState);
		
			scrollView.AddElement(subBagItme.gameObject);
			if(!isCanSell)
			{
				ItemStaticData stData = StaticDataMgr.Instance.GetItemData(subItemData.itemId);
				if(stData != null && stData.sellPrice > 0)
				{
					isCanSell = true;
				}
			}
		}

		ScrollRect sr = scrollView.GetComponent<ScrollRect> ();
		if (sr != null) 
		{
			sr.horizontalNormalizedPosition = 0.0f;
		}
	}

	void OnSellCancleClick(GameObject go)
	{
		BagStateAttr = BagState.Norml;
	}

	void OnSellConformClick(GameObject go)
	{
		if (sellItemsDic.Count < 1) 
		{
			//todo:msg
			Logger.LogError("未选择出售物品....");
			UIIm.Instance.ShowSystemHints(StaticDataMgr.Instance.GetTextByID("bag_record_006"),
			                              (int)PB.ImType.PROMPT);
			return;
		}
		int sellTotal = 0;
		foreach (var subSellItem in sellItemsDic) 
		{
			string itemId = subSellItem.Key;
			int count = subSellItem.Value;
			ItemStaticData stData = StaticDataMgr.Instance.GetItemData(itemId);

			sellTotal += stData.sellPrice * count;
		}

		SellConformDlg.OpenWith (sellTotal, OnSellConformed);
	}

	void OnSellConformed(MsgBox.PrompButtonClick click)
	{
		if (click == MsgBox.PrompButtonClick.OK)
		{
			Logger.LogError("sell.");
		}
		RequestSell ();
	}

	void RequestSell()
	{
		PB.HSItemSellBatch param = new PB.HSItemSellBatch ();
		foreach (var subItem in sellItemsDic) 
		{
			PB.ItemSell itemsell = new PB.ItemSell();
			itemsell.itemId = subItem.Key;
			itemsell.count = subItem.Value;
			param.items.Add(itemsell);
		}
		GameApp.Instance.netManager.SendMessage (PB.code.ITEM_SELL_BATCH_C.GetHashCode (), param);
	}

	void OnRequestSellFinished(ProtocolMessage msg)
	{
		UINetRequest.Close ();
		
		if (msg.GetMessageType() == (int)PB.sys.ERROR_CODE)
		{
			Logger.LogError("sell items Error.....");
			return;
		}
		BagStateAttr = BagState.Norml;
		PB.HSItemSellBatchRet sellReturn = msg.GetProtocolBody<PB.HSItemSellBatchRet> ();
		RefreshBag ();
	}


	void OnSellClick(GameObject go)
	{
		if (isCanSell)
		{
			BagStateAttr = BagState.Sell;
		}
		else 
		{
			UIIm.Instance.ShowSystemHints(StaticDataMgr.Instance.GetTextByID("bag_record_006"),
			                              (int)PB.ImType.PROMPT);
		}
	}

	public	void UpdateSellItems(string itemId, int itemCount)
	{
		if (itemCount == 0 )
		{
			if(sellItemsDic.ContainsKey (itemId))
			{
				sellItemsDic.Remove(itemId);
			}
			return;
		}
		if (sellItemsDic.ContainsKey (itemId))
		{
			sellItemsDic[itemId] = itemCount;
		}
		else 
		{
			sellItemsDic.Add(itemId,itemCount);
		}
	}

	void OnReward(ProtocolMessage msg)
	{
		PB.HSRewardInfo reward = msg.GetProtocolBody<PB.HSRewardInfo>();
		if (reward == null ||reward.hsCode != PB.code.ITEM_BOX_USE_BATCH_C.GetHashCode())
			return;
		listRewardForBox.Add (reward);
	}

	void	OnOpenBoxFinished()
	{
		RefreshBag ();
		//显示结果
		OpenBaoxiangResult.OpenWith (listRewardForBox);
	}

	public	void	UseItem(ItemData itemData)
	{
		curUseItemData = itemData;
		PB.HSItemUse param = new PB.HSItemUse ();
		param.itemId = itemData.itemId;
		param.itemCount = 1;
		GameApp.Instance.netManager.SendMessage (PB.code.ITEM_USE_C.GetHashCode (), param);
	}

	void	OnUseItemFinished(ProtocolMessage msg)
	{
		UINetRequest.Close ();

		ItemStaticData stData = StaticDataMgr.Instance.GetItemData (curUseItemData.itemId);
		bool isPilaoyao = (stData.addAttrType == 7);
		
		if (msg.GetMessageType() == (int)PB.sys.ERROR_CODE)
		{
			PB.HSErrorCode error = msg.GetProtocolBody<PB.HSErrorCode> ();
			if(error.errCode == (int)PB.itemError.ITEM_EXP_LEFT_TIMES)
			{
				UIIm.Instance.ShowSystemHints (StaticDataMgr.Instance.GetTextByID("bag_record_003"),
				                               (int)PB.ImType.PROMPT);
			}
            else if (error.errCode == (int)PB.itemError.ITEM_USE_COUNT)
            {
                UIIm.Instance.ShowSystemHints(StaticDataMgr.Instance.GetTextByID("bag_record_002"),
                                               (int)PB.ImType.PROMPT);
            }
            else if (error.errCode == (int)PB.PlayerError.FATIGUE_LIMIT)
            {
                UIIm.Instance.ShowSystemHints(StaticDataMgr.Instance.GetTextByID("im_recordhuoli_002"), (int)PB.ImType.PROMPT);
            }
          
			return;
		}
		string succMsg = "bag_record_004";
		if(!isPilaoyao)
		{
            UIIm.Instance.ShowSystemHints(StaticDataMgr.Instance.GetTextByID(succMsg), (int)PB.ImType.PROMPT);
            //succMsg = "bag_record_001";
        }
		RefreshBag ();
	}
}

