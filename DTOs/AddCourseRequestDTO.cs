namespace Pathify.DTOs
{
    public class AddCourseRequestDTO
    {
        public class AddCoursesRequest
        {
            public List<string> CourseIds { get; set; } = new();
        }
    }
}
