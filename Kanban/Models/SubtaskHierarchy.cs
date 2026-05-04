using System;
using System.Collections.Generic;

namespace Kanban.Models;

public partial class SubtaskHierarchy
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public int SubtaskId { get; set; }

    public int? ParentSubtaskId { get; set; }

    public int Level { get; set; }

    public int Order { get; set; }

    public virtual Subtask? ParentSubtask { get; set; }

    public virtual Subtask Subtask { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
