import os

from hqq.engine.hf import HQQModelForCausalLM
from transformers import AutoTokenizer, TextIteratorStreamer
from threading import Thread

from model import model_base

class model_llama_3dot1 (model_base):

    model_name = "llama3.1"

    tokenizer = None
    device = None
    model = None

    def __init__(self, device):
        self.device = device

        local_dir = os.path.dirname(os.path.abspath(__file__)) + "/llama-3.1-8b-hqq-fixed"

        self.model = HQQModelForCausalLM.from_quantized(local_dir)
        self.model = self.model.to(device)

        for layer in self.model.model.layers:
            if hasattr(layer.self_attn, 'rotary_emb'):
                layer.self_attn.rotary_emb = layer.self_attn.rotary_emb.to(device)

        if hasattr(self.model.model, 'rotary_emb'):
            self.model.model.rotary_emb = self.model.model.rotary_emb.to(device)

        self.tokenizer = AutoTokenizer.from_pretrained(local_dir)



    def Prompt(self, inpt):
        streamer = TextIteratorStreamer(self.tokenizer, skip_special_tokens=True)

        generation_kwargs = dict(
            input_ids=self.tokenizer(inpt, return_tensors="pt").input_ids.to(self.device),
            max_new_tokens=150,
            do_sample=True,
            top_k=50,
            top_p=0.9,
            temperature=0.7,
            streamer=streamer,
            pad_token_id=self.tokenizer.eos_token_id
        )

        thread = Thread(target=self.model.generate, kwargs=generation_kwargs)
        thread.start()

        for token in streamer:
            yield token

        thread.join()