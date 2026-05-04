using System;
using System.Collections.Generic;

namespace Kanban.Models;

public partial class UserTask
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public int UserRoleId { get; set; }

    public virtual Task Task { get; set; } = null!;

    public virtual UserRole UserRole { get; set; } = null!;
}
