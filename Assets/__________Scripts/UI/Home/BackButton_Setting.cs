
using UnityEngine.EventSystems;

public class BackButton_Setting : BackButton_Base
{
    SettingManager settingManager;

    protected override void Start()
    {
        base.Start();
        settingManager = SettingManager.Inst;
    }

    protected override void HideContent()
    {
        uiManager.SettingMain.HideSetting();
        uiManager.HomeButtons.ShowButtons();
        settingManager.Panel.SetWindowSize(UIWindow.Home);
        settingManager.SaveSettingValues();
    }
}
