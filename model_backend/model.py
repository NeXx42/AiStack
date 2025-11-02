from abc import ABC, abstractmethod

class model_base(ABC):

    model_name = None

    @abstractmethod
    def Prompt(inp):
        pass