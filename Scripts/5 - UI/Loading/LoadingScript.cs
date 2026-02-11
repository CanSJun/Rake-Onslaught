using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class LoadingScript : MonoBehaviour
{

    [SerializeField] private Slider _progressBar;

    private void Start() => StartCoroutine(OnLoad());


    private IEnumerator OnLoad()
    {
        yield return null;
        string nextScene = Loader.GetSceneName;
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        float displayProgress = 0f;
        while (!op.isDone)
        {
            float realProgress = Mathf.Clamp01(op.progress);
            displayProgress = Mathf.MoveTowards(displayProgress, 1f, Time.deltaTime * 0.5f);
            _progressBar.value = displayProgress;
            if (displayProgress >= 1f)
            {
                yield return new WaitForSeconds(0.2f);
                op.allowSceneActivation = true;
            }
            yield return null;
        }

    }

}
