using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MeleeAttackIndicator : MonoBehaviour
{
    [Header("References")]
    public Image radialGlow;

    [Header("Animation Settings")]
    [SerializeField] private float fadeSpeed = 6f;
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float pulseAmount = 0.1f;

    private enum State { Idle, Draw, Slash }
    private State currentState = State.Idle;

    private Color baseColor;
    private Coroutine flashRoutine;

    void Start()
    {
        if (radialGlow)
        {
            baseColor = radialGlow.color;
            Color c = baseColor;
            c.a = 0f; // hidden when idle
            radialGlow.color = c;
        }
    }

    void Update()
    {
        if (!radialGlow) return;

        switch (currentState)
        {
            case State.Idle:
                FadeOut();
                break;

            case State.Draw:
                FadeIn();
                Pulse();
                break;

            case State.Slash:
                // handled by Flash coroutine
                break;
        }
    }

    private void FadeIn()
    {
        Color c = radialGlow.color;
        c.a = Mathf.Lerp(c.a, 1f, Time.deltaTime * fadeSpeed);
        radialGlow.color = c;
    }

    private void FadeOut()
    {
        Color c = radialGlow.color;
        c.a = Mathf.Lerp(c.a, 0f, Time.deltaTime * fadeSpeed);
        radialGlow.color = c;
    }

    private void Pulse()
    {
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        radialGlow.transform.localScale = Vector3.one * pulse;
    }

    public void EnterDrawState()
    {
        currentState = State.Draw;
    }

    public void ExecuteSlash()
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashThenReset());
    }

    private IEnumerator FlashThenReset()
    {
        currentState = State.Slash;

        // bright white flash
        radialGlow.color = Color.white;
        yield return new WaitForSeconds(0.1f);

        // restore color, fade out
        radialGlow.color = baseColor;
        currentState = State.Idle;
    }
}
