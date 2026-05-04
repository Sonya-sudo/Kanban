using System;
using System.Collections.Generic;

namespace Kanban.Models;

public partial class UserRole
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int RoleId { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual ICollection<UserBoard> UserBoards { get; set; } = new List<UserBoard>();

    public virtual ICollection<UserTask> UserTasks { get; set; } = new List<UserTask>();
}
