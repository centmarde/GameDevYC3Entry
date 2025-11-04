using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class GroqLLMIntegration : MonoBehaviour
{
    [Header("Groq API Configuration")]
    [SerializeField] private string apiKey = ""; // Load from .env file
    [SerializeField] private string model = "llama-3.3-70b-versatile"; // Default Groq model
    
    [Header("UI References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI outputText;
    [SerializeField] private Button sendButton;
    [SerializeField] private ScrollRect scrollRect;
    
    [Header("Settings")]
    [SerializeField] private float temperature = 0.7f;
    [SerializeField] private int maxTokens = 1024;
    [SerializeField] private bool autoScroll = true;
    
    private const string GROQ_API_URL = "https://api.groq.com/openai/v1/chat/completions";
    private bool isProcessing = false;
    
    // Game Master conversation tracking
    private List<Message> conversationHistory = new List<Message>();
    private int questionCount = 0;
    private bool personalityAnalyzed = false;
    private const int MAX_QUESTIONS = 3;
    
    private const string SYSTEM_PROMPT = @"You are the 'Game Master', a mysterious and atmospheric horror game narrator who speaks in a mix of English and Tagalog (Taglish). Your role is to ask exactly 3 horror-related questions to understand the player's personality and fear level.

IMPORTANT RULES:
1. Mix English and Tagalog naturally in your questions (e.g., 'Are you takot sa dilim?' or 'Have you experienced ba any paranormal events?')
2. Ask questions about horror scenarios, supernatural beliefs, scary experiences, or fears
3. Be mysterious and atmospheric in your tone
4. After the player answers your 3rd question, analyze their personality based on ALL their responses
5. When analyzing (after question 3), respond with ONLY a personality assessment that MUST end with exactly one of these tags on a new line:
   [PERSONALITY:1] - if takot/scared easily
   [PERSONALITY:2] - if may trauma/has trauma
   [PERSONALITY:3] - if hindi naniniwala sa supernatural/doesn't believe
   [PERSONALITY:4] - if walang pake/doesn't care
   [PERSONALITY:5] - if gusto lang maglaro/just wants to play

Example question flow:
Q1: 'Kumusta, brave soul! Tell me, are you takot ba when you're mag-isa sa gabi sa madilim na lugar?'
Q2: 'Interesting... Have you ever nakakita ba ng something na hindi mo ma-explain? Any paranormal experience?'
Q3: 'Last question na... If may multo sa bahay mo, what would you do? Takbo agad or investigate pa?'

After Q3, analyze and end with the personality tag.";
    
    private void Start()
    {
        // Load API key from .env file
        LoadAPIKey();
        
        SetupUI();
        
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendButtonClicked);
        }
        
        if (inputField != null)
        {
            inputField.onSubmit.AddListener(OnInputSubmit);
        }
        
        // Initialize Game Master
        StartGameMaster();
    }
    
    private void LoadAPIKey()
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            apiKey = EnvLoader.GetEnvVariable("GROQ_API_KEY");
            
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogError("GROQ_API_KEY not found! Please create a .env file with your API key.");
                Debug.LogError("Example: GROQ_API_KEY=your_api_key_here");
            }
            else
            {
                Debug.Log("API Key loaded successfully from .env file!");
            }
        }
    }
    
    private void StartGameMaster()
    {
        // Clear conversation history
        conversationHistory.Clear();
        questionCount = 0;
        personalityAnalyzed = false;
        
        // Add system prompt
        conversationHistory.Add(new Message { role = "system", content = SYSTEM_PROMPT });
        
        // Game Master's opening
        StartCoroutine(SendGameMasterGreeting());
    }
    
    private IEnumerator SendGameMasterGreeting()
    {
        yield return new WaitForSeconds(0.5f);
        
        string greeting = "Magandang gabi, traveler... I am the Game Master. Before we begin our journey into darkness, I need to understand sino ka talaga. Answer my 3 questions truthfully... if you dare. 😈\n\n";
        AddToOutput($"<color=yellow>Game Master:</color> {greeting}\n");
        
        // Ask first question automatically
        yield return new WaitForSeconds(1f);
        conversationHistory.Add(new Message { role = "user", content = "Start the questionnaire" });
        StartCoroutine(SendGroqRequest("Start the questionnaire", true));
    }
    
    private void SetupUI()
    {
        // Auto-setup UI if not assigned
        if (inputField == null)
        {
            inputField = transform.Find("InputField")?.GetComponent<TMP_InputField>();
            Debug.Log($"InputField found: {inputField != null}");
        }
        
        if (outputText == null)
        {
            // Try multiple possible paths
            outputText = transform.Find("OutputText")?.GetComponent<TextMeshProUGUI>();
            if (outputText == null)
            {
                outputText = transform.Find("ScrollView/Viewport/Content/OutputText")?.GetComponent<TextMeshProUGUI>();
            }
            Debug.Log($"OutputText found: {outputText != null}, GameObject: {outputText?.gameObject.name}");
        }
        
        if (sendButton == null)
        {
            sendButton = transform.Find("SendButton")?.GetComponent<Button>();
            Debug.Log($"SendButton found: {sendButton != null}");
        }
        
        if (scrollRect == null)
        {
            scrollRect = transform.Find("ScrollView")?.GetComponent<ScrollRect>();
            Debug.Log($"ScrollRect found: {scrollRect != null}");
        }
        
        // Initialize output text
        if (outputText != null)
        {
            // Ensure proper TextMeshPro settings
            outputText.richText = true;
            outputText.enableWordWrapping = true;
            outputText.overflowMode = TextOverflowModes.Overflow;
            outputText.fontSize = 18; // Increase font size for better visibility
            outputText.color = Color.white;
            outputText.alignment = TextAlignmentOptions.TopLeft;
            
            outputText.text = "";
            Debug.Log($"OutputText initialized");
            Debug.Log($"OutputText richText enabled: {outputText.richText}");
            Debug.Log($"OutputText fontSize: {outputText.fontSize}");
        }
        else
        {
            Debug.LogError("OutputText is NULL! Cannot display messages.");
        }
    }
    
    private void OnSendButtonClicked()
    {
        SendMessage();
    }
    
    private void OnInputSubmit(string input)
    {
        SendMessage();
    }
    
    public void SendMessage()
    {
        if (isProcessing)
        {
            Debug.LogWarning("Already processing a request. Please wait.");
            return;
        }
        
        if (personalityAnalyzed)
        {
            Debug.LogWarning("Questionnaire completed. Personality already analyzed.");
            AddToOutput("<color=red>The questionnaire has ended. Your personality has been determined!</color>\n\n");
            return;
        }
        
        if (inputField == null || string.IsNullOrWhiteSpace(inputField.text))
        {
            Debug.LogWarning("Input field is empty or not assigned.");
            return;
        }
        
        string userMessage = inputField.text.Trim();
        AddToOutput($"<color=cyan>You:</color> {userMessage}\n\n");
        
        // Add to conversation history
        conversationHistory.Add(new Message { role = "user", content = userMessage });
        questionCount++;
        
        StartCoroutine(SendGroqRequest(userMessage, false));
        
        inputField.text = "";
        inputField.ActivateInputField();
    }
    
    private IEnumerator SendGroqRequest(string message, bool isSystemInitiated)
    {
        isProcessing = true;
        UpdateButtonState(false);
        
        if (!isSystemInitiated)
        {
            AddToOutput("<color=yellow>Game Master:</color> ");
        }
        else
        {
            AddToOutput("<color=yellow>Game Master:</color> ");
        }
        
        // Create the request payload with full conversation history
        var payload = new GroqRequest
        {
            model = model,
            messages = conversationHistory,
            temperature = temperature,
            max_tokens = maxTokens
        };
        
        string jsonPayload = JsonUtility.ToJson(payload);
        
        // Create UnityWebRequest
        using (UnityWebRequest request = new UnityWebRequest(GROQ_API_URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            // Set headers
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            
            // Send request
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string responseText = request.downloadHandler.text;
                    Debug.Log($"Raw API Response: {responseText}");
                    
                    GroqResponse response = JsonUtility.FromJson<GroqResponse>(responseText);
                    
                    if (response != null && response.choices != null && response.choices.Count > 0)
                    {
                        string aiResponse = response.choices[0].message.content;
                        Debug.Log($"AI Response Content: {aiResponse}");
                        Debug.Log($"Model Used: {response.model}");
                        Debug.Log($"Tokens Used - Prompt: {response.usage.prompt_tokens}, Completion: {response.usage.completion_tokens}, Total: {response.usage.total_tokens}");
                        
                        // Add AI response to conversation history
                        conversationHistory.Add(new Message { role = "assistant", content = aiResponse });
                        
                        // Check for personality tag
                        int personality = ExtractPersonality(aiResponse);
                        if (personality > 0)
                        {
                            personalityAnalyzed = true;
                            LogPersonalityResult(personality);
                        }
                        
                        AddToOutput($"{aiResponse}\n\n");
                    }
                    else
                    {
                        Debug.LogWarning("Response received but format is invalid or empty");
                        AddToOutput("<color=red>Error: Invalid response format.</color>\n\n");
                    }
                }
                catch (Exception e)
                {
                    AddToOutput($"<color=red>Error parsing response: {e.Message}</color>\n\n");
                    Debug.LogError($"Response parsing error: {e.Message}\nResponse: {request.downloadHandler.text}");
                }
            }
            else
            {
                string errorMessage = $"Request failed: {request.error}\n";
                if (!string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    errorMessage += $"Details: {request.downloadHandler.text}";
                }
                AddToOutput($"<color=red>{errorMessage}</color>\n\n");
                Debug.LogError(errorMessage);
            }
        }
        
        isProcessing = false;
        UpdateButtonState(true);
    }
    
    private void AddToOutput(string text)
    {
        Debug.Log($"AddToOutput called with text: {text.Substring(0, Mathf.Min(50, text.Length))}...");
        
        if (outputText != null)
        {
            Debug.Log($"OutputText before: {outputText.text.Length} chars");
            outputText.text += text;
            Debug.Log($"OutputText after: {outputText.text.Length} chars");
            Debug.Log($"OutputText GameObject active: {outputText.gameObject.activeInHierarchy}");
            Debug.Log($"OutputText enabled: {outputText.enabled}");
            
            // Force immediate update
            outputText.ForceMeshUpdate();
            
            if (autoScroll && scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
                Debug.Log("Scroll position updated");
            }
        }
        else
        {
            Debug.LogError("Cannot add to output - outputText is NULL!");
        }
    }
    
    private void UpdateButtonState(bool enabled)
    {
        if (sendButton != null)
        {
            sendButton.interactable = enabled;
        }
        
        if (inputField != null)
        {
            inputField.interactable = enabled;
        }
    }
    
    private int ExtractPersonality(string response)
    {
        // Look for [PERSONALITY:X] tag
        if (response.Contains("[PERSONALITY:1]")) return 1;
        if (response.Contains("[PERSONALITY:2]")) return 2;
        if (response.Contains("[PERSONALITY:3]")) return 3;
        if (response.Contains("[PERSONALITY:4]")) return 4;
        if (response.Contains("[PERSONALITY:5]")) return 5;
        return 0;
    }
    
    private void LogPersonalityResult(int personality)
    {
        string personalityType = "";
        switch (personality)
        {
            case 1:
                personalityType = "SCARED - Player is takot/scared easily";
                break;
            case 2:
                personalityType = "TRAUMATIZED - Player has trauma";
                break;
            case 3:
                personalityType = "SKEPTIC - Player doesn't believe in supernatural";
                break;
            case 4:
                personalityType = "APATHETIC - Player doesn't care";
                break;
            case 5:
                personalityType = "PLAYFUL - Player just wants to play";
                break;
        }
        
        Debug.Log($"========================================");
        Debug.Log($"GAME MASTER ANALYSIS COMPLETE!");
        Debug.Log($"Player Personality Type: {personality}");
        Debug.Log($"Description: {personalityType}");
        Debug.Log($"========================================");
        
        AddToOutput($"\n<color=lime>═══════════════════════════════════════</color>\n");
        AddToOutput($"<color=lime>✦ Personality Analysis Complete! ✦</color>\n");
        AddToOutput($"<color=orange>Your Type: {personalityType}</color>\n");
        AddToOutput($"<color=lime>═══════════════════════════════════════</color>\n\n");
    }
    
    public void ClearOutput()
    {
        if (outputText != null)
        {
            outputText.text = "Groq LLM Ready. Enter your message above and press Send.\n";
        }
    }
    
    public void RestartQuestionnaire()
    {
        if (outputText != null)
        {
            outputText.text = "";
        }
        StartGameMaster();
    }
    
    public void SetModel(string newModel)
    {
        model = newModel;
        Debug.Log($"Model changed to: {model}");
    }
    
    // Data structures for JSON serialization
    [Serializable]
    private class GroqRequest
    {
        public string model;
        public List<Message> messages;
        public float temperature;
        public int max_tokens;
    }
    
    [Serializable]
    private class Message
    {
        public string role;
        public string content;
    }
    
    [Serializable]
    private class GroqResponse
    {
        public string id;
        public string @object;
        public long created;
        public string model;
        public List<Choice> choices;
        public Usage usage;
    }
    
    [Serializable]
    private class Choice
    {
        public int index;
        public Message message;
        public string finish_reason;
    }
    
    [Serializable]
    private class Usage
    {
        public int prompt_tokens;
        public int completion_tokens;
        public int total_tokens;
    }
}
