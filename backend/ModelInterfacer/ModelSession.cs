using System.Text;
using System.Text.Json;

namespace Nexx.Models.Backend;

public class ModelSession
{
    public string modelName { set; get; }
    public string modelId { set; get; }

    public ModelSession(string modelName, string modelId)
    {
        this.modelName = modelName;
        this.modelId = modelId;
    }


    public async Task Prompt(string prompt)
    {
        var payload = new
        {
            model_id = modelId,
            prompt = prompt
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{ModelBackend.PYTHON_ENDPOINT}/prompt")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        using HttpClient client = new HttpClient();

        using var response = await client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead
        );

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            Console.Write(line);
        }
    }
}