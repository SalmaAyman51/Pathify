using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pathify.Models;

namespace Pathify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseEnrollmentController : ControllerBase
    {
        private readonly PathifyContext _context;

        public CourseEnrollmentController(PathifyContext context)
        {
            _context = context;
        }

    }
}
