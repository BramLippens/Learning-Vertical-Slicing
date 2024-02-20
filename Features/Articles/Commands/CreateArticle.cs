using Carter;
using FluentValidation;
using FluentValidation.Results;
using Mapster;
using MediatR;
using Newsletter.Api.Contract;
using Newsletter.Api.Database;
using Newsletter.Api.Entities;
using Newsletter.Api.Shared;
using System.Runtime.CompilerServices;

namespace Newsletter.Api.Features.Articles;

public static class CreateArticle
{
    public class Command : IRequest<Result<Guid>>
    {
        public string Title { get; set; } = string.Empty;
        public string Content{ get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Title).NotEmpty().MinimumLength(5);
            RuleFor(x => x.Content).NotEmpty().MaximumLength(200);
        }
    }

    internal sealed class Handler: IRequestHandler<Command, Result<Guid>>
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<Command> _validator;

        public Handler(ApplicationDbContext context, IValidator<Command> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if(!validationResult.IsValid)
            {
                return Result.Failure<Guid>(new Error("CreateArticle.Validation", validationResult.ToString()));
            }

            var article = new Article
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Content = request.Content,
                Tags = request.Tags,
                CreateOnUtc = DateTime.UtcNow
            };

            _context.Add(article);
            await _context.SaveChangesAsync(cancellationToken);
            return article.Id;
        }
    }
}
public class CreateArticleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/articles", async (CreateArticleRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateArticle.Command>();

            var result = await sender.Send(command);
            if (result.IsFailure)
            {
                return Results.BadRequest(result.Error);
            }
            return Results.Ok(result.Value);
        });
    }
}
