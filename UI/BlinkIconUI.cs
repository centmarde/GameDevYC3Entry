using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BlinkIconUI : MonoBehaviour
{
    [Header("References")]
    public Player2_BlinkSkill blinkSkill;
    public Image iconImage;
    public Image cooldownMask;

    [Header("Visual Settings")]
    [Range(0f, 1f)] public float cooldownAlpha = 0.35f;
    public bool pulseWhenReady = true;
    public float readyPulseTime = 0.15f;
    public float readyPulseScale = 1.1f;

    private Color readyColor;
    private Color cooldownColor;
    private Vector3 baseScale;

    void Awake()
    {
        if (!iconImage) iconImage = GetComponent<Image>();

        readyColor = Color.white;
        cooldownColor = new Color(1f, 1f, 1f, cooldownAlpha);
        baseScale = transform.localScale;
    }

    void OnEnable()
    {
        if (blinkSkill != null)
        {
            blinkSkill.OnBlinkUsed += HandleBlinkUsed;
            blinkSkill.OnBlinkReady += HandleBlinkReady;
        }
        RefreshInstant();
    }

    void OnDisable()
    {
        if (blinkSkill != null)
        {
            blinkSkill.OnBlinkUsed -= HandleBlinkUsed;
            blinkSkill.OnBlinkReady -= HandleBlinkReady;
        }
    }

    void Update()
    {
        if (blinkSkill == null || cooldownMask == null || iconImage == null) return;

        if (blinkSkill.IsOnCooldown)
        {
            float t = Mathf.Clamp01(1f - blinkSkill.CooldownProgress);
            cooldownMask.fillAmount = 1f - t;
            iconImage.color = cooldownColor;
        }
        else
        {
            cooldownMask.fillAmount = 0f;
            iconImage.color = readyColor;
        }
    }

    private void HandleBlinkUsed()
    {
        if (cooldownMask) cooldownMask.fillAmount = 1f;
        if (iconImage) iconImage.color = cooldownColor;
    }

    private void HandleBlinkReady()
    {
        if (cooldownMask) cooldownMask.fillAmount = 0f;
        if (iconImage) iconImage.color = readyColor;
        if (pulseWhenReady) StartCoroutine(ReadyPulse());
    }

    private IEnumerator ReadyPulse()
    {
        float t = 0f;
        while (t < readyPulseTime)
        {
            t += Time.unscaledDeltaTime;
            float k = 1f + (readyPulseScale - 1f) * Mathf.Sin((t / readyPulseTime) * Mathf.PI);
            transform.localScale = baseScale * k;
            yield return null;
        }
        transform.localScale = baseScale;
    }

    private void RefreshInstant()
    {
        if (blinkSkill != null && blinkSkill.IsOnCooldown)
        {
            float t = Mathf.Clamp01(1f - blinkSkill.CooldownProgress);
            cooldownMask.fillAmount = 1f - t;
            iconImage.color = cooldownColor;
        }
        else
        {
            cooldownMask.fillAmount = 0f;
            iconImage.color = readyColor;
        }
    }
}
