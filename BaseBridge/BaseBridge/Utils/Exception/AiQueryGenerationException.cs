namespace BaseBridge.Utils.Exception;

public class AiQueryGenerationException: System.Exception
{
    public AiQueryGenerationException() : base("Gemini query generation error"){}
    public AiQueryGenerationException(string message) : base(message){}
}