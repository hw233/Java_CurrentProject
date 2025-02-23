﻿using UnityEngine;  
using UnityEngine.UI;  
using UnityEngine.EventSystems;  
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public enum MirrorState
{
    CannotUse = 0,//不可使用
    Recover,    //正在恢复
    Consum  //正在消耗
}
public class MirrorDray : MonoBehaviour,IPointerDownHandler, IPointerUpHandler, IDragHandler,IPointerClickHandler
{

	float	m_MinPosX = 0f;
	float 	m_MaxPosX = 0f;
	float 	m_MinPosY = 0f;
	float	m_MaxposY = 0f;

	RectTransform rectTrans;

	//Tweener  mirrorTween = null;

	Dictionary<MirrorTarget, float> lastFindWeakpoint = new Dictionary<MirrorTarget, float> ();

	MirrorRaycast m_MirrorRaycast = null;
	List<MirrorTarget> newFindTargetList =new List<MirrorTarget>();
	List<MirrorTarget> finishFindTargett = new List<MirrorTarget>();
	List<MirrorTarget> outFindTarget = new List<MirrorTarget> ();
	//Image mirrorUi;
	//Image MirrorDragImage;
	GameObject mirrorParticle;
	Animator mirrorUIAnimator;
    Image mirrorEnegyImage;


	bool isDragging = false;
	bool	isShowMirrorExitEffect = false;
	bool 	isCanShowMirrorExitEffect = false;

	int mirrorStateHash = -1;

	ParticleEffect mirrorExitEffect = null;

	public void Init(GameObject mirrorUi)
	{
		mirrorStateHash = Animator.StringToHash ("mirrorState");
		if (null == m_MirrorRaycast)
		{
			m_MirrorRaycast = gameObject.AddComponent<MirrorRaycast> ();
		}
		rectTrans = transform as RectTransform;

		mirrorUIAnimator = mirrorUi.GetComponent < Animator> ();
        mirrorEnegyImage = Util.FindChildByName(mirrorUi, "enegyImage").GetComponent<Image>();

        //MirrorDragImage = Util.FindChildByName (gameObject, "MirrorDragImage").GetComponent<Image> ();
        mirrorParticle = Util.FindChildByName (gameObject, "zhaoyaojing");
        //baozha_fire
		if (null == mirrorExitEffect)
		{
			mirrorExitEffect = ParticleEffect.CreateEffect (BattleController.Instance.GetUIBattle ().publicTopGroup, "zhaoyaojingbaozhatexiao");
			mirrorExitEffect.gameObject.SetActive (false);
		}

		ResetMirror ();
	}

    public  void   UpdateMirrorEnegy()
    {
        if (null != mirrorEnegyImage)
        {
            float enegyRatio = BattleController.Instance.MirrorEnegyAttr / GameConfig.Instance.MirrorMaxEnegy;
            mirrorEnegyImage.fillAmount = enegyRatio;

            if(Mathf.Abs( BattleController.Instance.MirrorEnegyAttr - GameConfig.Instance.MirrorMaxEnegy) < 0.001f)
            {
                
                int state = mirrorUIAnimator.GetInteger(mirrorStateHash);
                if(state != 3)
                {
                    mirrorUIAnimator.SetInteger(mirrorStateHash, 3);
                }
            }
        }
    }

	public void ResetMirror()
	{
		//MirrorDragImage.gameObject.SetActive (false);
	//	mirrorTween = MirrorDragImage.DOFade (0, 1.5f).OnComplete (OnResetMiirror);
		mirrorParticle.SetActive (false);
		//	mirrorUi.gameObject.SetActive (true);
		mirrorUIAnimator.SetInteger (mirrorStateHash, 2);
		OnResetMiirror ();
	}

	void OnResetMiirror()
	{
		RectTransform mirrorUiRt = mirrorUIAnimator.transform.parent.transform as RectTransform;
		
		Vector3 screenPos = UICamera.Instance.CameraAttr.WorldToScreenPoint (mirrorUiRt.position);
		screenPos /= UIMgr.Instance.CanvasAttr.scaleFactor ;
		
		screenPos.x += (rectTrans.pivot.x - 0.5f) * rectTrans.sizeDelta.x;
		screenPos.y += (rectTrans.pivot.y - 0.5f) * rectTrans.sizeDelta.y;
		rectTrans.anchoredPosition = screenPos ;
	}


