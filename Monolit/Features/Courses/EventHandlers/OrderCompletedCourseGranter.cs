using System.Threading;
using System.Threading.Tasks;
using Monolit.DataBase;
using Monolit.Entities;
using Monolit.Features.Orders.Events;
using Monolit.Helpers.DomainEvents;

namespace Monolit.Features.Courses.EventHandlers
{
    public class OrderCompletedCourseGranter : IDomainEventHandler<OrderCompletedEvent>
    {
        private readonly MonolitDbContext _context;

        public OrderCompletedCourseGranter(MonolitDbContext context)
        {
            _context = context;
        }

        public async Task HandleAsync(OrderCompletedEvent domainEvent, CancellationToken ct = default)
        {
            foreach (var courseId in domainEvent.CourseIds)
            {
                _context.UserCourses.Add(new UserCourse
                {
                    UserId = domainEvent.UserId,
                    CourseId = courseId
                });
            }

            await _context.SaveChangesAsync(ct);
        }
    }
}
