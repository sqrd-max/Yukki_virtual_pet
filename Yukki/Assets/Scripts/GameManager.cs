using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;


public class GameManager : MonoBehaviour
{
    public string username;
    
    public int maxMessages = 25;
    
    public YukkiBrainManager brainManager;
    
    public GameObject chatPanel, textObject, fbxModel;
    public TMP_InputField chatBox;
    public Animator modelAnimator;
    public Color userMessageColor;
    public Color yukkiMessageColor;
    public Color systemMessageColor;
    

    public Button player1Button;
    public Button player2Button;// You can assign this from the inspector
    public TMP_Text userNameText;
    public TMP_Text yukkiCloudText;
    public ChatMessage.MessageTypes lastMessageType = ChatMessage.MessageTypes.SystemMessage;

    public int currentUserId;
    public string[] availableAnimations = {
        "Walk",
        "HeadShakingNO",
        "HeadNodYES",
        "HandWaving",
        "Angry"
    };
    
    [SerializeField]
    List<ChatMessage> messageList = new List<ChatMessage>();
    
    void Start()
    {
        if (brainManager != null)
        {
            brainManager.onResponse.AddListener(HandleOpenAIResponse);
            brainManager.onAnimationUpdate.AddListener(CheckForAnimationCommand);
        }
        else
        {
            Debug.LogError("YukkiBrainManager is not assigned in GameManager");
        }
        
        // Add buttons event listeners
        player1Button = GameObject.Find("UserButton1").GetComponent<Button>();
        player1Button.onClick.AddListener(OnPlayer1ButtonPressed);
        
        player2Button = GameObject.Find("UserButton2").GetComponent<Button>();
        player2Button.onClick.AddListener(OnPlayer2ButtonPressed);
        
        SetUserById(0);
    }

    


    private void HandleOpenAIResponse(string response)
    {
        SendMessageToChat(response, ChatMessage.MessageTypes.YukkiMessage); 
    }
    
    
    void Update()
    {
        if (chatBox.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SendMessageToChat(chatBox.text, ChatMessage.MessageTypes.UserMessage);
                brainManager.AskWithText(chatBox.text);
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
        SendMessageToChat("Button1 pressed", ChatMessage.MessageTypes.SystemMessage);
        SetUserById(0);
    }
    
    private void OnPlayer2ButtonPressed()
    {
        SendMessageToChat("Button2 pressed", ChatMessage.MessageTypes.SystemMessage);
        SetUserById(1);
    }

    private void SetUserById(int userId)
    {
        if (userId == 0)
        {
            userNameText.text = "Пользователь: Наташа";
        }
        else
        {
            userNameText.text = "Пользователь: Петр";
        }

        SendMessageToChat(userNameText.text, ChatMessage.MessageTypes.SystemMessage);
        

        currentUserId = userId;
        brainManager.currentUserId = userId;
    }


    void SendMessageToChat(string text, ChatMessage.MessageTypes messageType)
    {
        if (messageList.Count >= maxMessages)
        {
            Destroy(messageList[0].textObject.gameObject);
            messageList.RemoveAt(0);
        }

        if (messageType == ChatMessage.MessageTypes.YukkiMessage && messageList.Count > 0 &&
            messageList.Last().messageType == ChatMessage.MessageTypes.YukkiMessage)
        {
            
            var lastMessage = messageList.Last();
            
            // It would be good to just update last message text like lastMessage.text += text;
            // But it is difficult to update text on screen
            
            // So we update text based on previoud message
            text = lastMessage.text + text;
            
            // And remove last message, so new message will have full text
            messageList.Remove(lastMessage);
            Destroy(lastMessage.textObject.gameObject);
        }
        
        // We do it here as text is altered
        if (messageType == ChatMessage.MessageTypes.YukkiMessage)
        {
            yukkiCloudText.text = text;
        }

        ChatMessage newChatMessage = new ChatMessage();
        newChatMessage.text = text;

        GameObject newText = Instantiate(textObject, chatPanel.transform);
        newChatMessage.textObject = newText.GetComponent<TMP_Text>();

        if (newChatMessage.textObject != null)
        {
            newChatMessage.messageType = messageType;
            newChatMessage.textObject.text = newChatMessage.text;
            newChatMessage.textObject.color = GetMessageColorByType(messageType);
            newChatMessage.textObject.alignment = messageType == ChatMessage.MessageTypes.UserMessage ?
                TextAlignmentOptions.MidlineRight : TextAlignmentOptions.MidlineLeft;
            newText.GetComponent<RectTransform>().pivot = new Vector2(messageType == ChatMessage.MessageTypes.UserMessage ? 1 : 0, 0.5f);

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
    
    void CheckForAnimationCommand(string animationName)
    {
        if (availableAnimations.Contains(animationName))
        {
            Debug.Log($"Playing animation: '{animationName}'");
            modelAnimator.SetTrigger(animationName);
        }
        else
        {
            Debug.Log($"Animation '{animationName}' is not found in available animations");
        }
    }

    
    Color GetMessageColorByType(ChatMessage.MessageTypes messageTypes)
    {
        switch (messageTypes)
        {
            case ChatMessage.MessageTypes.UserMessage:
                return userMessageColor;
            case ChatMessage.MessageTypes.YukkiMessage:
                return yukkiMessageColor;
            default:
                return systemMessageColor;
        }
    }
    
    
}

[System.Serializable]
public class ChatMessage
{
    public string text;
    public TMP_Text textObject;
    public MessageTypes messageType;
    
    public enum MessageTypes
    {
        UserMessage,
        YukkiMessage,
        SystemMessage
    }
}