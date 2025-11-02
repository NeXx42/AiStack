from model_handler import model_handler

from fastapi import FastAPI, Body
from fastapi.responses import StreamingResponse

from pydantic import BaseModel

app = FastAPI()
handler = model_handler()

@app.get("/")
def read_root():
    return {"Hello": "World"}


@app.get("/models")
def get_models():
    return handler.get_all_models()

@app.get("/active")
def get_active_models():
    return handler.get_active_models()

# model specific


class req_start_model(BaseModel):
    model: str

@app.post("/start")
def start_model(data: req_start_model):
    return handler.start_model(data.model)


class req_prompt(BaseModel):
    model_id: str
    prompt: str

@app.post("/prompt")
def model_predict(data: req_prompt):
    model = handler.get_active_model(data.model_id)

    if model != None:
        return StreamingResponse(model.Prompt(data.prompt), media_type="text/event-stream")
    
    return "Invalid model"



