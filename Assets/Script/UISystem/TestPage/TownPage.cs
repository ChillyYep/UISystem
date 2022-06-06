using GameBase.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TownPage : Page
{
    public Text titleTxt;
    public Text contentTxt;
    private string title;
    public override void OpenPage(string pageName)
    {
        switch (pageName)
        {
            case "TestPage1":
                Open(pageName, new Tuple<string>("TestPage1"), PageType.Cover);
                break;
            case "ChildPage/TestPage2":
                Open(pageName, new Tuple<string>("TestPage2"), PageType.Cover);
                break;
            case "ChildPage/ChildPage/TestPage3":
                Open(pageName, new Tuple<string>("TestPage3"), PageType.Cover);
                break;
            case "Test/TestPage4":
                Open(pageName, new Tuple<string>("TestPage4"), PageType.Cover);
                break;
            case "TestPage5":
                Open(pageName, new Tuple<string>("TestPage5"), PageType.Cover);
                break;
            case "TownPage":
                Open(pageName, new Tuple<string, string>("TownPage", "TownPage是城镇主页，是大家最常见、最基础的页面！"), PageType.FullScreen);
                break;
            case "TownPage2":
                Open(pageName, new Tuple<string, string>("TownPage2", "TownPage是城镇主页，是大家最常见、最基础的页面！而我只是个替身……"), PageType.FullScreen);
                break;
            case "TownPage3":
                Open(pageName, new Tuple<string, string>("TownPage3", "TownPage是城镇主页，是大家最常见、最基础的页面！而我只是个替身的替身……"), PageType.FullScreen, true);
                break;
        }
    }
    protected override void DoOpen()
    {
        base.DoOpen();
        var localParam = Params as Tuple<string, string>;
        title = localParam.Item1;
        titleTxt.text = localParam.Item1;
        contentTxt.text = localParam.Item2;
        Debug.Log(string.Format("{0}:TownPage Opened!", title));
    }
    protected override void DoClose()
    {
        base.DoClose();
        Debug.Log(string.Format("{0}:TownPage Closed!", title));
    }
    protected override void OnPageOpened(Intent intent)
    {
        base.OnPageOpened(intent);
        Debug.Log(string.Format("{0}:Page {1} Open!", title, intent.sender));
    }
    protected override void OnPageClosed(Intent intent)
    {
        base.OnPageClosed(intent);
        Debug.Log(string.Format("{0}:Page {1} Close!", title, intent.sender));
    }
    public override void OnPageNotify(Intent intent)
    {
        switch (intent.pageEvent)
        {
            default:
                base.OnPageNotify(intent);
                break;
        }
    }

    #region TestFunc
    [ContextMenu("CloseTownPage")]
    void CloseTownPage()
    {
        Close();
    }
    #endregion
}
