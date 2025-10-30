using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class MinimapDisableFogURP : MonoBehaviour
{
    private Camera minimapCam;
    private bool previousFog;

    void OnEnable()
    {
        minimapCam = GetComponent<Camera>();
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        if (cam == minimapCam)
        {
            // Remember the current fog state and disable it
            previousFog = RenderSettings.fog;
            RenderSettings.fog = false;
        }
    }

    private void OnEndCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        if (cam == minimapCam)
        {
            // Restore fog for the rest of the scene
            RenderSettings.fog = previousFog;
        }
    }
}
