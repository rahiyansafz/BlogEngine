﻿namespace Services.Exceptions.Users;
public class UsernameAlreadyExistsException : CustomException
{
    public UsernameAlreadyExistsException(string username)
        : base($"Username {username} Already Exits!")
    {
    }

}