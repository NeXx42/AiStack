import torch
import uuid

from bin.models.gpt_large.gpt_large import model_gpt_large
from bin.models.llama_3_1.llama_3_1 import model_llama_3dot1


class model_handler:

    all_models = {} 
    active_models = {}

    def __init__(self):
        self.device = "cuda" if torch.cuda.is_available() else "cpu"

        self.add_model(model_gpt_large)
        self.add_model(model_llama_3dot1)
        

    def add_model(self, modelType):
        self.all_models[modelType.model_name] = modelType


    def get_all_models(self):
        return list(self.all_models.keys())

    def get_active_models(self):
        return [{v.model_name: k} for k, v in self.active_models.items()]

    def start_model(self, model_name):
        model = self.all_models.get(model_name)

        if model != None:
            created_model = model(self.device)

            id = uuid.uuid4()
            self.active_models[str(id)] = created_model

            return str(id)

        return "Model not found"
    
    def kill_active_model(self, model_id):
        pass

    def get_active_model(self, model_id):
        return self.active_models.get(model_id)