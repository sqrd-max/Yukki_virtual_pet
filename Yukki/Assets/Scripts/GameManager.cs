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
    
    public GameObject chatPanel, textObject;
    public TMP_InputField chatBox;

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
                if (chatGPTManager != null)
                {
                    chatGPTManager.AskChatGPT(chatBox.text); 
                    chatBox.text = ""; 
                }
            }
        }
        else if (!chatBox.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            chatBox.ActivateInputField();
        }
    }



    public void SendMessageToChat(string text, Message.MessageType messageType)
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
            
            if (messageType == Message.MessageType.userMessage)
            {
                newMessage.textObject.alignment = TextAlignmentOptions.MidlineRight;
                newText.GetComponent<RectTransform>().pivot = new Vector2(1, 0.5f);
            }
            else if (messageType == Message.MessageType.yukkiMessage)
            {
                newMessage.textObject.alignment = TextAlignmentOptions.MidlineLeft;
                newText.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f); 
            }

            messageList.Add(newMessage);
        }
        else
        {
            Debug.LogError("Failed to get TMP_Text component from instantiated text object.");
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