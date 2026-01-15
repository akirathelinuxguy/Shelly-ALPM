using System;

namespace PackageManager.Alpm;

public class AlpmQuestionEventArgs(
    AlpmQuestionType questionType,
    string questionText)
    : EventArgs
{
    public AlpmQuestionType QuestionType { get; } = questionType;
    public string QuestionText { get; } = questionText;
    public int Response { get; set; } = 1; // Default to Yes (1)
}
