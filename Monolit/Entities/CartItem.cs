namespace Monolit.Entities
{
    public class CartItem
    {
        public Guid Id { get; set; }
        public Guid CartId { get; set; }
        public Cart Cart { get; set; }
        public Guid CourseId { get; set; }
        public Course Course { get; set; }
    }
}