	void Awake ()
	{  
		//RectTransform parentTransform = transform.parent as RectTransform;
		float rootWidth = Screen.width /UIMgr.Instance.CanvasAttr.scaleFactor ;
		float rootHeight =   Screen.height/UIMgr.Instance.CanvasAttr.scaleFactor;
		
		//Logger.LogError ("Screen.Width = " + Screen.width + "  Screen.height = " + Screen.height + "scaleFactor = " + UIMgr.Instance.CanvasAttr.scaleFactor );
		RectTransform thisTransform = transform as RectTransform;
		float myWith = thisTransform.rect.width;
		float myHeigth = thisTransform.rect.height;

		Vector2 pivot = thisTransform.pivot;
		m_MinPosX = myWith * pivot.x;
		m_MaxPosX = rootWidth - myWith*(1 - pivot.x)  ;
		m_MinPosY = myWith * pivot.y;
		m_MaxposY = rootHeight - myHeigth * (1-pivot.y);
	}  
    
    void OnEnable()
    {
        if(null != mirrorUIAnimator)
            mirrorUIAnimator.SetInteger(mirrorStateHash, 2);
    }


	// 鼠标按下  
	public void OnPointerDown (PointerEventData data)
    {
        if (BattleController.Instance.processStart == false)
            return;

        if(BattleController.Instance.MirrorEnegyAttr < GameConfig.Instance.UseMirrorMinEnegy)
        {
            UIIm.Instance.ShowSystemHints(StaticDataMgr.Instance.GetTextByID("battle_zhaoyaojing_001"), (int)PB.ImType.PROMPT);
            return;
        }
		//if (mirrorTween != null &&
		//    mirrorTween.IsPlaying())
		//{
		//	mirrorTween.Kill(true);
		//}
		isShowMirrorExitEffect = false;
		isCanShowMirrorExitEffect = true;
		isDragging = false;
		//MirrorDragImage.gameObject.SetActive (true);
		//MirrorDragImage.DOFade (1.0f, 0.5f);
		mirrorParticle.SetActive (true);

		//mirrorUi.gameObject.SetActive (false);
		mirrorUIAnimator.SetInteger (mirrorStateHash, 1);
		rectTrans.anchoredPosition = GetMirrorScreenPosition (Input.mousePosition);	
		OnSetMirrorModeState (true,false);
	}  

	//鼠标抬起
	public void OnPointerUp (PointerEventData eventData)
	{
		OnSetMirrorModeState (false,true);
		isCanShowMirrorExitEffect = false;
	}
	// 拖动  
	public void OnDrag (PointerEventData data)
	{  
		isDragging = true;
		
		rectTrans.anchoredPosition = GetMirrorScreenPosition (Input.mousePosition);	
	}  
	//点击
	public void OnPointerClick (PointerEventData eventData)
	{
		if (!isDragging) 
		{
			GameEventMgr.Instance.FireEvent<Vector3>(GameEventList.MirrorClicked,Input.mousePosition);
		}
	}

	Vector2 GetMirrorScreenPosition(Vector3 mousePosition)
	{
		Vector2 newPos = new Vector2 (mousePosition.x / UIMgr.Instance.CanvasAttr.scaleFactor , mousePosition.y / UIMgr.Instance.CanvasAttr.scaleFactor);

		/*if (newPos.x < m_MinPosX) {
			newPos.x = m_MinPosX;
		}
		if (newPos.x > m_MaxPosX) {
			newPos.x = m_MaxPosX;
		}
		if (newPos.y < m_MinPosY) {
			newPos.y = m_MinPosY;
		}
		if (newPos.y > m_MaxposY) {
			newPos.y = m_MaxposY;
		}
		*/
		return newPos;
	}

	public void	OnSetMirrorModeState(bool isMirror, bool isMirrExitEffect)
	{
		if (isMirror) 
		{
            mirrorExitEffect.gameObject.SetActive(false);
			StartRayCast();

            BattleController.Instance.mirrorState = MirrorState.Consum;
            BattleController.Instance.beginChangeEnegyTime = GameTimeMgr.Instance.TimeStampAsMilliseconds();
        } 
		else 
		{
            isShowMirrorExitEffect = isMirrExitEffect;
            BattleController.Instance.mirrorState = MirrorState.Recover;
            BattleController.Instance.beginChangeEnegyTime = GameTimeMgr.Instance.TimeStampAsMilliseconds();
            StopRayCast();
			if(isShowMirrorExitEffect && isCanShowMirrorExitEffect)
			{
				isShowMirrorExitEffect  = false;
				mirrorExitEffect.gameObject.SetActive(true);
				RectTransform rt = mirrorExitEffect.transform as RectTransform;
				//rt.anchoredPosition = new Vector2(300,300);
				if(rt != null )
				{
					Vector3 mirrorScreenPos = UIUtil.GetSpacePos(rectTrans,UIMgr.Instance.CanvasAttr,UICamera.Instance.CameraAttr);
					mirrorScreenPos.x -= (rectTrans.pivot.x -0.5f)*rectTrans.sizeDelta.x;
					mirrorScreenPos.y -= (rectTrans.pivot.y -0.5f)*rectTrans.sizeDelta.y;
					
					float fscale = UIMgr.Instance.CanvasAttr.scaleFactor;
					rt.anchoredPosition = new Vector2(mirrorScreenPos.x/fscale,mirrorScreenPos.y/fscale);
				}
			}
			else
			{
				mirrorExitEffect.gameObject.SetActive(false);
			}

			isCanShowMirrorExitEffect = false;
			ResetMirror();
		}
	}

