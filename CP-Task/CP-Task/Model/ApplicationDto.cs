﻿namespace CP_Task.Model
{
    public class ApplicationDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the application.
        /// </summary>
        public string? id { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the first name of the candidate who is submitting the application.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last name of the candidate who is submitting the application.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email of the candidate who is submitting the application.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of answers provided by the candidate.
        /// </summary>
        public List<AnswerDto> Answers { get; set; } = new List<AnswerDto>();
    }

}
