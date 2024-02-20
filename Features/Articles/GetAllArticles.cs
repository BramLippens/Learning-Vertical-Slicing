using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newsletter.Api.Contract;
using Newsletter.Api.Database;
using Newsletter.Api.Shared;

namespace Newsletter.Api.Features.Articles;

public static class GetAllArticles
{
    public class Query: IRequest<Result<List<ArticleResponse>>> { }

    internal sealed class Handler : IRequestHandler<Query, Result<List<ArticleResponse>>>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ArticleResponse>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var response = await _context
                .Articles
                .AsNoTracking()
                .Select(article => new ArticleResponse
                {
                    Id = article.Id,
                    Title = article.Title,
                    Content = article.Content,
                    Tags = article.Tags,
                    CreatedOnUtc = article.CreateOnUtc,
                    UpdatedOnUtc = article.UpdateOnUtc
                }).ToListAsync();

            if (response.IsNullOrEmpty())
            {
                return Result.Failure<List<ArticleResponse>>(new Error("GetAllArticles.Empty", "No articles found"));
            }
            return response;
        }
    }
}

public class GetAllArticlesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/articles/", async (ISender sender) =>
        {
            var query = new GetAllArticles.Query { };
            var result = await sender.Send(query);

            if (result.IsFailure)
            {
                return Results.NotFound(result.Error);
            }

            return Results.Ok(result.Value);
        });
    }
}
