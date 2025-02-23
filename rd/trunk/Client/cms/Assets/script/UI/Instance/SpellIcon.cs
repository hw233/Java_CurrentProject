﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpellIcon : MonoBehaviour
{
	public 	Image	iconImage;
	public	Image	normalFrame;
	public	Image	maskFrame;
	public	Button	iconButton;

	public	int	level = 1;
	public	string	spellId;

	public static SpellIcon	CreateWith(Transform parentTrans,float scaleRat = 1.0f)
	{
		GameObject go = ResourceMgr.Instance.LoadAsset ("spellIcon");
		SpellIcon icon =  go.GetComponent<SpellIcon>();

		go.transform.SetParent (parentTrans, false);
		go.transform.localScale = new Vector3 (scaleRat, scaleRat, scaleRat);

		return icon;
	}

	// Use this for initialization
	void Start ()
	{
        SetMask(false);
	}

	public	void SetData(int ilevel,string spellid)
	{
		level = ilevel;
		spellId = spellid;

		SpellProtoType spell = StaticDataMgr.Instance.GetSpellProtoData (spellid);
		
		Sprite iconSp = ResourceMgr.Instance.LoadAssetType<Sprite> (spell.icon) as Sprite;
		if (null != iconSp)
		{
			iconImage.sprite = iconSp;
		}
	}

	public	void SetMask(bool bMask)
	{
		maskFrame.gameObject.SetActive (bMask);
		normalFrame.gameObject.SetActive (!bMask);
	}

	public	void SetBoss()
	{
		Sprite iconSp = ResourceMgr.Instance.LoadAssetType<Sprite> ("bossDazhaoIcon") as Sprite;
		if (null != iconSp)
		{
			iconImage.sprite = iconSp;
		}
	}
}
