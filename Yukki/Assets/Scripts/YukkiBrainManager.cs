using System;
using System.ClientModel;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using UnityEngine;
using OpenAI;
using OpenAI.Assistants;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.ClientModel.Primitives;

#nullable disable
#pragma warning disable OPENAI001


public class YukkiBrainManager : MonoBehaviour
{
    /// <summary>
    /// This class is to fix
    /// https://github.com/openai/openai-dotnet/issues/68
    /// </summary>
    public class AddAuthHeadersPolicy : PipelinePolicy
    {
        public string OrganizationId { get; set; }
        public string ProjectId { get; set; }

        public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
        {
            ApplyHeaders(message?.Request?.Headers);
            ProcessNext(message, pipeline, currentIndex);
        }

        public override ValueTask ProcessAsync(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
        {
            ApplyHeaders(message?.Request?.Headers);
            return ProcessNextAsync(message, pipeline, currentIndex);
        }

        private void ApplyHeaders(PipelineRequestHeaders headers)
        {
            if (headers is null)
            {
                return;
            }
            if (!string.IsNullOrEmpty(OrganizationId))
            {
                headers.Set("OpenAI-Organization", OrganizationId);
            }
            if (!string.IsNullOrEmpty(ProjectId))
            {
                headers.Set("OpenAI-Project", ProjectId);
            }
        }
    }
    
    public UnityEvent<string> onResponse;
    public UnityEvent<string> onAnimationUpdate;

    public int currentUserId = 0;

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

                try
                {
                    _credentials = JsonConvert.DeserializeObject<YukkiBrainCredentials>(jsonData);
                    Debug.Log("Deserialized data");
                    _credentialsAreLoaded = true;

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Debug.Log(e);
                    throw;
                }
                
                // Handle the parsed data
                Debug.Log("Credentials loaded: ");
                
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
    
    public void AskWithText(string newText)
    {
        if (!_credentialsAreLoaded)
        {
            Debug.LogError("Can't ask, while YukkiBrain credentials are not loaded");
            return;
        }
        
        
        AddAuthHeadersPolicy authHeadersPolicy = new()
        {
            ProjectId = "proj_rwNpbWOoZWYvLA71d4hRiUgD",
            OrganizationId = "org-ioTjf9TOc7zHpKlq7nMgnb9g"
        };
        OpenAIClientOptions clientOptions = new();
        clientOptions.AddPolicy(authHeadersPolicy, PipelinePosition.BeforeTransport);

        
        OpenAIClient client = new(_credentials.key, clientOptions);
        var assistantClient = client.GetAssistantClient();
        var assistantInfo = assistantClient.GetAssistant(_credentials.assistant).Value;
        
        
        AssistantThread threadInfo;
        if (currentUserId == 0)
        {
            threadInfo = assistantClient.GetThread("thread_pJiHvkNOYVTDQx2MaZV5jIW8").Value;
        }
        else
        {
            threadInfo = assistantClient.GetThread("thread_4yYjTyrns5kqKcM4XZNw75wz").Value;
        }


        var message = assistantClient.CreateMessage(threadInfo, MessageRole.User, new[] { MessageContent.FromText(newText) },
        new MessageCreationOptions()
        {
        
        });
        
        var updates = assistantClient.CreateRunStreamingAsync(threadInfo, assistantInfo);
        
        
        ProcessThreadRun(assistantClient, updates);
        
        // var taskGetAssistant = assistantClient.GetAssistantAsync(_credentials.assistant);
        // var taskGetThread = assistantClient.GetThreadAsync(_credentials.thread);
        // var assistant = (await taskGetAssistant).Value;
        // var chatThread = (await taskGetThread).Value;
        // var message = await assistantClient.CreateMessageAsync(chatThread, MessageRole.User, new[] { MessageContent.FromText(newText) },
        //     new MessageCreationOptions()
        //     {
        //
        //     });
        // assistantClient.
        //assistantClient.CreateRun();




    }

    private async void ProcessThreadRun(AssistantClient assistantClient, AsyncResultCollection<StreamingUpdate> updates)
    {
        ThreadRun currentRun = null;
        do
        {
            currentRun = null;
            List<ToolOutput> outputsToSubmit = new List<ToolOutput>();
            await foreach (StreamingUpdate update in updates)
            {
                if (update is RunUpdate runUpdate)
                {
                    currentRun = runUpdate;
                }
                else if (update is RequiredActionUpdate requiredActionUpdate)
                {
                    Debug.Log("Requested function call: " + requiredActionUpdate.FunctionName);

                    if (requiredActionUpdate.FunctionName == "animation")
                    {
                        try
                        {
                            JObject jsonObject = JObject.Parse(requiredActionUpdate.FunctionArguments);
                            JToken animationNameToken;
            
                            // Check if 'animation_name' exists and is not empty
                            if (jsonObject.TryGetValue("animation_name", out animationNameToken) &&
                                !string.IsNullOrEmpty(animationNameToken.ToString()))
                            {
                                var animationName = animationNameToken.Value<string>();
                                Debug.Log("JSon parsed animation name: " + animationName);
                                onAnimationUpdate?.Invoke(animationName);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogError("Failed to parse Animation function JSON: " + ex.Message);
                        }
                    }
                    // if (requiredActionUpdate.FunctionName == getTemperatureTool.FunctionName)
                    // {
                    outputsToSubmit.Add(new ToolOutput(requiredActionUpdate.ToolCallId, "true"));
                    // }
                    // else if (requiredActionUpdate.FunctionName == getRainProbabilityTool.FunctionName)
                    // {
                    //     outputsToSubmit.Add(new ToolOutput(requiredActionUpdate.ToolCallId, "25%"));
                    // }
                }
                else if (update is MessageContentUpdate contentUpdate)
                {
                    Debug.Log(contentUpdate.Text);
                    onResponse?.Invoke(contentUpdate.Text);
                }
            }
            if (outputsToSubmit.Count > 0 && currentRun != null)
            {
                updates = assistantClient.SubmitToolOutputsToRunStreamingAsync(currentRun, outputsToSubmit);
            }
        }
        while (currentRun?.Status.IsTerminal == false);
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
