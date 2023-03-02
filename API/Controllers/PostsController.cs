﻿using System.Net.Mime;

using AutoMapper;

using DataAccess.Repositories.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Models.ApiModels.RequestDTO;
using Models.ApiModels.ResponseDTO;
using Models.Entities;
using Models.QueryParameters;

using Services.Exceptions.Blogs;
using Services.Extensions;

namespace API.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PostsController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all posts for some blog
    /// </summary>
    /// <param name="blogId"></param>
    /// <param name="postParameters"></param>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    [Route("/api/posts")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PostResponse>))]
    public async Task<IActionResult> Get([FromQuery] PostParameters postParameters)
    {
        try
        {
            var userId = User.Claims.Where(x => x.Type == "uid").FirstOrDefault()?.Value;
            postParameters.UsreId = userId;

            var posts = await _unitOfWork.PostRepository
                .GetPostsAsync(postParameters);

            Response.AddPaginationHeader(
               currentPage: posts.CurrentPage,
               itemsPerPage: posts.PageSize,
               totalItems: posts.TotalCount,
               totalPages: posts.TotalPages
            );

            return Ok(
                 _mapper.Map<List<PostResponse>>(posts)
                 );
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("GetPagePost", ex.Message);
            return BadRequest(ModelState);
        }

    }

    /// <summary>
    /// Get one post by id
    /// </summary>
    /// <param name="postId"></param>
    /// <returns></returns>
    /// <exception cref="PostNotFoundException"></exception>
    [AllowAnonymous]
    [HttpGet("/api/posts/{postId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int postId)
    {
        try
        {
            var userId = User.Claims.Where(x => x.Type == "uid").FirstOrDefault()?.Value;
            var post = await _unitOfWork.PostRepository.GetOneAsync(postId, userId);
            if (post == null)
                return NotFound($"Post with id {postId} not found.");

            return Ok(
                    _mapper.Map<PostResponse>(post)
                );
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("GetPostById", ex.Message);
            return BadRequest(ModelState);
        }
    }

    /// <summary>
    /// Add one post to database
    /// </summary>
    /// <param name="blogId"></param>
    /// <param name="postModel"></param>
    /// <returns></returns>
    /// <exception cref="BlogNotFoundException"></exception>
    [HttpPost("/api/posts")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromBody] PostRequest postModel)
    {
        try
        {
            var userId = User.Claims.Where(x => x.Type == "uid").FirstOrDefault()?.Value;
            var blog = await _unitOfWork.BlogRepository
                .GetOneAsync(b => b.Id == postModel.BlogId, tracked: true);

            if (blog == null)
            {
                throw new BlogNotFoundException(postModel.BlogId);
            }
            if (blog.UserId != userId)
            {
                return Unauthorized();
            }

            var post = _mapper.Map<Post>(postModel);
            post.UserId = userId;
            blog.Posts.Add(post);
            await _unitOfWork.SaveAsync();

            return Created(
                $"~api/blogs/{blog.Id}/posts",
                postModel
                );

        }
        catch (Exception ex)
        {
            ModelState.AddModelError("AddPost", ex.Message);
        }

        return BadRequest(ModelState);
    }

    /// <summary>
    /// Update post
    /// </summary>
    /// <param name="postId"></param>
    /// <param name="postModel"></param>
    /// <returns></returns>
    [HttpPut("/api/posts/{postId}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Put(int postId, [FromBody] PostRequest postModel)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var post = await _unitOfWork.PostRepository
                    .GetOneAsync(p => p.Id == postId, default!, default!);
                if (post == null)
                {
                    return BadRequest("Post doen't exist in database");
                }
                var userId = User.Claims.Where(x => x.Type == "uid").FirstOrDefault()?.Value;
                if (post.UserId != userId)
                {
                    return Unauthorized();
                }
                post.HeadLine = postModel.HeadLine;
                post.Content = postModel.Content;

                await _unitOfWork.PostRepository.UpdateAsync(post);
                await _unitOfWork.SaveAsync();

                return NoContent();
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("UpdatePost", ex.Message);
        }

        return BadRequest(ModelState);
    }

    /// <summary>
    /// Delete post
    /// </summary>
    /// <param name="postId"></param>
    /// <returns></returns>
    [HttpDelete("/api/posts/{postId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int postId)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var post = await _unitOfWork.PostRepository
                    .GetOneAsync(p => p.Id == postId, default!, default!);
                if (post == null)
                {
                    return BadRequest(ModelState);
                }
                var userId = User.Claims.Where(x => x.Type == "uid").FirstOrDefault()?.Value;
                if (post.UserId != userId)
                {
                    return Unauthorized();
                }
                await _unitOfWork.PostRepository.RemoveAsync(post);
                await _unitOfWork.SaveAsync();

                return NoContent();
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("DeletePost", ex.Message);
        }

        return BadRequest(ModelState);
    }

    /// <summary>
    /// Add like to post
    /// </summary>
    /// <param name="postId"></param>
    /// <returns></returns>
    [HttpPost("/api/posts/{postId}/like")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LikePost(int postId)
    {
        try
        {
            var Post = await _unitOfWork.PostRepository
                .GetOneAsync(c => c.Id == postId, default!, default!);
            if (Post == null) return BadRequest();
            var userId = User.Claims.Where(x => x.Type == "uid").FirstOrDefault()?.Value;
            await _unitOfWork.PostRepository.AddLikeAsync(Post.Id, userId);
            await _unitOfWork.SaveAsync();

            return Created($"~api/posts/{postId}/add-tag",
                _mapper.Map<PostResponse>(Post)
            );
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("LikePost", ex.Message);
        }
        return BadRequest(ModelState);
    }

    /// <summary>
    /// Remove like from post
    /// </summary>
    /// <param name="postId"></param>
    /// <returns></returns>
    [HttpPost("/api/posts/{postId}/unlike")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnLikePost(int postId)
    {
        try
        {
            var Post = await _unitOfWork.PostRepository
                .GetOneAsync(c => c.Id == postId, default!, default!);
            if (Post == null) return BadRequest("Post doesn't Exits.");
            var userId = User.Claims.Where(x => x.Type == "uid").FirstOrDefault()?.Value;
            await _unitOfWork.PostRepository.RemoveLikeAsync(Post.Id, userId);
            await _unitOfWork.SaveAsync();

            return Created($"~api/posts/{postId}/add-tag",
                _mapper.Map<PostResponse>(Post)
            );
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("UnlikePost", ex.Message);
        }
        return BadRequest(ModelState);
    }

    /// <summary>
    /// Get all users' likes for a post
    /// </summary>
    /// <param name="postId"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("/api/posts/{postId}/likes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Likes(int postId)
    {

        var Post = await _unitOfWork.PostRepository.GetOneAsync(c => c.Id == postId);
        if (Post == null) return BadRequest();
        var usersLikes = await _unitOfWork.PostRepository.GetLikesAsync(postId);

        return Ok(
            _mapper.Map<List<AppUserResponse>>(usersLikes)
        );
    }

    /// <summary>
    ///  Get all comments for a post
    /// </summary>
    /// <param name="postId"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("/api/posts/{postId}/comments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComments(int postId)
    {
        var userId = User.Claims.Where(x => x.Type == "uid").FirstOrDefault()?.Value;
        var comments = await _unitOfWork.CommentRepository
            .GetAllCommentstAsync(postId, userId);
        var commentResponse = _mapper.Map<List<CommentResponse>>(comments);

        return Ok(
            commentResponse
            );
    }

    /// <summary>
    /// Add Tag to your post
    /// </summary>
    /// <param name="postId"></param>
    /// <param name="tagName"></param>
    /// <returns></returns>
    [HttpGet("/api/posts/{postId}/add-tag")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TagPost([FromRoute] int postId, [FromQuery] string tagName)
    {
        try
        {
            var post = await _unitOfWork.PostRepository
                .GetOneAsync(c => c.Id == postId, "PostTags", tracked: true);
            if (Post == null)
                return BadRequest();

            var userId = User.Claims.Where(x => x.Type == "uid").FirstOrDefault()?.Value;
            if (post.UserId != userId)
                return Unauthorized();

            Tag? tag = await _unitOfWork.PostRepository.GetTagByName(tagName);
            if (tag == null)
                return BadRequest(
                    "Not valid tag name."
                    );
            if (post.PostTags.Any(x => x.Tag.Id == tag.Id))
                return BadRequest(
                    "Tag already on post."
                    );

            post.PostTags.Add(new PostTag { TagId = tag.Id });
            await _unitOfWork.SaveAsync();

            return Created($"~api/posts/{postId}/add-tag",
                _mapper.Map<PostResponse>(post)
            );
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("GetPostlikes", ex.Message);
        }
        return BadRequest(ModelState);
    }

    /// <summary>
    /// Remove tag from your post
    /// </summary>
    /// <param name="postId"></param>
    /// <param name="tagName"></param>
    /// <returns></returns>
    [HttpGet("/api/posts/{postId}/remove-tag")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveTagPost([FromRoute] int postId, [FromQuery] string tagName)
    {
        try
        {
            if (tagName == null)
            {
                return BadRequest(
                    "You must Specify tagname in request query."
                    );
            }
            var post = await _unitOfWork.PostRepository
                .GetOneAsync(c => c.Id == postId, "PostTags", tracked: true);
            if (Post == null)
                return BadRequest();

            var userId = User.Claims.Where(x => x.Type == "uid").FirstOrDefault()?.Value;
            if (post.UserId != userId)
                return Unauthorized(
                    "This post doenst belong to you."
                    );

            Tag? tag = await _unitOfWork.PostRepository.GetTagByName(tagName);
            if (tag == null)
                return BadRequest(
                    "Not a valid tag name."
                    );

            if (post.PostTags.Any(x => x.Tag.Id == tag.Id))
                return BadRequest(
                    "Tag already on post."
                    );

            post.PostTags.Remove(new PostTag { TagId = tag.Id });
            await _unitOfWork.SaveAsync();

            return Created(
                $"~api/posts/{postId}/remove-tag",
                _mapper.Map<PostResponse>(post)
            );
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("GetPostlikes", ex.Message);
        }
        return BadRequest(ModelState);
    }


}