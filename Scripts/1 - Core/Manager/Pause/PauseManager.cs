using UnityEngine;

public static class PauseManager
{
    private static int _pauseCount;
    private static float _prevTimeScale = 1f;
    private static bool _prevAudioPause;
    private static CursorLockMode _prevCursorLock;
    private static bool _prevCursorVisible;


    public static bool IsPaused => _pauseCount > 0;

    /// <summary>
    /// Pause 요청 쌓기
    /// </summary>
    /// <param name="showCursor"></param>

    public static void PushPause(bool showCursor = true)
    {
        if (_pauseCount == 0)
        {
            _prevTimeScale = Time.timeScale;
            _prevAudioPause = AudioListener.pause;

            _prevCursorLock = Cursor.lockState;
            _prevCursorVisible = Cursor.visible;

            Time.timeScale = 0f;
          //  AudioListener.pause = true;

            if (showCursor)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        _pauseCount++;
    }
    /// <summary>
    /// Pause 하나씩 빼기
    /// 
    /// </summary>
    public static void PopPause()
    {
        if (_pauseCount <= 0) return;

        _pauseCount--;
        if (_pauseCount > 0) return;

        Time.timeScale = _prevTimeScale;
        AudioListener.pause = _prevAudioPause;

        Cursor.lockState = _prevCursorLock;
        Cursor.visible = _prevCursorVisible;
    }

    /// <summary>
    /// 
    /// Pause 강제 Clear
    /// </summary>
    public static void PauseClear()
    {
        _pauseCount = 0;
        Time.timeScale = _prevTimeScale;
        AudioListener.pause = _prevAudioPause;

        Cursor.lockState = _prevCursorLock;
        Cursor.visible = _prevCursorVisible;
    }
}