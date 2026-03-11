using System;

namespace Modular.Modules.Courses.Entities
{
    public class UserCourse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid CourseId { get; set; }
    }
}
