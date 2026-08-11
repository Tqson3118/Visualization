using System;

namespace VisualizationDSA.Domain.Entities
{
    public class CustomNode
    {
        public Guid Id { get; private set; }
        public Guid RoadmapId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string Difficulty { get; private set; } = "Medium"; // Easy/Medium/Hard
        public string ContentJson { get; private set; } = "[]"; // content blocks
        public string? VideoUrl { get; private set; }
        public Guid? VisualizerId { get; private set; }
        public string? VisualizerConfig { get; private set; } // JSON: { algorithm, sampleInput, speed } (G3.5.6/G4.1.6)
        public Guid? QuizId { get; private set; }
        public Guid? LabId { get; private set; }
        public Guid? LeetCodeId { get; private set; }
        public int SortOrder { get; private set; }
        public bool IsHidden { get; private set; } // E — ẩn bài vi phạm (moderation)

        public string? OfficialApproach { get; private set; }
        public string? OfficialSolution { get; private set; }
        public string? ComplexityNote { get; private set; }

        public virtual CustomRoadmap Roadmap { get; private set; } = null!;

        public bool IsComplete => QuizId != null && LabId != null && LeetCodeId != null;

        private CustomNode() { } // EF

        public CustomNode(Guid roadmapId, string name, string description, string difficulty, int sortOrder)
        {
            Id = Guid.NewGuid();
            RoadmapId = roadmapId;
            Name = name;
            Description = description;
            Difficulty = difficulty;
            SortOrder = sortOrder;
        }

        public void UpdateContent(string contentJson, string? videoUrl, Guid? visualizerId)
        {
            ContentJson = contentJson;
            VideoUrl = videoUrl;
            VisualizerId = visualizerId;
        }

        public void UpdateVisualizerConfig(string? visualizerConfig)
        {
            VisualizerConfig = visualizerConfig;
        }

        public void UpdatePractice(Guid? quizId, Guid? labId, Guid? leetCodeId)
        {
            QuizId = quizId;
            LabId = labId;
            LeetCodeId = leetCodeId;
        }

        public void UpdateOrder(int sortOrder)
        {
            SortOrder = sortOrder;
        }

        public void SetApproach(string approach, string solution, string complexityNote)
        {
            OfficialApproach = approach;
            OfficialSolution = solution;
            ComplexityNote = complexityNote;
        }

        public void Hide()
        {
            IsHidden = true;
        }

        public void Unhide()
        {
            IsHidden = false;
        }
    }
}
