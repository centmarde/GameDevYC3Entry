using UnityEngine;
using System.Collections;
public class Object_Chest : MonoBehaviour , IInteractable
{


    [Header("References")]
    private Collider solidCollider; // drag the chest's collider here
    private PersistentGuid guid;
    private GameObject mimicPrefab;
    private InteractionProfile profile;

    [Header("Animations")]
    private Animator anim;  
    private string chestOpenTrigger = "open";

    [Header("Search Reveal")]
    private int delayInSeconds = 1;
    private bool hiddenAtStart = true;

    //Runtime changes
    public bool isHidden { get; private set; }
    private bool isOpen;
    [SerializeField] private ChestOutcome outcome = ChestOutcome.Unknown;
    [SerializeField] private bool resolved = false;

    public System.Action OnShowLootUI;
    public System.Action OnShowCodexUI;

    private Renderer[] chestRenderers;

    private void Reset()
    {
        if (!solidCollider) solidCollider = GetComponent<Collider>();
        if (!anim) anim = GetComponentInChildren<Animator>(true);
        if (!guid) guid = GetComponent<PersistentGuid>();
    }

    public void Awake()
    {
        if (!anim) anim = GetComponentInChildren<Animator>(true);
        if (!solidCollider) solidCollider = GetComponent<Collider>();
        if (!guid) guid = GetComponent<PersistentGuid>();

        chestRenderers = GetComponentsInChildren<Renderer>(true);

        if (guid && ChestSave.TryLoad(guid.Value, out var savedOutcome, out var savedResolved))
        {
            outcome = savedOutcome;
            resolved = savedResolved;
        }


        if (hiddenAtStart)
        {
            Hide();
        }
        else
        {
            Appear();
        }

        if (resolved) { isOpen = true; SetOpenedVisuals(); }   // ← block re‑open
    }

    public void Interact(Player player)
    {
        if (isHidden || isOpen || resolved) return;

        isOpen = true;
        if (anim && !string.IsNullOrEmpty(chestOpenTrigger))
            anim.SetTrigger(chestOpenTrigger);

        OpenFlow(); // <- call directly (no coroutine)
    }

    public InteractionProfile GetProfile() => profile;


    public void Hide()
    {
        isHidden = true;

        foreach (Renderer chestRenderer in chestRenderers)
        {
            if (chestRenderer) 
                chestRenderer.enabled = false;
        }

        if (solidCollider) solidCollider.isTrigger = true;

    }

    public void Appear()
    {
        if (isHidden == false) return;

        isHidden = false;

        foreach (Renderer chestRenderer in chestRenderers)
        {
            if (chestRenderer)
                chestRenderer.enabled = true;
        }

        if (solidCollider) solidCollider.isTrigger = false;

        Debug.Log("Found a hidden chest nearby!");


    }

    public void OnDetectedBySearchSkill(Vector3 pingOrigin, float pingMaxRadius , float pingDuration)
    {
        if (isHidden == false) return;

        float speed = Mathf.Max(0.0001f, pingMaxRadius / Mathf.Max(0.01f, pingDuration)); // radius/sec
        float dist = Vector3.Distance(pingOrigin, transform.position);
        float when = (dist / speed) + delayInSeconds;

        StartCoroutine(RevealAfterDelay(when));



    }

    IEnumerator RevealAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Appear();
    }



   

    private void SetOpenedVisuals()
    {
        // Cheap “opened” look: disable collider and slightly dim
        if (solidCollider) solidCollider.enabled = false;
        foreach (var r in chestRenderers) if (r) r.material.color *= 0.8f;
        // You can also keep the lid in an Open pose via animator.
    }

   

    // 2) OpenFlow signature
    private void OpenFlow()   // <- was: IEnumerator
    {
        if (outcome == ChestOutcome.Unknown)
        {
            outcome = RollOutcomeDeterministic(guid ? guid.Value : name);
            SaveState();
        }

        switch (outcome)
        {
            case ChestOutcome.TimeChest:
                if (OnShowCodexUI != null || OnShowLootUI != null)
                {
                    OnShowCodexUI?.Invoke();
                    OnShowLootUI?.Invoke();
                }
                else
                {
                    Resolve();
                }
                break;

            case ChestOutcome.Mimic:
                TransformToMimic();
                Resolve(); // for now; later resolve after enemy death
                break;
        }
    }

    private void Resolve()
    {
        resolved = true;
        SaveState();
        SetOpenedVisuals();
    }

    private void SaveState()
    {
        if (guid) ChestSave.Save(guid.Value, outcome, resolved);
    }


    private void TransformToMimic()
    {
        // Hide chest visuals and collision
        foreach (var r in chestRenderers) if (r) r.enabled = false;
        if (solidCollider) solidCollider.enabled = false;

        // Spawn placeholder enemy (replace later with your real mimic)
        if (mimicPrefab) Instantiate(mimicPrefab, transform.position, transform.rotation);
    }




    // ---- Deterministic chooser: same GUID -> same outcome forever ----
    private ChestOutcome RollOutcomeDeterministic(string seed)
    {
        int h = Mathf.Abs(seed.GetHashCode());
        int r = h % 100;
        // Tune weights: e.g., 80% TimeChest, 20% Mimic
        return (r < 80) ? ChestOutcome.TimeChest : ChestOutcome.Mimic;
    }
}
