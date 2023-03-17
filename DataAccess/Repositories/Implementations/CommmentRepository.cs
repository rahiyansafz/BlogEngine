using DataAccess.DataContext;
using DataAccess.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;

using Models.Entities;

namespace DataAccess.Repositories.Implementations;
public class CommmentRepository : Repository<Comment>, ICommentRepository
{
    private readonly AppDbContext _appContext;

    public CommmentRepository(AppDbContext appContext) : base(appContext)
     => _appContext = appContext;

    public async Task<IEnumerable<Comment>> GetAllCommentstAsync(int postId, string userId)
    {
        var commments = await _appContext.Comments
            .Where(c => c.PostId == postId)
            .Include(x => x.User)
            .ToListAsync();

        foreach (var comment in commments)
        {
            var commentLikes = await _appContext.CommentLikes
                .Where(like => like.CommentId == comment.Id)
                .ToListAsync();
            comment.IsLiked = commentLikes.Any(like => like.UserId == userId);
            comment.LikesCount = commentLikes.Count();
        }

        return commments;
    }

    public async Task AddLikeAsync(int commentId, string userId)
    {
        await _appContext.CommentLikes.AddAsync(
            new CommentLike { UserId = userId, CommentId = commentId }
            );
    }

    public async Task RemoveLikeAsync(int commentId, string userId)
    {
        var like = await _appContext.CommentLikes
            .Where(like => like.UserId == userId && like.CommentId == commentId)
            .FirstOrDefaultAsync();

        if (like is not null)
            _appContext.CommentLikes.Remove(like);
    }

    public async Task<int> GetLikesCount(int commentId)
     => await _appContext.CommentLikes
            .CountAsync(like =>
                like.CommentId == commentId
            );

    public async Task<bool> IsLiked(int commentId, string userId)
     => await _appContext
            .CommentLikes
            .CountAsync(like =>
                like.CommentId == commentId && like.UserId == userId
                ) > 0;
}