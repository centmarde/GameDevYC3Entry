using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class RectPercentSizer : MonoBehaviour
{
    [Tooltip("Usually the MonsterButton root (the cell). If left empty, uses this object's parent.")]
    public RectTransform sizeRelativeTo;

    [Range(0f, 1f)] public float widthPercent = 0.8f;
    [Range(0f, 1f)] public float heightPercent = 0.8f;
    public bool keepSquare = true;

    RectTransform rt;
    Vector2 lastParentSize;

    void OnEnable()
    {
        rt = (RectTransform)transform;
        if (sizeRelativeTo == null)
            sizeRelativeTo = rt.parent as RectTransform;

        // normalize
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;

        // first pass
        Recompute();
        // do a late pass after layout
        StartLatePass();
    }

    void OnRectTransformDimensionsChange()
    {
        // parent changed size -> update
        Recompute();
    }

#if UNITY_EDITOR
    void Update()
    {
        if (!Application.isPlaying)
            Recompute();
    }
#endif

    void LateUpdate()
    {
        // during play, layout may update this frame -> keep synced
        Recompute();
    }

    void StartLatePass()
    {
        if (Application.isPlaying)
            StartCoroutine(LateRecompute());
    }

    System.Collections.IEnumerator LateRecompute()
    {
        yield return new WaitForEndOfFrame();
        Recompute();
    }

    void Recompute()
    {
        if (rt == null || sizeRelativeTo == null) return;

        Vector2 parentSize = sizeRelativeTo.rect.size;
        if (parentSize == lastParentSize && Application.isPlaying) return;
        lastParentSize = parentSize;

        float w = parentSize.x * widthPercent;
        float h = parentSize.y * heightPercent;

        if (keepSquare)
        {
            float s = Mathf.Min(w, h);
            w = h = s;
        }

        // clamp just in case
        w = Mathf.Min(w, parentSize.x);
        h = Mathf.Min(h, parentSize.y);

        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = Vector2.zero;
    }
}
