using System.Text;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Nexx.Models.Backend;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseHttpsRedirection();

ModelBackend.Setup();
ModelSession session = await ModelBackend.StartOrGetSession("gpt_large");

await session.Prompt("hello what is my name");


app.Run();