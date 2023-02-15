using Chia_NFT_Minter;
using NFT.Storage.Net.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Minter_UI
{
    internal static class OpenAiAccount
    {
        static OpenAiAccount()
        {
            TryLoadApiKey();
        }
        /// <summary>
        /// the api key, obtained from nft.storage
        /// </summary>
        internal static string? ApiKey
        {
            get
            {
                if (_ApiKeyEncrypted == null)
                {
                    return null;
                }
                return Cipher.Decrypt(_ApiKeyEncrypted, Environment.UserName);
            }
            set
            {
                if (value == null)
                {
                    _ApiKeyEncrypted = null;
                    File.Delete(_ApiKeyFile.FullName);
                    return;
                }
                _ApiKeyEncrypted = Cipher.Encrypt(value, Environment.UserName);
                File.WriteAllText(_ApiKeyFile.FullName, _ApiKeyEncrypted);
                File.SetAttributes(_ApiKeyFile.FullName, FileAttributes.Hidden);
            }
        }
        /// <summary>
        /// the api key, obtained from nft.storage in encrypted form. used to provide some memory readout protection. <br/>
        /// can be decrypted with the username
        /// </summary>
        internal static string? _ApiKeyEncrypted { get; set; }
        /// <summary>
        /// the file where the encrypted api key is stored
        /// </summary>
        private static FileInfo _ApiKeyFile = new FileInfo("openai_api.id");
        /// <summary>
        /// tries to load the encrypted api key file from disk
        /// </summary>
        /// <returns>true if NFT_Storage_API was sucessfully initialized</returns>
        public static bool TryLoadApiKey()
        {
            if (_ApiKeyEncrypted == null || _ApiKeyEncrypted == "")
            {
                // try loading file
                if (_ApiKeyFile.Exists)
                {
                    _ApiKeyEncrypted = File.ReadAllText(_ApiKeyFile.FullName);
                    string? apiKey = ApiKey;
                    if (apiKey != null && apiKey != "")
                    {
                        return true;
                    }
                    else
                    { return false; }
                }
                else
                {
                    return false;
                }
                // test api key
            }
            return false;
        }
        private static HttpClient _httpClient { get; set; } = new HttpClient();
        /// <summary>
        /// Completes a text prompt using the OpenAI text completion API.
        /// </summary>
        /// <param name="prompt">The starting text for the text completion task.</param>
        /// <param name="maxTokens">The maximum number of tokens to generate in the completed text.</param>
        /// <param name="temperature">A value between 0 and 1 that controls the creativity of the generated text. Higher values result in more diverse and unexpected completions.</param>
        /// <param name="stop">A list of strings that the text completion should stop at when encountered. (can be a "." to stop after the first completed sentence)</param>
        /// <param name="model">The name of the language model to use for the text completion task.</param>
        /// <param name="answerVariations">The number of possible answers to generate.</param>
        /// <returns>The completed text.</returns>
        public static async Task<string> CompleteTextAsync(string prompt, int maxTokens = 1000, float temperature = 0.7f, string[] stop = null, OpenAiModel model = OpenAiModel.davinciInstructBeta, int answerVariations = 1)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
            var requestData = new
            {
                prompt = prompt,
                max_tokens = maxTokens,
                temperature = temperature,
                stop = stop,
                model = "text-davinci-002",
                n = answerVariations
            };

            var json = JsonSerializer.Serialize(requestData);

            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            try
            {
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<OpenAITextCompletionApiResponse>(responseJson);

                return responseData.choices[0].text.Trim();
            }
            catch (Exception ex)
            {
                if ((int)response.StatusCode == 429)
                {
                    MessageBox.Show("OpenAi returned response: too many requests!"+Environment.NewLine+"Do you have enough Balance?");
                }
                else
                {
                    MessageBox.Show($"OpenAi returned response: {ex.Message}!");
                }
            }

            return null;
        }
    }
    public class OpenAITextCompletionApiResponse
    {
        public OpenAITextCompletionApiChoice[] choices { get; set; }
    }

    public class OpenAITextCompletionApiChoice
    {
        public string text { get; set; }
        public float score { get; set; }
    }
    /// <summary>
    /// Enumerates the OpenAI models that can be used for text completion.
    /// </summary>
    public enum OpenAiModel
    {
        [ModelName("davinci")]
        /// <summary>
        /// The davinci model is the most powerful and versatile model available in the OpenAI API.
        /// It can be used for a wide range of tasks, including text completion, translation, summarization, and more.
        /// </summary>
        /// <remarks>
        ///  $0.006 per token
        /// </remarks>
        davinci,

        [ModelName("curie")]
        /// <summary>
        /// The curie model is a powerful and versatile model that can be used for a wide range of tasks,
        /// including text completion, translation, summarization, and more.
        /// </summary>
        /// <remarks>
        ///  $0.008 per token
        /// </remarks>
        curie,

        [ModelName("babbage")]
        /// <summary>
        /// The babbage model is a smaller and less powerful model than the davinci and curie models.
        /// It's designed for use cases that require less computational resources or smaller models.
        /// </summary>
        /// <remarks>
        ///  $0.004 per token
        /// </remarks>
        babbage,

        [ModelName("ada")]
        /// <summary>
        /// The ada model is a smaller and less powerful model than the davinci and curie models.
        /// It's designed for use cases that require less computational resources or smaller models.
        /// </summary>
        /// <remarks>
        ///  $0.0008 per token
        /// </remarks>
        ada,

        [ModelName("davinci-codex")]
        /// <summary>
        /// The davinci-codex model is specifically designed for generating code from natural language inputs.
        /// It's trained on a large dataset of code and natural language text, and can generate code in a variety of programming languages.
        /// This model is particularly useful for developers who want to quickly prototype or generate code without having to write it manually.
        /// </summary>
        /// <remarks>
        ///  $0.008 per token
        /// </remarks>
        davinciCodex,

        [ModelName("text-davinci-002")]
        /// <summary>
        /// The davinci-instruct-beta model is designed for use in the development of AI-powered chatbots and assistants.
        /// It's trained on a large dataset of conversations and can generate human-like responses to a wide range of inputs.
        /// </summary>
        /// <remarks>
        ///  $0.006 per token
        /// </remarks>
        davinciInstructBeta,

        [ModelName("text-curie-001")]
        /// <summary>
        /// The curie-instruct-beta model is designed for use in the development of AI-powered chatbots and assistants.
        /// It's trained on a large dataset of conversations and can generate human-like responses to a wide range of inputs.
        /// </summary>
        /// <remarks>
        ///  $0.008 per token
        /// </remarks>
        curieInstructBeta
    }

    /// <summary>
    /// Specifies the name of an OpenAI model as a string.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class ModelNameAttribute : Attribute
    {
        public string ModelName { get; }

        public ModelNameAttribute(string modelName)
        {
            ModelName = modelName;
        }
    }
}
