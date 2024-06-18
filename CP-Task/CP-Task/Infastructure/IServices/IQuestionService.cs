using CP_Task.Model;

namespace CP_Task.Infastructure.IServices
{
    public interface IQuestionService
    {
        Task<string> AddQuestionAsync(QuestionDto question);
        Task DeleteQuestionAsync(string id);
        Task<QuestionDto> GetQuestionAsync(string id);
        Task<IEnumerable<QuestionDto>> GetQuestionsAsync();
        Task UpdateQuestionAsync(string id, QuestionDto question);
    }
}
