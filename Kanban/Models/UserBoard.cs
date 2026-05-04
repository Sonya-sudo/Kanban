using System;
using System.Collections.Generic;

namespace Kanban.Models;

public partial class UserBoard
{
    public int Id { get; set; }

    public int BoardId { get; set; }

    public int UserRoleId { get; set; }

    public virtual Board Board { get; set; } = null!;

    public virtual UserRole UserRole { get; set; } = null!;
}
