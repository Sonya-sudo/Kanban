using System;
using System.Collections.Generic;

namespace Kanban.Models;

public partial class Column
{
    public int Id { get; set; }

    public int BoardId { get; set; }

    public string Name { get; set; } = null!;

    public int Position { get; set; }

    public int? WipLimit { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Board Board { get; set; } = null!;

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
