﻿using UnityEngine;
using UnityEngine.UI;
//---------------------------------------------------------------------------------------------
public class Fps : MonoBehaviour
{
    public Text m_FPS;
    float _updateInterval = 0.5f;//设定更新帧率的时间间隔为1秒
    float _accum = .0f;//累积时间
    int _frames = 0;//在_updateInterval时间内运行了多少帧
    float _timeLeft;

    void Start()
    {
        if (!m_FPS)
        {
            enabled = false;
            return;
        }
        _timeLeft = _updateInterval;
    }

    // Update is called once per frame
    void Update()
    {
        _timeLeft -= Time.deltaTime;
        //Time.timeScale可以控制Update 和LateUpdate 的执行速度,
        //Time.deltaTime是以秒计算，完成最后一帧的时间
        //相除即可得到相应的一帧所用的时间
        _accum += Time.timeScale / Time.deltaTime;
        ++_frames;//帧数

        if (_timeLeft <= 0)
        {
            float fps = _accum / _frames;
            //Logger.Log(_accum + "__" + _frames);
            string fpsFormat = System.String.Format("{0:F2}FPS", fps);//保留两位小数
            m_FPS.text = fpsFormat;

            _timeLeft = _updateInterval;
            _accum = .0f;
            _frames = 0;
        }
    }
}
//---------------------------------------------------------------------------------------------