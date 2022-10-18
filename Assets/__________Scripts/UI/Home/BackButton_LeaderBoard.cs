
public class BackButton_LeaderBoard : BackButton_Base
{
    SettingManager settingManager;

    protected override void Start()
    {
        base.Start();
        settingManager = SettingManager.Inst;
    }

    protected override void HideContent()
    {
        uiManager.LeaderBoard_Home.HideLeaderBoard();
        uiManager.HomeButtons.ShowButtons();
        settingManager.Panel.SetWindowSize(UIWindow.Home);
    }
}
