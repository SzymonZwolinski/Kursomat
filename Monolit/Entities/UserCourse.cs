namespace Monolit.Entities
{
    public class UserCourse
    {
        public Guid Id { get; set; } = default!;
        public Guid UserId { get; set; } = default!;
        public User User { get; set; } = default!;
        public Guid CourseId { get; set; } = default!;
        public Course Course { get; set; } = default!;
    }
}
