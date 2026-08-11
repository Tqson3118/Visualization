using MediatR;
using System;

namespace VisualizationDSA.Application.Features.TheoryArticles.Commands.DeleteTheoryArticle
{
    public record DeleteTheoryArticleCommand : IRequest
    {
        public Guid ArticleId { get; init; }
        public Guid AuthorId { get; init; }
    }
}
