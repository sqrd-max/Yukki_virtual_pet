using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 


public class GameManager : MonoBehaviour
{
    public string username;
    
    public int maxMessages = 25;

    
    
    public ChatGPTManaager chatGPTManager;
    
    public GameObject chatPanel, textObject, fbxModel;
    public TMP_InputField chatBox;
    public Animator modelAnimator;
    public Color userMessage, yukkiMessage;
    
    public Button player1Button; // You can assign this from the inspector
    
    
    [SerializeField]
    List<ChatMessage> messageList = new List<ChatMessage>();
    
    void Start()
    {
        if (chatGPTManager != null)
        {
            chatGPTManager.onResponse.AddListener(HandleOpenAIResponse);
        }
        else
        {
            Debug.LogError("ChatGPTManager is not assigned in GameManager");
        }
        
        // Optionally, find the button by tag or name if it's not assigned
        if (player1Button == null)
            player1Button = GameObject.Find("UserButton1").GetComponent<Button>();

        // Add a click listener to the button
        player1Button.onClick.AddListener(OnPlayer1ButtonPressed);
    }

    private void HandleOpenAIResponse(string response)
    {
        SendMessageToChat(response, ChatMessage.MessageType.YukkiMessage); 
    }
    
    
    void Update()
    {
        if (chatBox.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SendMessageToChat(chatBox.text, ChatMessage.MessageType.UserMessage);
                CheckForAnimationCommand(chatBox.text);
                chatBox.text = "";
            }
        }
        else if (!chatBox.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            chatBox.ActivateInputField();
        }
    }

    public void OnPlayer1ButtonPressed()
    {
        SendMessageToChat("Button1 pressed", ChatMessage.MessageType.YukkiMessage);
    }



    void SendMessageToChat(string text, ChatMessage.MessageType messageType)
    {
        if (messageList.Count >= maxMessages)
        {
            Destroy(messageList[0].textObject.gameObject);
            messageList.RemoveAt(0);
        }

        ChatMessage newChatMessage = new ChatMessage();
        newChatMessage.text = text;

        GameObject newText = Instantiate(textObject, chatPanel.transform);
        newChatMessage.textObject = newText.GetComponent<TMP_Text>();

        if (newChatMessage.textObject != null)
        {
            newChatMessage.textObject.text = newChatMessage.text;
            newChatMessage.textObject.color = MessageTypeColor(messageType);
            newChatMessage.textObject.alignment = messageType == ChatMessage.MessageType.UserMessage ?
                TextAlignmentOptions.MidlineRight : TextAlignmentOptions.MidlineLeft;
            newText.GetComponent<RectTransform>().pivot = new Vector2(messageType == ChatMessage.MessageType.UserMessage ? 1 : 0, 0.5f);

            RectTransform textRect = newChatMessage.textObject.GetComponent<RectTransform>();
            messageList.Add(newChatMessage);
        }
        else
        {
            Debug.LogError("Failed to get TMP_Text component from instantiated text object.");
        }
    }
    
    // void UpdateFBXModelSize(TMP_Text textObject)
    // {
    //     RectTransform textRect = textObject.GetComponent<RectTransform>();
    //     Vector2 textSize = new Vector2(textRect.rect.width, textRect.rect.height);
    //
    //     // Предполагаемые исходные размеры модели, к которым вы хотите привязать масштабирование
    //     float baseWidth = 1000f; // Это примерное значение, его нужно подобрать под вашу модель
    //     float baseHeight = 500f; // Это примерное значение, его нужно подобрать под вашу модель
    //
    //     // Вычисляем коэффициенты масштабирования относительно базового размера
    //     float widthScaleFactor = textSize.x / baseWidth;
    //     float heightScaleFactor = textSize.y / baseHeight;
    //
    //     // Применяем масштаб с учетом исходного масштаба и рассчитанных коэффициентов
    //     fbxModel.transform.localScale = new Vector3(-200f * widthScaleFactor, -200f * heightScaleFactor, -200f);
    // }
    
    void CheckForAnimationCommand(string message)
    {
        message = message.ToLower();
        if (message.Contains("walk"))
        {
            modelAnimator.SetTrigger("Walk");
        }
        else if (message.Contains("no") || message.Contains("head shake"))
        {
            modelAnimator.SetTrigger("HeadShakingNO");
        }
        else if (message.Contains("yes") || message.Contains("head nod"))
        {
            modelAnimator.SetTrigger("HeadNodYES");
        }
        else if (message.Contains("wave") || message.Contains("hand wave"))
        {
            modelAnimator.SetTrigger("HandWaving");
        }
        else if (message.Contains("angry"))
        {
            modelAnimator.SetTrigger("Angry");
        }
        
    }

    
    Color MessageTypeColor(ChatMessage.MessageType messageType)
    {
        Color color = yukkiMessage;

        switch (messageType)
        {
            case ChatMessage.MessageType.UserMessage:
                color = userMessage;
                break;
        }
        
        return color;
    }
    
    
}

[System.Serializable]
public class ChatMessage
{
    public string text;
    public TMP_Text textObject;
    public MessageType messageType;
    
    public enum MessageType
    {
        UserMessage,
        YukkiMessage,
    }
}