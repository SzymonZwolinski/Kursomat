using System;
using System.Collections.Generic;

namespace Courses.Api.Entities;

public class Course
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string VideoPath { get; set; }

    public ICollection<UserCourse> UserCourses { get; set; }
}
