using System;
using System.Collections.Generic;

namespace Modular.Shared.Events
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
