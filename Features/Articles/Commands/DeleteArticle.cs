using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newsletter.Api.Database;
using Newsletter.Api.Shared;

namespace Newsletter.Api.Features.Articles.Commands;

public static class DeleteArticle
{
    public class Command : IRequest<Result<Guid>>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Command, Result<Guid>>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            await _context.Articles.Where(x => x.Id == request.Id).ExecuteDeleteAsync();

            return request.Id;
        }
    }
}

public class DeleteArticleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/articles/{id}", async (Guid id, ISender sender) =>
        {
            var query = new DeleteArticle.Command { Id = id };
            var result = await sender.Send(query);
            return Results.Ok(result.Value);
        });
    }
}