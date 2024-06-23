using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using OpenAI;
using OpenAI.Assistants;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using Newtonsoft.Json;

#pragma warning disable OPENAI001

public class ChatGPTManaager : MonoBehaviour
{
    public UnityEvent<string> onResponse;

    private YukkiBrainCredentials _credentials;
    private bool _credentialsAreLoaded = false;
    void Start()
    {
        StartCoroutine(GetJsonData());
    }

    IEnumerator GetJsonData()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://yukkipet.com/config.json"))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(": Error: " + webRequest.error);
            }
            else
            {
                // Parse JSON data
                string jsonData = webRequest.downloadHandler.text;
                var myData = JsonConvert.DeserializeObject<YukkiBrainCredentials>(jsonData);
                // Handle the parsed data
                Debug.Log("Credentials loaded: ");
                _credentialsAreLoaded = true;
            }
        }
    }

    // Define a class that matches the structure of your JSON data
    [System.Serializable]
    public class YukkiBrainCredentials
    {
        public string key;
        public string assistant;
        public string thread;
        
    }
    
    public async void AskWithText(string newText)
    {
        OpenAIClient client = new(_credentials.key);
        var assistantClient = client.GetAssistantClient();
        var taskGetAssistant = assistantClient.GetAssistantAsync(_credentials.assistant);
        var taskGetThread = assistantClient.GetThreadAsync(_credentials.thread);
        var assistant = (await taskGetAssistant).Value;
        var chatThread = (await taskGetThread).Value;
        var message = await assistantClient.CreateMessageAsync(chatThread, MessageRole.User, new[] { MessageContent.FromText(newText) },
            new MessageCreationOptions()
            {

            });
        // assistantClient.
        //assistantClient.CreateRun();




    }
    


    // private OpenAIApi openAI = new OpenAIApi();
    // private List<ChatMessage> messages = new List<ChatMessage>();
    //
    // public async void AskChatGPT(string newText)
    // {
    //     ChatMessage newMessage = new ChatMessage();
    //     newMessage.Content = newText;
    //     newMessage.Role = "user";
    //     
    //     messages.Add(newMessage);
    //
    //     CreateChatCompletionRequest request = new CreateChatCompletionRequest();
    //     request.Messages = messages;
    //     request.Model = "gpt-3.5-turbo";
    //
    //     var response = await openAI.CreateChatCompletion(request);
    //
    //     if (response.Choices != null && response.Choices.Count > 0)
    //     {
    //         var chatResponse = response.Choices[0].Message;
    //         messages.Add(chatResponse);
    //         
    //         //Debug.Log(chatResponse.Content);
    //         
    //         onResponse.Invoke(chatResponse.Content);
    //     }
    // }
    
}
