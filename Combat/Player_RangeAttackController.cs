using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

public class Player_RangeAttackController : MonoBehaviour
{
    [Header("Optional UI Label")]
    [SerializeField] private TextMeshProUGUI attackLabelText;

    private List<Player_RangeAttack> attackVariants = new List<Player_RangeAttack>();
    private int currentIndex = 0;

    // Optional: cooldown between scroll switches (prevents rapid cycling)
    [SerializeField] private float switchCooldown = 0.2f;
    private float lastSwitchTime = 0f;

    public Player_RangeAttack CurrentAttack =>
        attackVariants.Count > 0 ? attackVariants[currentIndex] : null;

    private void Awake()
    {
        // Automatically find all attached attack types
        attackVariants.AddRange(GetComponents<Player_RangeAttack>());
    }

    private void Start()
    {
        UpdateAttackLabel();
    }

    //  Called from Input System (bind this to your Scroll action)
    public void OnScroll(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || attackVariants.Count <= 1) return;

        Vector2 scroll = ctx.ReadValue<Vector2>();
        float scrollY = scroll.y;

        // Apply a small cooldown to prevent over-scrolling
        if (Time.time - lastSwitchTime < switchCooldown)
            return;

        if (scrollY > 0f)
            CyclePrevious();
        else if (scrollY < 0f)
            CycleNext();

        lastSwitchTime = Time.time;
    }


    public void OnSwitchAttackType(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        CycleNext();
    }

    private void CycleNext()
    {
        currentIndex = (currentIndex + 1) % attackVariants.Count;
        UpdateAttackLabel();
    }

    private void CyclePrevious()
    {
        currentIndex = (currentIndex - 1 + attackVariants.Count) % attackVariants.Count;
        UpdateAttackLabel();
    }

    private void UpdateAttackLabel()
    {
        if (attackLabelText && CurrentAttack != null)
            attackLabelText.text = $"Attack: {CurrentAttack.GetType().Name}";
    }
}
