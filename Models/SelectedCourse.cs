namespace Pathify.Models
{
    public class SelectedCourse
    {

        //public int Id { get; set; }
        public string StudentSsn { get; set; } = null!;
        public string CourseId { get; set; } = null!;
        public DateTime SelectedAt { get; set; } = DateTime.Now;

    }
}

