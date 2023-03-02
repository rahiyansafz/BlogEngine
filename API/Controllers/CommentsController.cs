﻿using System.Net.Mime;

using API.Filters;

using AutoMapper;

using DataAccess.Repositories.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Models.ApiModels.RequestDTO;
using Models.Entities;

namespace API.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
[ServiceFilter(typeof(SuspenededActionFilter))]
public class CommentsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;


    public CommentsController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }


    /// <summary>
    /// Add a comment to a post 
    /// </summary>
    /// <param name="postId"></param>
    /// <param name="comment"></param>
    /// <returns></returns>
    [HttpPost("/api/comments")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromBody] CommentRequest comment)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var userId = User.Claims.Where(x => x.Type == "uid").FirstOrDefault()?.Value;
                var post = await _unitOfWork.PostRepository
                    .GetOneAsync(p => p.Id == comment.PostId, default!, default!);
                if (post == null)
                {
                    return BadRequest(
                         "Post doens't exist."
                         );
                }
                if (post.CommentsDisabled)
                {
                    return BadRequest(
                         "Comments are desabled for this post."
                         );
                }
                var newComment = _mapper.Map<Comment>(comment);
                newComment.UserId = userId;
                post.Comments.Add(newComment);
                await _unitOfWork.SaveAsync();

                return Created(
                    $"~api/posts/{comment.PostId}/comments",
                    comment
                    );
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("addComment", ex.Message);
        }
        return BadRequest(ModelState);
    }

    /// <summary>
    /// update a comment 
    /// </summary>
    /// <param name="postId"></param>
    /// <param name="commentId"></param>
    /// <param name="ModifiedComment"></param>
    /// <returns></returns>
    [HttpPut("/api/comments/{commentId}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]

    public async Task<IActionResult> Put(int commentId, [FromBody] CommentRequest ModifiedComment)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var comment = await _unitOfWork.CommentRepository
                    .GetOneAsync(c => c.Id == commentId, default!, default!);
                var userId = User.Claims.Where(x => x.Type == "uid").FirstOrDefault()?.Value;
                if (comment.UserId != userId)
                {
                    return Unauthorized();
                }
                if (comment == null)
                {
                    return BadRequest("Comment doesn't exist");
                }
                comment.Content = ModifiedComment.Content;
                await _unitOfWork.CommentRepository.UpdateAsync(comment);
                await _unitOfWork.SaveAsync();

                return NoContent();
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("EditComment", ex.Message);
        }
        return BadRequest(ModelState);
    }

    /// <summary>
    /// Delete a comment 
    /// </summary>
    /// <param name="postId"></param>
    /// <param name="commentId"></param>
    /// <returns></returns>
    [HttpDelete("/api/comments/{commentId}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int commentId)
    {
        try
        {
            var comment = await _unitOfWork.CommentRepository
                .GetOneAsync(c => c.Id == commentId, default!, default!);
            if (comment == null)
            {
                return BadRequest("Comment Doesn't Exist or already deleted.");
            }
            var userId = User.Claims.Where(x => x.Type == "uid").FirstOrDefault()?.Value;
            if (comment.UserId != userId)
            {
                return Unauthorized();
            }
            await _unitOfWork.CommentRepository.RemoveAsync(comment);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("DeleteComment", ex.Message);
        }
        return BadRequest(ModelState);

    }

    /// <summary>
    /// Like a Comment
    /// </summary>
    /// <param name="postId"></param>
    /// <param name="commentId"></param>
    /// <returns></returns>
    [HttpPost("/api/comments/{commentId}/like")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LikeComment(int commentId)
    {
        try
        {
            var comment = await _unitOfWork.CommentRepository
                .GetOneAsync(c => c.Id == commentId, default!, default!);
            if (comment == null)
                return BadRequest("Comment not found.");
            var userId = User.Claims.Where(x => x.Type == "uid").FirstOrDefault()?.Value;
            await _unitOfWork.CommentRepository.AddLikeAsync(commentId, userId);
            await _unitOfWork.SaveAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("likeComment", ex.Message);
        }
        return BadRequest(ModelState);
    }

    /// <summary>
    /// Remove like of a Comment
    /// </summary>
    /// <param name="postId"></param>
    /// <param name="commentId"></param>
    /// <returns></returns>
    [HttpPost("/api/comments/{commentId}/unlike")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveLike(int commentId)
    {
        try
        {
            var comment = await _unitOfWork.CommentRepository
                .GetOneAsync(c => c.Id == commentId, default!, default!);
            if (comment == null)
                return BadRequest("Like doesn't exist Or already deleted.");
            var userId = User.Claims.Where(x => x.Type == "uid").FirstOrDefault()?.Value;
            await _unitOfWork.CommentRepository.RemoveLikeAsync(commentId, userId);
            await _unitOfWork.SaveAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("UnlikeComment", ex.Message);
        }
        return BadRequest(ModelState);
    }

}