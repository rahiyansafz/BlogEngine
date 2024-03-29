﻿using DataAccess.DataContext;
using DataAccess.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;

using Models.Entities;

namespace DataAccess.Repositories.Implementations;
public class CommentRepository : Repository<Comment>, ICommentRepository
{
    private readonly AppDbContext _appContext;

    public CommentRepository(AppDbContext appContext) : base(appContext) => _appContext = appContext;

    public async Task<IEnumerable<Comment>> GetAllCommentsAsync(int postId, string userId)
    {
        var comments = await _appContext.Comments
            .Where(c => c.PostId == postId)
            .Include(x => x.User)
            .Include(x => x.Likes) // todo : add pagination
            .Select(comment => new Comment
            {
                Id = comment.Id,
                Content = comment.Content,
                PostId = comment.PostId,
                User = comment.User,
                Likes = comment.Likes.ToList(),
                IsLiked = comment.Likes.Any(like => like.UserId == userId),
                LikesCount = comment.Likes.Count()
            })
            .ToListAsync();

        return comments;
    }

    public async Task AddLikeAsync(int commentId, string userId)
    {
        await _appContext.CommentLikes.AddAsync(
            new CommentLike { UserId = userId, CommentId = commentId }
            );
        await _appContext.SaveChangesAsync();
    }

    public async Task RemoveLikeAsync(int commentId, string userId)
    {
        var like = await _appContext.CommentLikes
            .Where(like => like.UserId == userId && like.CommentId == commentId)
            .FirstOrDefaultAsync();

        if (like is not null)
        {
            _appContext.CommentLikes.Remove(like);
            await _appContext.SaveChangesAsync();
        }
    }

    public async Task<int> GetLikesCount(int commentId)
    {
        return await _appContext.CommentLikes
            .CountAsync(like =>
                like.CommentId == commentId
            );
    }

    public async Task<bool> IsLiked(int commentId, string userId)
    {
        return await _appContext
            .CommentLikes
            .CountAsync(like =>
                like.CommentId == commentId && like.UserId == userId
                ) > 0;
    }
}