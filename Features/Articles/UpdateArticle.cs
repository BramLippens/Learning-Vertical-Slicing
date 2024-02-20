using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newsletter.Api.Contract;
using Newsletter.Api.Database;
using Newsletter.Api.Shared;

namespace Newsletter.Api.Features.Articles;

public static class UpdateArticle
{
    public class Command : IRequest<Result<Guid>>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Title).Length(5,20).When(x => !string.IsNullOrEmpty(x.Title));
            RuleFor(x => x.Content).MaximumLength(200);
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
            if (!validationResult.IsValid)
            {
                return Result.Failure<Guid>(new Error("UpdateArticle.Validation",validationResult.ToString()));
            }

            var article = _context.Articles.Where(x => x.Id == request.Id).FirstOrDefault();
            if (article is null)
            {
                return Result.Failure<Guid>(new Error("UpdateArticle.Null", "The article with the specified ID was not found."));
            }
            article.Title = request.Title.IsNullOrEmpty() ? article.Title:request.Title;
            article.Content = request.Content ?? article.Content;

            _context.Entry(article).State = EntityState.Modified;
            await _context.SaveChangesAsync(cancellationToken);
            return article.Id;
        }
    }
}

public class UpdateArticleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/articles/{id}", async (Guid id, UpdateArticleRequest request, ISender sender) => {
            var command = request.Adapt<UpdateArticle.Command>();
            command.Id = id;

            var result = await sender.Send(command);

            if(result.IsFailure)
            {
                return Results.BadRequest(result.Error);
            }

            return Results.Ok(result.Value);
        });
    }
}