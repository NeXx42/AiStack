using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Nexx.Models.Backend;

public class ModelSession : IDisposable
{
    public string modelName { set; get; }
    public Guid modelId { set; get; }

    private CancellationTokenSource sessionToken;
    List<Thread> activeSockets = new List<Thread>();


    public ModelSession(string modelName, Guid modelId)
    {
        this.modelName = modelName;
        this.modelId = modelId;

        sessionToken = new CancellationTokenSource();
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
        }
    }


    public async Task AcceptSocket(WebSocketManager socketReq)
    {
        using (WebSocket socket = await socketReq.AcceptWebSocketAsync())
        {
            await HandleSocket(socket);
        }

    }

    private async Task HandleSocket(WebSocket socket)
    {
        await socket.SendAsync(UTF8Encoding.UTF8.GetBytes("Hello"), WebSocketMessageType.Text, true, sessionToken.Token);
        var buffer = new byte[1024 * 4];

        while (socket.State == WebSocketState.Open && !sessionToken.IsCancellationRequested)
        {
            var _res = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), sessionToken.Token);
            string res = UTF8Encoding.UTF8.GetString(buffer, 0, _res.Count);

            var payload = new
            {
                model_id = modelId,
                prompt = res
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

            var buffer2 = new byte[256];
            int read;

            while ((read = await stream.ReadAsync(buffer2, 0, buffer2.Length)) > 0)
            {
                await socket.SendAsync(
                    new ArraySegment<byte>(buffer2, 0, read),
                    WebSocketMessageType.Text,
                    true,
                    sessionToken.Token
                );
            }

            await socket.SendAsync(UTF8Encoding.UTF8.GetBytes("[[END]]"), WebSocketMessageType.Text, true, sessionToken.Token);
        }

        if (socket.State != WebSocketState.Closed)
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
    }

    public void Dispose()
    {
        sessionToken.Cancel();
    }
}