using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using VisualizationDSA.Application.Interfaces;

namespace VisualizationDSA.Application.Features.TheoryArticles.Commands.DeleteTheoryArticle
{
    public class DeleteTheoryArticleCommandHandler : IRequestHandler<DeleteTheoryArticleCommand>
    {
        private readonly IApplicationDbContext _context;

        public DeleteTheoryArticleCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeleteTheoryArticleCommand request, CancellationToken cancellationToken)
        {
            var article = await _context.TheoryArticles
                .FirstOrDefaultAsync(a => a.Id == request.ArticleId, cancellationToken);

            if (article == null)
                throw new ArgumentException("Article not found.");

            article.Delete();
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
