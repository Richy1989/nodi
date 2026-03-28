namespace nodiCore.Models;

/// <summary>Application roles. Admin users have access to the /api/admin endpoints.</summary>
public enum UserRole { User, Admin }

/// <summary>
/// An authenticated account in the system. Each user owns their own notes and tags;
/// data is never shared between users.
/// </summary>
public class User
{
    public int Id { get; set; }

    /// <summary>Unique login name. Enforced at the database level with a unique index.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Unique email address. Enforced at the database level with a unique index.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>BCrypt hash of the user's password. The raw password is never stored.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.User;

    /// <summary>Inactive users cannot log in. Admins can toggle this via /api/admin/users/{id}.</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UI colour scheme preference: "Light" or "Dark". Persisted per user.</summary>
    public string Theme { get; set; } = "Dark";

    public ICollection<Note> Notes { get; set; } = [];
    public ICollection<Tag> Tags { get; set; } = [];
}
