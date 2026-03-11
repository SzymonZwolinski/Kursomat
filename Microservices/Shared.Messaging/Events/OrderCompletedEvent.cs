using System;
using System.Collections.Generic;

namespace Shared.Messaging.Events
{
    public class OrderCompletedEvent
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
