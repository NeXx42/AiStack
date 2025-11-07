namespace Nexx.Models.Backend;

public static class ModelBackend
{
    public const string PYTHON_ENDPOINT = "http://localhost:8000";
    private static List<ModelSession> activeSessions;


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

        string id = await startResponse.Content.ReadAsStringAsync();
        id = id.Replace("\u0022", "");

        ModelSession newSession = new ModelSession(modelName, id);
        activeSessions.Add(newSession);

        return newSession;
    }

    public static async Task<ModelSession> StartOrGetSession(string modelName)
    {
        ModelSession? session = GetActiveSession(modelName);

        if (session == null)
        {
            (string _modelName, string _modelId) = (await GetAllActivePythonModels()).FirstOrDefault(x => x.name == modelName);

            if (!string.IsNullOrEmpty(_modelName) && !string.IsNullOrEmpty(_modelId))
            {
                session = new ModelSession(_modelName, _modelId);
                activeSessions.Add(session);

                return session;
            }
        }

        return await StartModelSession(modelName);
    }

    private static async Task<(string name, string id)[]> GetAllActivePythonModels()
    {
        using HttpClient client = new HttpClient();
        var res = await client.GetAsync($"{PYTHON_ENDPOINT}/active");

        List<Dictionary<string, string>>? active = await res.Content.ReadFromJsonAsync<List<Dictionary<string, string>>>();
        List<(string, string)> toReturn = new List<(string, string)>(active?.Count ?? 0);

        if (active != null)
            foreach (var model in active)
            {
                string modelName = model.Keys.First();
                string modelId = model.Values.First();
                if (string.IsNullOrEmpty(modelName) || string.IsNullOrEmpty(modelId))
                    continue;

                Console.WriteLine(modelName + "  : " + modelId);
                toReturn.Add((modelName, modelId));
            }

        return toReturn.ToArray();
    }



    public static ModelSession? GetActiveSession(string modelName)
        => activeSessions.FirstOrDefault(s => s.modelName == modelName);


    public static ModelSession[] GetActiveModels()
    {
        return activeSessions.ToArray();
    }

    public static async Task<string[]> GetModelTypes()
    {
        using HttpClient client = new HttpClient();
        var res = await client.GetAsync($"{PYTHON_ENDPOINT}/models");

        return await res.Content.ReadFromJsonAsync<string[]>();
    }
}