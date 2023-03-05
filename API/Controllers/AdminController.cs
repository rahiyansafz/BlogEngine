using System.Net.Mime;

using AutoMapper;

using DataAccess.Repositories.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Models.ApiModels.ResponseDTO;
using Models.Constants;
using Models.Entities;

using Services.Exceptions.Users;

namespace API.Controllers;
[Authorize(Roles = Roles.Admin)]
[Route("api/[controller]")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<AdminController> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Suspend a user by username
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    /// <exception cref="UserNotFoundException"></exception>
    [HttpGet("suspend-user/{username}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SuspendUser([FromRoute] string username)
    {
        try
        {
            var user = await _unitOfWork.AppUsers.SuspendByUsername(username) ?? throw new UserNotFoundException(
                    username
                    );
            _logger.LogInformation("User with usrname : {} is now suspended.", username);

            return Ok(
                _mapper.Map<AppUserAdminResponse>(user)
                );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "{Error} Executing {Action} with parameters {Parameters}.",
                    ex.Message, nameof(SuspendUser), username
                );

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Unsuspend a user by username
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    /// <exception cref="UserNotFoundException"></exception>
    [HttpGet("unsuspend-user/{username}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnSuspendUser([FromRoute] string username)
    {
        try
        {
            var user = await _unitOfWork.AppUsers.UnSuspendByUsername(username) ?? throw new UserNotFoundException(
                    username
                    );
            _logger.LogInformation("User with usrname : {} is now unsuspended.", username);

            return Ok(
                _mapper.Map<List<AppUser>>(user)
                );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "{Error} Executing {Action} with parameters {Parameters}.",
                    ex.Message, nameof(UnSuspendUser), username
                );

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
