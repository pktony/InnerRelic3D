using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingSceneManager : MonoBehaviour
{
    public static string nextScene;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void OnSceneLoad(Scene arg0, LoadSceneMode arg1)
    {
    }

    void Start()
    {
        StartCoroutine(LoadScene());
    }

    /// <summary>
    /// 다음 씬으로 넘어가기 전 로딩씬 불러오기
    /// </summary>
    /// <param name="_nextScene">로딩씬 다음으로 로드할 씬</param>
    public static void LoadScene(string _nextScene)
    {
        nextScene = _nextScene;
        SceneManager.LoadScene("LoadingScene");
    }

    IEnumerator LoadScene()
    {
        AsyncOperation AsyncOp = SceneManager.LoadSceneAsync(nextScene);
        //AsyncOp.allowSceneActivation = false;

        //yield return new WaitForSeconds(4.0f);

        AsyncOp.allowSceneActivation = true;
        yield return null;
    }
}
