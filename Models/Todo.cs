using System;
using System.Collections.Generic;

namespace TodoApp.Models;

public partial class Todo
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public bool? Completed { get; set; }

    public virtual User User { get; set; } = null!;
}
