using CP_Task.Data;

namespace CP_Task.Model
{
    public class QuestionDto
    {

        /// <summary>
        /// Gets or sets the unique identifier for the question.
        /// </summary>
        public string id { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the type of the question (e.g., Paragraph, YesNo, Dropdown, MultipleChoice, Date, Number).
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the text of the question.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the options for the question, applicable for question types like Dropdown and MultipleChoice.
        /// </summary>
        public List<string> Options { get; set; } = new List<string>();

        public static explicit operator QuestionDto(Question v)
        {
            return new QuestionDto() { Options = v.Options, Text = v.Text, Type = v.Type };
        }
    }

}
