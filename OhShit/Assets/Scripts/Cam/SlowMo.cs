using System.Collections;
using UnityEngine;

public class SlowMotionManager : MonoBehaviour
{
    public static SlowMotionManager Instance { get; private set; }

    [Header("Default Slow-Mo Settings")]
    [SerializeField] private float defaultTimeScale = 0.15f;
    [SerializeField] private float defaultSlowDuration = 0.15f;
    [SerializeField] private float defaultEaseBackDuration = 0.3f;

    private float originalFixedDeltaTime;
    private Coroutine activeRoutine;

    private void Awake()
    {
        Instance = this;
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    public void TriggerSlowMotion()
    {
        TriggerSlowMotion(defaultTimeScale, defaultSlowDuration, defaultEaseBackDuration);
    }

    public void TriggerSlowMotion(float timeScale, float slowDuration, float easeBackDuration)
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(SlowMotionRoutine(timeScale, slowDuration, easeBackDuration));
    }

    private IEnumerator SlowMotionRoutine(float targetScale, float slowDuration, float easeBackDuration)
    {
        SetTimeScale(targetScale);

        yield return new WaitForSecondsRealtime(slowDuration);

        float elapsed = 0f;
        while (elapsed < easeBackDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / easeBackDuration;
            float scale = Mathf.Lerp(targetScale, 1f, t);
            SetTimeScale(scale);
            yield return null;
        }

        SetTimeScale(1f);
        activeRoutine = null;
    }

    private void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = originalFixedDeltaTime * scale;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;
    }
}