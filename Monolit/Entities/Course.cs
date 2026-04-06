namespace Monolit.Entities
{
	public class Course
	{
		public Guid Id { get; set; } = default!;
		public string Name { get; set; } = default!;
		public string Description { get; set; } = default!;
		public string? VideoPath { get; set; } = default!;
		public decimal Price { get; set; } = default!;
	}
}
