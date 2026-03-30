using UnityEngine;

/// <summary>
/// 全局 BGM 管理器：单例常驻，跨场景持续播放。
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public sealed class GlobalBgmManager : MonoBehaviour
{
    public static GlobalBgmManager Instance { get; private set; }

    [Header("Default BGM")]
    [SerializeField] private AudioClip defaultBgm;
    [SerializeField] private bool playOnAwake = true;

    [Header("Audio Settings")]
    [SerializeField] [Range(0f, 1f)] private float volume = 0.7f;
    [SerializeField] private bool loop = true;

    private AudioSource bgmSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource = GetComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = loop;
        bgmSource.volume = volume;

        if (playOnAwake && defaultBgm != null)
        {
            Play(defaultBgm);
        }
    }

    /// <summary>
    /// 播放指定 BGM（与当前相同则忽略）。
    /// </summary>
    public void Play(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        if (bgmSource == null)
        {
            return;
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.volume = volume;
        bgmSource.Play();
    }

    /// <summary>
    /// 停止当前 BGM。
    /// </summary>
    public void Stop()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    /// <summary>
    /// 设置音量并立即生效。
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (bgmSource != null)
        {
            bgmSource.volume = volume;
        }
    }

    /// <summary>
    /// 获取当前播放音量。
    /// </summary>
    public float GetVolume()
    {
        return volume;
    }
}
