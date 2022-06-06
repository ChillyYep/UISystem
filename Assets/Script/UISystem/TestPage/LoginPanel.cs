using GameBase.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginPanel : MonoBehaviour
{
    public void Login()
    {
        UIModuleManagementCentre.Instance.OpenUI(UIModuleEnum.UIPage, new PageModuleParam()
        {
            name = "TownPage",
            pageType = PageType.FullScreen,
            uiParams = new Tuple<string, string>("TownPage", "TownPage是城镇主页，是大家最常见、最基础的页面！")
        });
        gameObject.SetActive(false);
    }
}
