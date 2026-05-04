using System;
using System.Collections.Generic;

namespace Kanban.Models;

public partial class Subtask
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public string Title { get; set; } = null!;

    public bool? IsCompleted { get; set; }

    public int? Position { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<SubtaskHierarchy> SubtaskHierarchyParentSubtasks { get; set; } = new List<SubtaskHierarchy>();

    public virtual ICollection<SubtaskHierarchy> SubtaskHierarchySubtasks { get; set; } = new List<SubtaskHierarchy>();

    public virtual Task Task { get; set; } = null!;
}
