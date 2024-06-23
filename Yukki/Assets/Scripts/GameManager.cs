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
    
    [SerializeField]
    List<Message> messageList = new List<Message>();
    
    void Start()
    {
        if (chatGPTManager != null)
        {
            chatGPTManager.OnResponse.AddListener(HandleOpenAIResponse);
        }
        else
        {
            Debug.LogError("ChatGPTManager is not assigned in GameManager");
        }
    }

    private void HandleOpenAIResponse(string response)
    {
        SendMessageToChat(response, Message.MessageType.yukkiMessage); 
    }
    
    
    void Update()
    {
        if (chatBox.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SendMessageToChat(chatBox.text, Message.MessageType.userMessage);
                CheckForAnimationCommand(chatBox.text);
                chatBox.text = "";
            }
        }
        else if (!chatBox.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            chatBox.ActivateInputField();
        }
    }



    void SendMessageToChat(string text, Message.MessageType messageType)
    {
        if (messageList.Count >= maxMessages)
        {
            Destroy(messageList[0].textObject.gameObject);
            messageList.RemoveAt(0);
        }

        Message newMessage = new Message();
        newMessage.text = text;

        GameObject newText = Instantiate(textObject, chatPanel.transform);
        newMessage.textObject = newText.GetComponent<TMP_Text>();

        if (newMessage.textObject != null)
        {
            newMessage.textObject.text = newMessage.text;
            newMessage.textObject.color = MessageTypeColor(messageType);
            newMessage.textObject.alignment = messageType == Message.MessageType.userMessage ?
                TextAlignmentOptions.MidlineRight : TextAlignmentOptions.MidlineLeft;
            newText.GetComponent<RectTransform>().pivot = new Vector2(messageType == Message.MessageType.userMessage ? 1 : 0, 0.5f);

            RectTransform textRect = newMessage.textObject.GetComponent<RectTransform>();
            messageList.Add(newMessage);
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

    
    Color MessageTypeColor(Message.MessageType messageType)
    {
        Color color = yukkiMessage;

        switch (messageType)
        {
            case Message.MessageType.userMessage:
                color = userMessage;
                break;
        }
        
        return color;
    }
    
    
}

[System.Serializable]
public class Message
{
    public string text;
    public TMP_Text textObject;
    public MessageType messageType;
    
    public enum MessageType
    {
        userMessage,
        yukkiMessage
        
        
    }
}