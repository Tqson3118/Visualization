using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces;

public interface IAIVisualizerService
{
    Task<AIVisualizerResponse> GenerateVisualizationAsync(AIVisualizerRequest request);
}
