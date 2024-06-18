using CP_Task.Data;
using CP_Task.Infastructure.IServices;
using CP_Task.Model;
using Microsoft.AspNetCore.Mvc;

namespace CP_Task.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationsService _applicationService;

        public ApplicationsController(IApplicationsService applicationService)
        {
            _applicationService = applicationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Application>>> GetApplications()
        {
            var applications = await _applicationService.GetApplicationsAsync();
            return Ok(applications);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Application>> GetApplication(string id)
        {
            var application = await _applicationService.GetApplicationAsync(id);
            if (application == null)
            {
                return NotFound();
            }
            return Ok(application);
        }

        [HttpPost]
        public async Task<ActionResult> AddApplication(ApplicationDto applicationDto)
        {

            var id = await _applicationService.AddApplicationAsync(applicationDto);
            return Ok(new { id });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateApplication(ApplicationDto applicationDto)
        {
            var id = await _applicationService.UpdateApplicationAsync(applicationDto.id, applicationDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteApplication(string id)
        {
            var application = await _applicationService.GetApplicationAsync(id);
            if (application == null)
            {
                return NotFound();
            }
            await _applicationService.DeleteApplicationAsync(id);
            return NoContent();
        }
    }
}
