using UnityEngine.SceneManagement;

public static class Loader
{
    private static string _sceneName;


    public static void Load(string sceneName)
    {
        _sceneName = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }
    public static string GetSceneName => _sceneName;

}
