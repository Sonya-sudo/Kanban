using System;
using System.Collections.Generic;

namespace Kanban.Models;

public partial class TaskHistory
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public int UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string ActionType { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