	void StartRayCast()
	{
		lastFindWeakpoint.Clear ();
		StartCoroutine ("weakPointRayCastCo");
	}

	void StopRayCast()
	{
		StopCoroutine("weakPointRayCastCo");

		List<MirrorTarget> listTarget = new List<MirrorTarget> (lastFindWeakpoint.Keys);
		if (null !=listTarget && listTarget.Count > 0) 
		{
			GameEventMgr.Instance.FireEvent<List<MirrorTarget>>(GameEventList.MirrorOutWeakPoint,listTarget);
		}
		//GameEventMgr.Instance.FireEvent(GameEventList.HideFindMonsterInfo);
	}

	IEnumerator weakPointRayCastCo()
	{
		List<MirrorTarget> listFindTarget = null;
		while (true)
		{
			newFindTargetList.Clear();
			outFindTarget.Clear();
			finishFindTargett.Clear();

			Vector3 mirrorScreenPos = UIUtil.GetSpacePos(mirrorParticle.transform as RectTransform,UIMgr.Instance.CanvasAttr,UICamera.Instance.CameraAttr);

			listFindTarget = m_MirrorRaycast.WeakpointRayCast (mirrorScreenPos);
			if(listFindTarget.Count > 0)
			{
				MirrorTarget subTarget = null;
				for(int i =0 ; i< listFindTarget.Count; ++i)
				{
					subTarget = listFindTarget[i];
					WeakPointRuntimeData wpRuntime = subTarget.WpRuntimeData;

					if(lastFindWeakpoint.ContainsKey(subTarget))
					{
						lastFindWeakpoint[subTarget] += Time.deltaTime;
						if(lastFindWeakpoint[subTarget] > GameConfig.Instance.FindWeakPointFinishedNeedTime)
						{
							finishFindTargett.Add(subTarget);
						}
					}
					else
					{
						if(wpRuntime.IsFind)
						{
							lastFindWeakpoint.Add(subTarget,0.0f);
							finishFindTargett.Add(subTarget);
						}
						else
						{
							lastFindWeakpoint.Add(subTarget,0.0f);
							newFindTargetList.Add(subTarget);
						}
					}
				}
			}
			if(newFindTargetList.Count > 0)
			{
				GameEventMgr.Instance.FireEvent<List<MirrorTarget>>(GameEventList.FindWeakPoint,newFindTargetList);
			}

			if(finishFindTargett.Count > 0)
			{
				GameEventMgr.Instance.FireEvent<List<MirrorTarget>>(GameEventList.FindFinishedWeakPoint,finishFindTargett);
				//GameEventMgr.Instance.FireEvent<List<MirrorTarget>>(GameEventList.ShowFindMonsterInfo, finishFindTargett);
			}
			else
			{
				//GameEventMgr.Instance.FireEvent(GameEventList.HideFindMonsterInfo);
			}

			List<MirrorTarget> lastFindKeys =  new List<MirrorTarget>( lastFindWeakpoint.Keys);
			if(null!=lastFindKeys && lastFindKeys.Count > 0)
			{
				for(int i =0 ;i < lastFindKeys.Count;++i)
				{
					if(!listFindTarget.Contains(lastFindKeys[i]))
					{
						outFindTarget.Add(lastFindKeys[i]);
						lastFindWeakpoint.Remove(lastFindKeys[i]);
					}
				}
			}
			if(outFindTarget.Count> 0)
			{
				GameEventMgr.Instance.FireEvent<List<MirrorTarget>>(GameEventList.MirrorOutWeakPoint,outFindTarget);
			}

			//Logger.LogError("finding....."+ transform.position.x);
			yield return new WaitForFixedUpdate();
		}
	}

}
