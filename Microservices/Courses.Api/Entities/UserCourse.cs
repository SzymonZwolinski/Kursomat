using System;

namespace Courses.Api.Entities;

public class UserCourse
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public Course Course { get; set; }
}
