using System;
using System.Collections.Generic;

namespace Kanban.Models;

public partial class Board
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsPrivate { get; set; }

    public virtual ICollection<Column> Columns { get; set; } = new List<Column>();

    public virtual ICollection<UserBoard> UserBoards { get; set; } = new List<UserBoard>();
}
