using CP_Task.Data;
using CP_Task.Infastructure.IServices;
using CP_Task.Model;
using Microsoft.AspNetCore.Mvc;
using System.Web.Http.Description;

namespace CP_Task.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionService _questionService;

        public QuestionsController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Question>>> GetQuestions()
        {
            var questions = await _questionService.GetQuestionsAsync();
            return Ok(questions);
        }

        [HttpGet("{id}"), ResponseType(typeof(QuestionDto))]
        public async Task<ActionResult<Question>> GetQuestion(string id)
        {
            var question = await _questionService.GetQuestionAsync(id);
            if (question == null)
            {
                return NotFound();
            }
            return Ok(question);
        }

        [HttpPost]
        public async Task<ActionResult> AddQuestion(QuestionDto questionDto)
        {

            var id = await _questionService.AddQuestionAsync(questionDto);
            return Ok(new { id });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateQuestion(QuestionDto questionDto)
        {
            var id = await _questionService.UpdateQuestionAsync(questionDto.id, questionDto);
            if (id == null)
            {
                return NotFound();
            }
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteQuestion(string id)
        {
            var question = await _questionService.GetQuestionAsync(id);
            if (question == null)
            {
                return NotFound();
            }
            await _questionService.DeleteQuestionAsync(id);
            return Ok();
        }
    }
}
