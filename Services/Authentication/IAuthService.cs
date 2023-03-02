using Models.ApiModels.Auth.Request;
using Models.ApiModels.Auth.Response;

namespace Services.Authentication;
public interface IAuthService
{
    Task<List<string>> RegisterAsync(RegistrationModelRequest model);
    Task<LoginModelResponse> LoginAsync(LoginModelRequest model);
    Task<LoginModelResponse> VerifyAndGenerateToken(TokenRequest tokenRequest);
    Task RevokeTokenAsync(string userId);
}