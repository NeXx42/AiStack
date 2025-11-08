import os
import torch

from model import model_base
from transformers import GPT2LMHeadModel, GPT2Tokenizer

class model_gpt_large (model_base):

    model_name = "gpt_large"

    tokenizer = None
    model = None
    device = None

    def __init__(self, device):
        self.device = device

        model_path = os.path.dirname(os.path.abspath(__file__)) + "/model"

        self.tokenizer = GPT2Tokenizer.from_pretrained(model_path)
        self.model = GPT2LMHeadModel.from_pretrained(model_path).to(device)

        self.model = self.model.half()

    def Prompt(self, inpt):
        # Encode input
        inputs = self.tokenizer(inpt, return_tensors="pt")
        inputs = {k: v.to(self.device) for k, v in inputs.items()}

        # Start with the input IDs
        generated = inputs["input_ids"]

        # Generate token by token
        for _ in range(150):
            # Generate one new token
            outputs = self.model.generate(
                input_ids=generated,
                max_new_tokens=1,
                do_sample=True,
                top_k=50,
                top_p=0.9,
                temperature=0.7,
                repetition_penalty=1.2,
                pad_token_id=self.tokenizer.eos_token_id
            )

            # Get the newly generated token ID
            new_token_id = outputs[0, -1].unsqueeze(0).unsqueeze(0)  # shape [1,1]
            generated = torch.cat([generated, new_token_id], dim=-1)

            token = self.tokenizer.decode(new_token_id[0,0], skip_special_tokens=True)
            yield token

            if new_token_id.item() == self.tokenizer.eos_token_id:
                break