using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Utility class to load environment variables from .env file
/// </summary>
public static class EnvLoader
{
    private static Dictionary<string, string> envVariables = new Dictionary<string, string>();
    private static bool isLoaded = false;

    /// <summary>
    /// Load the .env file from the project root directory
    /// </summary>
    public static void Load()
    {
        if (isLoaded)
            return;

        string envPath = Path.Combine(Application.dataPath, "..", ".env");
        
        if (!File.Exists(envPath))
        {
            Debug.LogWarning($"[EnvLoader] .env file not found at: {envPath}");
            Debug.LogWarning("[EnvLoader] Please create a .env file in the project root with your API keys.");
            return;
        }

        try
        {
            string[] lines = File.ReadAllLines(envPath);
            
            foreach (string line in lines)
            {
                // Skip empty lines and comments
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                    continue;

                // Parse key=value pairs
                int separatorIndex = line.IndexOf('=');
                if (separatorIndex > 0)
                {
                    string key = line.Substring(0, separatorIndex).Trim();
                    string value = line.Substring(separatorIndex + 1).Trim();
                    
                    // Remove quotes if present
                    if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                        (value.StartsWith("'") && value.EndsWith("'")))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }
                    
                    envVariables[key] = value;
                }
            }

            isLoaded = true;
            Debug.Log($"[EnvLoader] Successfully loaded {envVariables.Count} environment variables from .env file");
        }
        catch (Exception e)
        {
            Debug.LogError($"[EnvLoader] Error loading .env file: {e.Message}");
        }
    }

    /// <summary>
    /// Get an environment variable value
    /// </summary>
    /// <param name="key">The variable name</param>
    /// <param name="defaultValue">Default value if not found</param>
    /// <returns>The variable value or default</returns>
    public static string GetVariable(string key, string defaultValue = "")
    {
        if (!isLoaded)
            Load();

        if (envVariables.ContainsKey(key))
        {
            return envVariables[key];
        }

        if (!string.IsNullOrEmpty(defaultValue))
        {
            Debug.LogWarning($"[EnvLoader] Environment variable '{key}' not found, using default value");
        }
        else
        {
            Debug.LogWarning($"[EnvLoader] Environment variable '{key}' not found and no default provided");
        }

        return defaultValue;
    }

    /// <summary>
    /// Check if a variable exists
    /// </summary>
    public static bool HasVariable(string key)
    {
        if (!isLoaded)
            Load();

        return envVariables.ContainsKey(key);
    }

    /// <summary>
    /// Reload the .env file (useful for development)
    /// </summary>
    public static void Reload()
    {
        isLoaded = false;
        envVariables.Clear();
        Load();
    }
}
