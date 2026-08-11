namespace Application.DTOs;

public class AIVisualizerRequest
{
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = "javascript";
}

public class AIVisualizerResponse
{
    public string JsonData { get; set; } = "[]";
}
