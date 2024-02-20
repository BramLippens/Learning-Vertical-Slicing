namespace Newsletter.Api.Entities;

public class Article
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreateOnUtc { get; set; }
    public DateTime UpdateOnUtc { get; set;}
}
