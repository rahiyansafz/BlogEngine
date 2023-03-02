namespace Services.Exceptions.Users;
public class NotValidUsernameException : CustomException
{
    public NotValidUsernameException(string username)
        : base($"Username {username} is not valid!")
    {

    }
}