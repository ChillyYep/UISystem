using GameBase.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestPage1 : Page
{
    public Image image;
    public Text titleTxt;
    protected string titleName;
    protected override void DoOpen()
    {
        base.DoOpen();
        var localParams = Params as Tuple<string>;
        titleName = localParams.Item1;
        //titleTxt.text = titleName;
        Debug.Log(string.Format("{0}:Opened!", titleName));
    }
    protected override void DoClose()
    {
        base.DoClose();
        Debug.Log(string.Format("{0}:Closed!", titleName));
    }
    protected override void OnPageClosed(Intent intent)
    {
        base.OnPageClosed(intent);
    }
    protected override void OnPageOpened(Intent intent)
    {
        base.OnPageOpened(intent);
    }
    public override void OnPageNotify(Intent intent)
    {
        base.OnPageNotify(intent);
    }
    public override void OpenPage(string pageName)
    {
        switch (pageName)
        {
            case "TownPage2":
                Open(pageName, new Tuple<string, string>("TownPage2", "TownPage是城镇主页，是大家最常见、最基础的页面！而我只是个替身……"), PageType.FullScreen);
                break;
        }
    }
}
