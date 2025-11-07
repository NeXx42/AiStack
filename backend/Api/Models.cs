using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexx.Models.Backend;

namespace backend.Api;

[ApiController]
[Route("api/[controller]")]
public class Models : ControllerBase
{
    [HttpGet("active")]
    public ModelSession[] GetActiveModels()
        => ModelBackend.GetActiveModels();

    [HttpGet("types")]
    public async Task<string[]> GetModelTypes()
        => await ModelBackend.GetModelTypes();


    [HttpPost("{modelType}/start")]
    public async Task StartModel(string modelType)
        => await ModelBackend.StartModelSession(modelType);
}
