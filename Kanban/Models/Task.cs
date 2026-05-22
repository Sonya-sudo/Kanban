using System;
using System.Collections.Generic;

namespace Kanban.Models;

public partial class Task
{
    public int Id { get; set; }

    public int ColumnId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Priority { get; set; }

    public DateTime? Deadline { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int OrderInColumn { get; set; }

    public bool? Escalated { get; set; }

    public int? StopperCount { get; set; }

    public string? Color { get; set; }

    public int? AssigneeId { get; set; }

    public string Status { get; set; } = "InProgress"; // "InProgress" или "Completed"

    public virtual Column Column { get; set; } = null!;

    public virtual ICollection<SubtaskHierarchy> SubtaskHierarchies { get; set; } = new List<SubtaskHierarchy>();

    public virtual ICollection<Subtask> Subtasks { get; set; } = new List<Subtask>();

    public virtual ICollection<TaskHistory> TaskHistories { get; set; } = new List<TaskHistory>();

    public virtual ICollection<UserTask> UserTasks { get; set; } = new List<UserTask>();
}
