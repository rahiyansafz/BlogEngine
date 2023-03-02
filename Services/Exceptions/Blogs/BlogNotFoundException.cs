namespace Services.Exceptions.Blogs;
public class BlogNotFoundException : NotFoundException
{
    public BlogNotFoundException(int blogId)
        : base($"Blog wiht id {blogId} does not exist.")
    {
    }
}