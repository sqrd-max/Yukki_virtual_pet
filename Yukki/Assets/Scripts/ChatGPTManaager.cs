using System.Collections.Generic;
using UnityEngine;
using OpenAI;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class ChatGPTManaager : MonoBehaviour
{
    public UnityEvent<string> onResponse;
    

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
