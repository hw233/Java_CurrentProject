﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;
using System.IO;
using BestHTTP;

public class UIUpdate : MonoBehaviour ,IPointerUpHandler
{
	private bool userCanRequestUpdate = false;

	public	Text	msgText;
    public RectTransform progressRt;
    public Text progressRateText;
    public Text progressMsgText;

    private float progressSizeDelthaX;
    private float progressSizeDelthaY;

    private int zipTotal;
    private int zipCurrent;
    
    StreamFileDownload filedownload;
    ArrayList downloadVersionList;
    string resourceServer;
    private string downloadZipFileName;
    private int downloadVersionId;

    // Use this for initialization
    IEnumerator Start () 
	{
        msgText.text = "";
        progressRateText.text = "";
        progressMsgText.text = "";
        progressSizeDelthaX = progressRt.sizeDelta.x;
        progressSizeDelthaY = progressRt.sizeDelta.y;
        progressRt.sizeDelta = new Vector2(0, progressSizeDelthaY);

        if (null == filedownload)
        {
            filedownload = gameObject.AddComponent<StreamFileDownload>();
        }

		yield return new WaitForEndOfFrame ();
		CheckExtractResource ();
	}

	/// <summary>
	/// 释放资源
	/// </summary>
	public void CheckExtractResource()
	{
        bool isExists = UpdateHelpter.IsResouceExtracted();
		//
		if (Const.DebugMode || isExists)
		{
			StartCoroutine(OnUpdateResource());
			return;   //文件已经解压过了，自己可添加检查文件列表逻辑
		}
		StartCoroutine(OnExtractResource());    //启动释放协成 
	}
	
	IEnumerator OnExtractResource()
	{
		string dataPath = Util.ResPath;  //数据目录
		string resPath = Util.AppContentPath(); //游戏包资源目录
		Logger.Log(Util.ResPath);
		
		if (Directory.Exists(dataPath))
			Directory.Delete(dataPath, true);
		Directory.CreateDirectory(dataPath);
		
		string infile = Path.Combine(resPath, "files.txt");
		string outfile = Path.Combine(dataPath, "files.txt");
		if (File.Exists(outfile))
			File.Delete(outfile);
		
		Logger.Log("正在解包文件:>files.txt\n");
		if (Application.platform == RuntimePlatform.Android)
		{
			WWW www = new WWW(infile);
			yield return www;
			
			if (www.isDone)
			{
				File.WriteAllBytes(outfile, www.bytes);
			}
			yield return 0;
		}
		else File.Copy(infile, outfile, true);
		yield return new WaitForEndOfFrame();
		
		//释放所有文件到数据目录
		string[] files = File.ReadAllLines(outfile);
		int count = 1;
		foreach (var file in files)
		{
			infile = resPath + file;
			outfile = dataPath + file;
			Logger.Log("正在解包文件:>" + file);
			msgText.text = string.Format("正在解包 第{0}个，共{1}个",count ++,files.Length);
			string dir = Path.GetDirectoryName(outfile);
			if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
			
			if (Application.platform == RuntimePlatform.Android)
			{
				WWW www = new WWW(infile);
				yield return www;
				
				if (www.isDone)
				{
					//Logger.LogError(infile + " ## " + www.bytes.Length + " ## " + outfile);
					File.WriteAllBytes(outfile, www.bytes);
				}
				yield return null;
			}
			else
				File.Copy(infile, outfile, true);
			
			yield return new WaitForEndOfFrame();
		}
        //资源抽取完成，写入客户端初始时的版本号
        UpdateHelpter.SetResouceCode(Const.resouceCode);

		Logger.Log("解包完成!!!");
		msgText.text = "资源包抽取抽取完成";
		yield return new WaitForSeconds(0.1f);
		
		//释放完成，开始启动更新资源
		StartCoroutine(OnUpdateResource());
	}

