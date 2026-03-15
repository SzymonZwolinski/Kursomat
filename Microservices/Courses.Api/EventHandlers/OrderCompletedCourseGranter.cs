using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Courses.Api.Data;
using Courses.Api.Entities;
using MassTransit;
using Shared.Messaging.Events;

namespace Microservices.Courses.Api.EventHandlers
{
    public class OrderCompletedCourseGranter : IConsumer<OrderCompletedEvent>
    {
        private readonly CoursesDbContext _context;

        public OrderCompletedCourseGranter(CoursesDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
        {
            var message = context.Message;
            foreach (var courseId in message.CourseIds)
            {
                _context.Set<UserCourse>().Add(new UserCourse
                {
                    UserId = message.UserId,
                    CourseId = courseId
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
