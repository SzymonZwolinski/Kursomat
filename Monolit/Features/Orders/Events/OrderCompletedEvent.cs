using System;
using System.Collections.Generic;
using Monolit.Helpers.DomainEvents;

namespace Monolit.Features.Orders.Events
{
    public class OrderCompletedEvent : IDomainEvent
    {
        public Guid UserId { get; set; }
        public Guid OrderId { get; set; }
        public IEnumerable<Guid> CourseIds { get; set; }

        public OrderCompletedEvent(Guid userId, Guid orderId, IEnumerable<Guid> courseIds)
        {
            UserId = userId;
            OrderId = orderId;
            CourseIds = courseIds;
        }
    }
}
