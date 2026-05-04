using System;
using System.Collections.Generic;

namespace Kanban.Models;

public partial class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<TaskHistory> TaskHistories { get; set; } = new List<TaskHistory>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
