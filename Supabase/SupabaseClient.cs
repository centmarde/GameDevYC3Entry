using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Supabase REST API client for Unity
/// Handles all HTTP communication with Supabase
/// </summary>
public class SupabaseClient : MonoBehaviour
{
    public static SupabaseClient Instance { get; private set; }
    
    [Header("Configuration")]
    [SerializeField] private SupabaseConfig config;
    
    private string apiUrl;
    private Dictionary<string, string> headers;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeClient();
    }
    
    /// <summary>
    /// Initialize the Supabase client with config
    /// </summary>
    private void InitializeClient()
    {
        if (config == null)
        {
            Debug.LogError("SupabaseClient: No config assigned! Create a SupabaseConfig asset.");
            return;
        }
        
        if (!config.IsValid())
        {
            Debug.LogError("SupabaseClient: Invalid configuration!");
            return;
        }
        
        apiUrl = $"{config.supabaseUrl}/rest/v1";
        
        // Setup default headers
        headers = new Dictionary<string, string>
        {
            { "apikey", config.supabaseAnonKey },
            { "Authorization", $"Bearer {config.supabaseAnonKey}" },
            { "Content-Type", "application/json" },
            { "Prefer", "return=representation" }
        };
        
        LogDebug("SupabaseClient initialized successfully!");
    }
    
    /// <summary>
    /// GET request to fetch data from a table
    /// </summary>
    public IEnumerator Get(string table, string query, Action<string, bool> callback)
    {
        if (config.testMode)
        {
            LogDebug($"[TEST MODE] GET: {table}?{query}");
            callback?.Invoke("{}", true);
            yield break;
        }
        
        string url = $"{apiUrl}/{table}";
        if (!string.IsNullOrEmpty(query))
        {
            url += $"?{query}";
        }
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // Add headers
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
            
            request.timeout = (int)config.requestTimeout;
            
            LogDebug($"GET: {url}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                LogDebug($"GET Success: {request.downloadHandler.text}");
                callback?.Invoke(request.downloadHandler.text, true);
            }
            else
            {
                Debug.LogError($"SupabaseClient GET Error: {request.error}\nURL: {url}");
                callback?.Invoke(request.error, false);
            }
        }
    }
    
    /// <summary>
    /// POST request to insert data
    /// </summary>
    public IEnumerator Post(string table, string jsonBody, Action<string, bool> callback)
    {
        if (config.testMode)
        {
            LogDebug($"[TEST MODE] POST: {table} | Body: {jsonBody}");
            callback?.Invoke(jsonBody, true);
            yield break;
        }
        
        string url = $"{apiUrl}/{table}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            // Add headers
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
            
            request.timeout = (int)config.requestTimeout;
            
            LogDebug($"POST: {url}\nBody: {jsonBody}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                LogDebug($"POST Success: {request.downloadHandler.text}");
                callback?.Invoke(request.downloadHandler.text, true);
            }
            else
            {
                Debug.LogError($"SupabaseClient POST Error: {request.error}\nURL: {url}\nBody: {jsonBody}");
                callback?.Invoke(request.error, false);
            }
        }
    }
    
    /// <summary>
    /// PATCH request to update existing data
    /// </summary>
    public IEnumerator Patch(string table, string query, string jsonBody, Action<string, bool> callback)
    {
        if (config.testMode)
        {
            LogDebug($"[TEST MODE] PATCH: {table}?{query} | Body: {jsonBody}");
            callback?.Invoke(jsonBody, true);
            yield break;
        }
        
        string url = $"{apiUrl}/{table}?{query}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        
        using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            // Add headers
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
            
            request.timeout = (int)config.requestTimeout;
            
            LogDebug($"PATCH: {url}\nBody: {jsonBody}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                LogDebug($"PATCH Success: {request.downloadHandler.text}");
                callback?.Invoke(request.downloadHandler.text, true);
            }
            else
            {
                Debug.LogError($"SupabaseClient PATCH Error: {request.error}\nURL: {url}\nBody: {jsonBody}");
                callback?.Invoke(request.error, false);
            }
        }
    }
    
    /// <summary>
    /// DELETE request to remove data
    /// </summary>
    public IEnumerator Delete(string table, string query, Action<string, bool> callback)
    {
        if (config.testMode)
        {
            LogDebug($"[TEST MODE] DELETE: {table}?{query}");
            callback?.Invoke("{}", true);
            yield break;
        }
        
        string url = $"{apiUrl}/{table}?{query}";
        
        using (UnityWebRequest request = UnityWebRequest.Delete(url))
        {
            // Add headers
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
            
            request.timeout = (int)config.requestTimeout;
            
            LogDebug($"DELETE: {url}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                LogDebug($"DELETE Success");
                callback?.Invoke("{}", true);
            }
            else
            {
                Debug.LogError($"SupabaseClient DELETE Error: {request.error}\nURL: {url}");
                callback?.Invoke(request.error, false);
            }
        }
    }
    
    /// <summary>
    /// Helper to log debug messages
    /// </summary>
    private void LogDebug(string message)
    {
        if (config != null && config.showDebugLogs)
        {
            Debug.Log($"[SupabaseClient] {message}");
        }
    }
    
    /// <summary>
    /// Check if client is properly configured
    /// </summary>
    public bool IsConfigured()
    {
        return config != null && config.IsValid();
    }
}
