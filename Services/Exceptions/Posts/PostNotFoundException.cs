namespace Services.Exceptions.Posts;
public class PostNotFoundException : NotFoundException
{
    public PostNotFoundException(int postId)
        : base($"Post wiht id {postId} does not exist.")
    {

    }
}