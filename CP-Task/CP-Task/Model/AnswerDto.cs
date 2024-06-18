namespace CP_Task.Model
{
    public class AnswerDto
    {  /// <summary>
       /// Gets or sets the identifier of the question that this answer corresponds to.
       /// </summary>
        public string QuestionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the response provided for the question.
        /// </summary>
        public string Response { get; set; } = string.Empty;
    }

}