	/// <summary>
	/// 启动更新下载
	/// </summary>
	IEnumerator OnUpdateResource()
	{
		//不更新模式下直接添加ResourceManager
		if (!Const.UpdateMode)
		{
			OnUpdateFinished();
			yield break;
		}
		
		HTTPRequest httpRquest = null;
		string dataPath = Util.ResPath;  //数据目录
		string url = Const.UpdateUrl;
		
	
		httpRquest = new HTTPRequest (new Uri(url),HTTPMethods.Post);
		httpRquest.AddField ("channel", Const.channel);
		httpRquest.AddField ("vid", UpdateHelpter.GetResouceCode().ToString());// 
        httpRquest.AddField ("platform", Const.platform);
		httpRquest.Send ();
		yield return StartCoroutine (httpRquest);
		if (httpRquest.Response== null || !httpRquest.Response.IsSuccess)
		{
			OnUpdateFailed(string.Empty);
			yield break;
		}
		//将version list文件解析为列表
		string remoteVersion = httpRquest.Response.DataAsText;

	   Hashtable ht = MiniJsonExtensions.hashtableFromJson (remoteVersion);
		if (null == ht )
		{
			Logger.LogError("更新错误，解析数据出错: " + remoteVersion);
			OnUpdateFailed("");
			yield break;
		}
        int status = int.Parse(ht["status"].ToString());
        if (status != 0)
		{
            string msg = "";
            if(status == 1)
            {
                msg = "版本过期，请更新程序到最新版本.";
            }
            else
            {
                msg = string.Format("数据错误 status = {0}", status);
            }
			Logger.LogError(msg);
			OnUpdateFailed(msg);
			yield break;
		}

		resourceServer = ht ["resourceServer"] as String;
        downloadVersionList = ht ["resources"] as ArrayList;
        zipTotal = 0;
        zipCurrent = 1;
        if(null != downloadVersionList)
        {
            zipTotal = downloadVersionList.Count;
        }

        StartDownLoadResouce();
	}

    void StartDownLoadResouce()
    {
        if (zipCurrent > zipTotal)
        {
            OnUpdateFinished();
            return;
        }

        Hashtable subVersion = downloadVersionList[zipCurrent - 1] as Hashtable;

        downloadVersionId = int.Parse(subVersion["vid"].ToString());
        string verZip = string.Format("{0}.zip", subVersion["resourceName"].ToString());
        float fileSizeKb = float.Parse(subVersion["resourceSize"].ToString());
        downloadZipFileName = Path.Combine(Util.ResPath, verZip);
        filedownload.RequestDownload(downloadZipFileName, string.Format("{0}/{1}", resourceServer, verZip), OnDownloadProgress, OnDownloadFinish);

    }

    void OnDownloadProgress(HTTPRequest originalRequest, int downloaded, int downloadLength)
    {
        if(downloadLength > 0)
        {
            float rate = downloaded / (float)downloadLength;
            progressRateText.text = string.Format("{0:D}%", (int)(rate * 100));
            float totalSizeM = downloadLength / (1024.0f*1024.0f);
            progressMsgText.text = string.Format("共有{0}个更新，当前第{1}个({2:F2}M)", zipTotal, zipCurrent, totalSizeM);
            progressRt.sizeDelta = new Vector2(rate * progressSizeDelthaX, progressSizeDelthaY);
        }
        else
        {
            Debug.LogErrorFormat("r.... {0} ---{1}", downloaded, downloadLength);
        }
    }
    void OnDownloadFinish(int state ,string errmsg)
    {
        if (state == 0)
        {
            //unzip
            //解压zip，写入本地目录
            Util.UnZipFromFile(downloadZipFileName, Util.ResPath, false);
            //更新成功后写入客户端的version文件
            UpdateHelpter.SetResouceCode(downloadVersionId);

            zipCurrent++;
            StartDownLoadResouce();
        }
        else
        {
            Logger.LogError(errmsg);
        }
    }

    void OnUpdateFailed(string msg)
	{
		msgText.text = msg;
		userCanRequestUpdate = true;
	}

	void OnUpdateFinished()
	{	
		InitMgr ();
	}

	void InitMgr()
	{
		msgText.text = "正在初始化组件...";
		GameEventMgr.Instance.Init();
		ResourceMgr.Instance.Init();
		
		StaticDataMgr.Instance.Init();
		ObjectDataMgr.Instance.Init();
		GameDataMgr.Instance.Init();
		StatisticsDataMgr.Instance.Init();
		LayerConst.Init ();

		
		UIMgr.Instance.Init();
		GameMain.Instance.Init();
		SpellService.Instance.Init();
		ActorEventService.Instance.Init();
		GameSpeedService.Instance.Init();

        ResourceMgr.Instance.LoadLevelAsyn("firstScene", false);
        UIMgr.Instance.ClearUILayerList();
        Destroy (gameObject);
	}

	//--------------Event------------------------
	public void OnPointerUp (PointerEventData eventData)
	{
		userCanRequestUpdate = false;
		StartCoroutine(OnUpdateResource());
	}
}
