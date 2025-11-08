using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Nexx.Models.Backend;

public class ModelSession : IDisposable
{
    public const string ENDING_TOKEN = "[[END]]";


    public string modelName { set; get; }
    public Guid modelId { set; get; }

    private CancellationTokenSource sessionToken;

    private List<MessageEntry> messages;


    public ModelSession(string modelName, Guid modelId)
    {
        this.modelName = modelName;
        this.modelId = modelId;

        messages = new List<MessageEntry>();
        sessionToken = new CancellationTokenSource();
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
        var buffer = new byte[1024 * 4];

        MemoryStream streamingMessage = new MemoryStream();
        CancellationTokenSource socketToken = new CancellationTokenSource();

        while (socket.State == WebSocketState.Open && !sessionToken.IsCancellationRequested)
        {
            await streamingMessage.FlushAsync();

            WebSocketReceiveResult response = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), sessionToken.Token);
            string userInput = Encoding.UTF8.GetString(buffer, 0, response.Count);

            if (string.IsNullOrEmpty(userInput))
                continue;

            await SeekPrompt(socketToken.Token, GenerateMessageConext(userInput), ProcessToken);
            await socket.SendAsync(Encoding.UTF8.GetBytes(ENDING_TOKEN), WebSocketMessageType.Text, true, sessionToken.Token);

            RecordMessage(userInput, true);
            RecordMessage(Encoding.UTF8.GetString(streamingMessage.ToArray()), false);

            await streamingMessage.FlushAsync();
        }

        await socketToken.CancelAsync();

        if (socket.State != WebSocketState.Closed)
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);

        async Task ProcessToken(ArraySegment<byte> token)
        {
            await socket.SendAsync(token, WebSocketMessageType.Text, true, sessionToken.Token);
            await streamingMessage.WriteAsync(token);
        }
    }


    private async Task SeekPrompt(CancellationToken cancellationToken, StringBuilder prompt, Func<ArraySegment<byte>, Task> onProcessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{ModelBackend.PYTHON_ENDPOINT}/prompt")
        {
            Content = new StringContent(JsonSerializer.Serialize(new { model_id = modelId, prompt = prompt.ToString() }), Encoding.UTF8, "application/json")
        };

        using HttpClient client = new HttpClient();

        using var response = await client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        var streamBuffer = new byte[256];
        int bytesRead;

        try
        {
            while (!cancellationToken.IsCancellationRequested && (bytesRead = await stream.ReadAsync(streamBuffer, 0, streamBuffer.Length)) > 0)
            {
                await onProcessToken(new ArraySegment<byte>(streamBuffer, 0, bytesRead));
            }
        }
        catch (OperationCanceledException)
        {

        }
    }


    private void RecordMessage(string msg, bool isUser)
    {
        MessageEntry m = new MessageEntry()
        {
            isUser = !isUser,
            content = msg,
            timestamp = DateTime.UtcNow
        };

        messages.Add(m);
        Console.WriteLine(m.ToString());
    }


    private StringBuilder GenerateMessageConext(string userInput)
    {
        int contextLimit = 10_000;
        int spentContext = 0;

        StringBuilder footer = new StringBuilder(MessageEntry.FormatPrompt(userInput, true));
        footer.AppendLine(MessageEntry.FormatPrompt(string.Empty, false));

        StringBuilder sb = new StringBuilder("system:You are a helpful assistant that answers questions clearly and politely.");

        spentContext += footer.Length;
        spentContext += sb.Length;

        if (messages.Count > 0)
        {
            for (int i = messages.Count - 1; i >= 0; i--)
            {
                string purposed = messages[i].GetPrompt();

                if (spentContext + purposed.Length > contextLimit)
                    break;

                sb.AppendLine(purposed);
            }
        }

        Console.WriteLine(sb.ToString());
        return sb;
    }



    public void Dispose()
    {
        sessionToken.Cancel();
    }



    struct MessageEntry
    {
        public bool isUser;
        public string content;
        public DateTime timestamp;


        public override string ToString()
        {
            return $"[{(isUser ? "USER" : "AGENT")}] {content}";
        }

        public string GetPrompt()
            => MessageEntry.FormatPrompt(content, isUser);

        public static string FormatPrompt(string msg, bool isUser)
        {
            return $"{(isUser ? "USER:" : "ai")}:{msg}";
        }
    }
}