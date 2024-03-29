﻿using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;

namespace Models.Entities;
public class AppUser : IdentityUser
{
    [Required]
    [MaxLength(20)]
    public string FirstName { get; set; }
    [Required]
    [MaxLength(20)]
    public string LastName { get; set; }
    public bool IsSuspended { get; set; } = false;
    public string? ProfileImagePath { get; set; }
    public ICollection<Blog> Blogs { get; set; }
}