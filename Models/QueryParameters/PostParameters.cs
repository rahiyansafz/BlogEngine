namespace Models.QueryParameters;
public class PostParameters : QueryStringParameters
{
    public string? UsreId { get; set; }
    public int? BlogId { get; set; }
    public string? Tag { get; set; }
    public bool MostLiked { get; set; } = false;
}