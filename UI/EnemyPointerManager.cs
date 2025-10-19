using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages enemy pointers on screen for the IsoFollowCamera system.
/// Automatically builds UI and tracks all enemies in the scene.
/// </summary>
public class EnemyPointerManager : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private Camera gameCamera;
    [SerializeField] private Transform playerTransform;
    
    [Header("Pointer Settings")]
    [SerializeField] private GameObject pointerPrefab;
    [SerializeField] private float maxPointerDistance = 100f;
    [SerializeField] private float minPointerDistance = 10f;
    [SerializeField] private float screenEdgeOffset = 50f;
    [SerializeField] private bool showDistanceText = true;
    [SerializeField] private bool showOnlyOffscreenEnemies = true;
    
    [Header("Auto UI Settings")]
    [SerializeField] private bool autoCreateUI = true;
    [SerializeField] private string canvasName = "EnemyPointerCanvas";
    [SerializeField] private int canvasSortOrder = 100;
    
    [Header("Pointer Appearance")]
    [SerializeField] private Color defaultPointerColor = Color.red;
    [SerializeField] private Vector2 pointerSize = new Vector2(30f, 30f);
    
    [Header("Performance")]
    [SerializeField] private float updateFrequency = 0.1f;
    [SerializeField] private int maxPointersToShow = 20;
    
    private Canvas pointerCanvas;
    private RectTransform canvasRect;
    private List<EnemyPointer> activePointers = new List<EnemyPointer>();
    private List<GameObject> trackedEnemies = new List<GameObject>();
    private float lastUpdateTime;
    
    private void Awake()
    {
        SetupReferences();
        if (autoCreateUI)
        {
            CreatePointerUI();
        }
    }
    
    private void Start()
    {
        RefreshEnemyList();
    }
    
    private void Update()
    {
        if (Time.time - lastUpdateTime >= updateFrequency)
        {
            RefreshEnemyList();
            UpdatePointers();
            lastUpdateTime = Time.time;
        }
    }
    
    private void SetupReferences()
    {
        if (gameCamera == null)
        {
            gameCamera = Camera.main;
            if (gameCamera == null)
            {
                gameCamera = FindObjectOfType<Camera>();
            }
        }
        
        if (playerTransform == null)
        {
            Player player = FindFirstObjectByType<Player>();
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerTransform = playerObj.transform;
                }
            }
        }
        
    }
    
    private void CreatePointerUI()
    {
        GameObject existingCanvas = GameObject.Find(canvasName);
        if (existingCanvas != null)
        {
            pointerCanvas = existingCanvas.GetComponent<Canvas>();
        }
        
        if (pointerCanvas == null)
        {
            GameObject canvasObj = new GameObject(canvasName);
            canvasObj.transform.SetParent(transform);
            
            pointerCanvas = canvasObj.AddComponent<Canvas>();
            pointerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            pointerCanvas.sortingOrder = canvasSortOrder;
            
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        canvasRect = pointerCanvas.GetComponent<RectTransform>();
        
        if (pointerPrefab == null)
        {
            CreateDefaultPointerPrefab();
        }
    }
    
    private void CreateDefaultPointerPrefab()
    {
        GameObject prefab = new GameObject("EnemyPointer");
        prefab.transform.SetParent(pointerCanvas.transform, false);
        
        RectTransform rectTransform = prefab.AddComponent<RectTransform>();
        rectTransform.sizeDelta = pointerSize;
        
        var image = prefab.AddComponent<Image>();
        Texture2D arrowTexture = CreateArrowTexture();
        Sprite arrowSprite = Sprite.Create(arrowTexture, new Rect(0, 0, arrowTexture.width, arrowTexture.height), new Vector2(0.5f, 0.5f));
        image.sprite = arrowSprite;
        image.color = defaultPointerColor;
        
        var pointerComponent = prefab.AddComponent<EnemyPointer>();
        pointerComponent.pointerImage = image;
        pointerComponent.rectTransform = rectTransform;
        
        if (showDistanceText)
        {
            GameObject textObj = new GameObject("DistanceText");
            textObj.transform.SetParent(prefab.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(60, 20);
            textRect.anchoredPosition = new Vector2(0, -25);
            
            var text = textObj.AddComponent<Text>();
            text.text = "0m";
            text.fontSize = 12;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            pointerComponent.distanceText = text;
        }
        
        pointerPrefab = prefab;
        prefab.SetActive(false);
    }
    
    private Texture2D CreateArrowTexture()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        
        int centerX = size / 2;
        int tipY = size - 4;
        int baseY = 4;
        
        for (int y = baseY; y < tipY; y++)
        {
            float t = (float)(y - baseY) / (tipY - baseY);
            int width = Mathf.RoundToInt(Mathf.Lerp(size * 0.6f, 2f, t));
            int halfWidth = width / 2;
            
            for (int x = centerX - halfWidth; x <= centerX + halfWidth; x++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                {
                    pixels[y * size + x] = Color.white;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    
    public void RefreshEnemyList()
    {
        trackedEnemies.RemoveAll(e => e == null || !e.activeInHierarchy);
        
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy.activeInHierarchy && !trackedEnemies.Contains(enemy))
            {
                trackedEnemies.Add(enemy);
            }
        }
    }
    
    public void AddEnemy(GameObject enemy)
    {
        if (enemy != null && !trackedEnemies.Contains(enemy))
        {
            trackedEnemies.Add(enemy);
        }
    }
    
    public void RemoveEnemy(GameObject enemy)
    {
        if (enemy != null)
        {
            trackedEnemies.Remove(enemy);
        }
    }
    
    private void UpdatePointers()
    {
        if (gameCamera == null || pointerCanvas == null) return;
        
        List<GameObject> validEnemies = trackedEnemies.Where(e => e != null && ShouldShowPointer(e)).ToList();
        
        if (validEnemies.Count > maxPointersToShow && playerTransform != null)
        {
            validEnemies = validEnemies.OrderBy(e => Vector3.Distance(playerTransform.position, e.transform.position)).Take(maxPointersToShow).ToList();
        }
        
        while (activePointers.Count > validEnemies.Count)
        {
            DestroyPointer(activePointers[activePointers.Count - 1]);
        }
        
        while (activePointers.Count < validEnemies.Count)
        {
            CreatePointer();
        }
        
        for (int i = 0; i < validEnemies.Count; i++)
        {
            UpdatePointer(activePointers[i], validEnemies[i]);
        }
    }
    
    private bool ShouldShowPointer(GameObject enemy)
    {
        if (enemy == null || !enemy.activeInHierarchy) return false;
        
        Vector3 referencePoint = playerTransform != null ? playerTransform.position : gameCamera.transform.position;
        float distance = Vector3.Distance(referencePoint, enemy.transform.position);
        
        if (distance < minPointerDistance || distance > maxPointerDistance) return false;
        
        if (showOnlyOffscreenEnemies)
        {
            Vector3 screenPoint = gameCamera.WorldToViewportPoint(enemy.transform.position);
            if (screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1 && screenPoint.z > 0)
            {
                return false;
            }
        }
        
        return true;
    }
    
    private void CreatePointer()
    {
        if (pointerPrefab == null) return;
        
        GameObject pointerObj = Instantiate(pointerPrefab, pointerCanvas.transform);
        pointerObj.SetActive(true);
        
        EnemyPointer pointer = pointerObj.GetComponent<EnemyPointer>();
        if (pointer != null)
        {
            activePointers.Add(pointer);
        }
    }
    
    private void UpdatePointer(EnemyPointer pointer, GameObject enemy)
    {
        if (pointer == null || enemy == null) return;
        
        pointer.SetTarget(enemy);
        
        Vector3 enemyScreenPos = gameCamera.WorldToScreenPoint(enemy.transform.position);
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 direction = (new Vector2(enemyScreenPos.x, enemyScreenPos.y) - screenCenter).normalized;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        pointer.SetRotation(angle);
        
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float maxX = (screenWidth / 2f) - screenEdgeOffset;
        float maxY = (screenHeight / 2f) - screenEdgeOffset;
        
        Vector2 pointerPos = direction * Mathf.Min(maxX, maxY);
        pointer.SetPosition(pointerPos);
        
        if (showDistanceText && playerTransform != null)
        {
            float distance = Vector3.Distance(playerTransform.position, enemy.transform.position);
            pointer.SetDistance(distance);
        }
        
        pointer.Show();
    }
    
    private void DestroyPointer(EnemyPointer pointer)
    {
        if (pointer != null)
        {
            activePointers.Remove(pointer);
            if (pointer.gameObject != null)
            {
                Destroy(pointer.gameObject);
            }
        }
    }
    
    public void ForceRefresh()
    {
        RefreshEnemyList();
        UpdatePointers();
    }
    
    public void TogglePointers(bool visible)
    {
        if (pointerCanvas != null)
        {
            pointerCanvas.gameObject.SetActive(visible);
        }
    }
    
    public int GetTrackedEnemyCount()
    {
        return trackedEnemies.Count;
    }
    
    public int GetActivePointerCount()
    {
        return activePointers.Count;
    }
    
    public void SetMaxDistance(float distance) => maxPointerDistance = distance;
    public void SetMinDistance(float distance) => minPointerDistance = distance;
    public void SetShowOnlyOffscreen(bool offscreenOnly) => showOnlyOffscreenEnemies = offscreenOnly;
    public void SetUpdateFrequency(float frequency) => updateFrequency = Mathf.Max(0.01f, frequency);
    
    private void OnDestroy()
    {
        foreach (var pointer in activePointers)
        {
            if (pointer != null && pointer.gameObject != null)
            {
                Destroy(pointer.gameObject);
            }
        }
        activePointers.Clear();
    }
}
