using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class SplashManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName;

    void Start()
    {
        videoPlayer.loopPointReached += ChangeScene;
    }

    void ChangeScene(VideoPlayer vp)
    {
        SceneManager.LoadScene(nextSceneName);
    }

    void OnDisable()
    {
        videoPlayer.loopPointReached -= ChangeScene;
    }
}