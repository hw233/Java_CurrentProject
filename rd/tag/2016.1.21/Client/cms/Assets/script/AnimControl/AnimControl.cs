﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class AnimControl : MonoBehaviour 
{
    private AnimatorOverrideController actualControler;

    private Animator animator;
    //param hash
    private int hashSiwang;
    private int hashPaobu;
    private int hashShengli;
    private int hashDazhao;
	private int hashWugong;
	private int hashFagong;
    private int hashToulan;
    private int hashShoukong;
	private int hashShouji;
	private int hashFangyu;
	private int hashDazhaoxuanyao;
    private int hashChuchang;
    //state hash
    private int hashSiwangState;
    private int hashShoukongState;
    private int hashShengliState;

    //public delegate void AnimEvent(string clipName);
    //public static event AnimEvent OnAnimationBegin;
    //public static event AnimEvent OnAnimationEnd;

    //---------------------------------------------------------------------------------------------
    void Awake()
    {
        animator = gameObject.GetComponent<Animator>();
        RuntimeAnimatorController curController = animator.runtimeAnimatorController;
        actualControler = new AnimatorOverrideController();

        actualControler.runtimeAnimatorController = curController;
        animator.runtimeAnimatorController = actualControler;

        hashSiwang = Animator.StringToHash("siwang");
        hashPaobu = Animator.StringToHash("paobu");
        hashShengli = Animator.StringToHash("shengli");
		hashDazhao = Animator.StringToHash("dazhao");
		hashFagong = Animator.StringToHash("fagong");
        hashWugong = Animator.StringToHash("wugong");
        hashToulan = Animator.StringToHash("toulan");
        hashShoukong = Animator.StringToHash("shoukong");
		hashShouji = Animator.StringToHash("shouji");
		hashFangyu = Animator.StringToHash("fangyu");
        hashChuchang = Animator.StringToHash("chuchang");
		hashDazhaoxuanyao = Animator.StringToHash("dazhaoxuanyao");
        hashSiwangState = Animator.StringToHash("Base Layer.siwang");
        hashShoukongState = Animator.StringToHash("Base Layer.shoukong");
        hashShengliState = Animator.StringToHash("Base Layer.shengli");
    }
    //---------------------------------------------------------------------------------------------
    void Update()
    {
        AnimatorClipInfo[] aniList = animator.GetCurrentAnimatorClipInfo(0);
        for (int i = 0; i < aniList.Length; ++i)
        {
            if (aniList[i].clip.name.Contains("daiji") || aniList[i].clip.name.Contains("dazhaoxuanyao"))
            {
                animator.speed = 1.0f / Time.timeScale;
                //animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                return;
            }
        }
        animator.speed = 1.0f;
        //animator.updateMode = AnimatorUpdateMode.Normal;
    }
    //---------------------------------------------------------------------------------------------
    public void SetController(string controllerName)
    {
        RuntimeAnimatorController curController = ResourceMgr.Instance.LoadAssetType<RuntimeAnimatorController>(controllerName);
        //RuntimeAnimatorController curController = (RuntimeAnimatorController)Resources.Load(controllerName);
        if (curController != null)
        {
            actualControler.runtimeAnimatorController = curController;
        }
    }
    //---------------------------------------------------------------------------------------------
    public void Pause()
    {
        animator.speed = 0;
    }
    //---------------------------------------------------------------------------------------------
    public void SetSpeed(float speed)
    {
        animator.speed = speed;
    }
    //---------------------------------------------------------------------------------------------
    public void SetFloat(string name, float value)
    {
        //TODO: use hash
        animator.SetFloat(name, value);
    }
    //---------------------------------------------------------------------------------------------
    public void SetBool(string name, bool value)
    {
        //TODO: find why
        if (name == "fangyu")
        {
            animator.SetBool(hashFangyu, value);
        }
        else
        {
            animator.SetBool(name, value);
        }
    }
    //---------------------------------------------------------------------------------------------
    public void SetInt(string name, int value)
    {
        animator.SetInteger(name, value);
    }
    //---------------------------------------------------------------------------------------------
    //call back functions
    //---------------------------------------------------------------------------------------------
    public void OnDeadBegin()
    {
    }
    //---------------------------------------------------------------------------------------------
    public void OnDeadEnd()
    {
        animator.SetBool(hashSiwang, false);
    }
    //---------------------------------------------------------------------------------------------
    public void OnRunBegin()
    {
    }
    //---------------------------------------------------------------------------------------------
    public void OnRunEnd()
    {
        animator.SetBool(hashPaobu, false);
	}
	//---------------------------------------------------------------------------------------------
	public void OnFangyuEnd()
	{
		animator.SetBool(hashFangyu, false);
	}
    //---------------------------------------------------------------------------------------------
    public void OnWinBegin()
    {
    
    }
    //---------------------------------------------------------------------------------------------
    public void OnWinEnd()
    {
        animator.SetBool(hashShengli, false);
    }
    //---------------------------------------------------------------------------------------------
    public void OnDaZhaoBegin()
    {

    }
    //---------------------------------------------------------------------------------------------
    public void OnDaZhaoEnd()
    {
        animator.SetBool(hashDazhao, false);
    }
	//---------------------------------------------------------------------------------------------
	public void OnDaZhaoxuanyaoEnd()
	{
		animator.SetBool(hashDazhaoxuanyao, false);
	}
    //---------------------------------------------------------------------------------------------
    public void OnDShoujiBegin()
    {

    }
    //---------------------------------------------------------------------------------------------
    public void OnShoujiEnd()
    {
        animator.SetBool(hashShouji, false);
    }
    //---------------------------------------------------------------------------------------------
    public void OnWuGongBegin()
    {
    
    }
    //---------------------------------------------------------------------------------------------
    public void OnWuGongEnd()
    {
        animator.SetBool(hashWugong, false);
	}
	//---------------------------------------------------------------------------------------------
	public void OnFaGongBegin()
	{
		
	}
	//---------------------------------------------------------------------------------------------
	public void OnFaGongEnd()
	{
		animator.SetBool(hashFagong, false);
	}
    //---------------------------------------------------------------------------------------------
    public void OnLazyBegin()
    {

    }
    //---------------------------------------------------------------------------------------------
    public void OnLazyEnd()
    {
        animator.SetBool(hashToulan, false);
    }
    //---------------------------------------------------------------------------------------------
    public void OnStunBegin()
    {

    }
    //---------------------------------------------------------------------------------------------
    public void OnStunEnd()
    {
        animator.SetBool(hashFagong, false);
        animator.SetBool(hashWugong, false);
    }
    //---------------------------------------------------------------------------------------------
    //-------------------------------------------------------+--------------------------------------
    public void OnNormalEnd(string str)
    {
		int hashstr = Animator.StringToHash (str);
		animator.SetBool(hashstr, false);
    }
    //---------------------------------------------------------------------------------------------
    public void OnChuChangEnd()
    {
        animator.SetBool(hashChuchang, false);
    }
}
