namespace Models.QueryParameters;
public class PostParameters : QueryStringParameters
{
    public string? UserId { get; set; }
    public int? BlogId { get; set; }
    public string? Tag { get; set; }
    public bool MostLiked { get; set; } = false;
}