namespace Nexx.Models.Backend;

public static class ModelBackend
{
    public const string PYTHON_ENDPOINT = "http://localhost:8000";
    private static List<ModelSession>? activeSessions;


    public static async Task Setup()
    {
        activeSessions = new List<ModelSession>();
        var entries = await GetAllActivePythonModels();

        foreach (var entry in entries)
        {
            activeSessions.Add(new ModelSession(entry.name, entry.id));
        }
    }


    public static async Task<ModelSession> StartModelSession(string modelName)
    {
        using HttpClient client = new HttpClient();
        var startPayload = new { model = modelName };

        var startResponse = await client.PostAsJsonAsync($"{PYTHON_ENDPOINT}/start", startPayload);
        startResponse.EnsureSuccessStatusCode();

        string idStr = await startResponse.Content.ReadAsStringAsync();
        Guid id = Guid.Parse(idStr.Replace("\u0022", ""));

        ModelSession newSession = new ModelSession(modelName, id);
        activeSessions!.Add(newSession);

        return newSession;
    }

    private static async Task<(string name, Guid id)[]> GetAllActivePythonModels()
    {
        using HttpClient client = new HttpClient();
        var res = await client.GetAsync($"{PYTHON_ENDPOINT}/active");

        List<Dictionary<string, string>>? active = await res.Content.ReadFromJsonAsync<List<Dictionary<string, string>>>();
        List<(string, Guid)> toReturn = new List<(string, Guid)>(active?.Count ?? 0);

        if (active != null)
            foreach (var model in active)
            {
                string modelName = model.Keys.First();
                string modelId = model.Values.First();

                if (string.IsNullOrEmpty(modelName) || string.IsNullOrEmpty(modelId))
                    continue;

                if (!Guid.TryParse(modelId, out Guid id))
                    continue;

                toReturn.Add((modelName, id));
            }

        return toReturn.ToArray();
    }



    public static ModelSession? GetActiveSession(Guid modelId)
        => activeSessions!.FirstOrDefault(s => s.modelId == modelId);


    public static bool GetActiveSession(Guid modelName, out ModelSession session)
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        session = GetActiveSession(modelName);
#pragma warning restore CS8601 // Possible null reference assignment.
        return session != null;
    }

    public static ModelSession[] GetActiveModels()
    {
        return activeSessions!.ToArray();
    }

    public static async Task<string[]> GetModelTypes()
    {
        using HttpClient client = new HttpClient();
        var res = await client.GetAsync($"{PYTHON_ENDPOINT}/models");

        return await res.Content.ReadFromJsonAsync<string[]>();
    }
}