namespace Pathify.DTOs
{
    public class EnrollCoursesDTO
    {
        public class EnrollCoursesDto
        {
            public string StudentSSN { get; set; }
            public List<string> CourseIds { get; set; }
            
        }
    }
}
