namespace TodoApp.DTOs
{
    public class TodoDTO
    {
        public int Id { get; set; }   // Todo ID
        public int UserId { get; set; }   // Reference to User by ID
        public string Title { get; set; } = null!;  // Title of the Todo
        public string? Description { get; set; }   // Description of the Todo
        public bool? Completed { get; set; }   // Is it completed?
    }
}
