using System;
using System.Threading;
using System.Threading.Tasks;
using Modular.Modules.Courses.Data;
using Modular.Modules.Courses.Entities;
using Modular.Shared.Events;

namespace Modular.Modules.Courses.EventHandlers
{
    public class OrderCompletedCourseGranter : IDomainEventHandler<OrderCompletedEvent>
    {
        private readonly CoursesDbContext _context;

        public OrderCompletedCourseGranter(CoursesDbContext context)
        {
            _context = context;
        }

        public async Task HandleAsync(OrderCompletedEvent domainEvent, CancellationToken ct = default)
        {
            foreach (var courseId in domainEvent.CourseIds)
            {
                _context.Set<UserCourse>().Add(new UserCourse
                {
                    Id = Guid.NewGuid(),
                    UserId = domainEvent.UserId,
                    CourseId = courseId
                });
            }

            await _context.SaveChangesAsync(ct);
        }
    }
}
