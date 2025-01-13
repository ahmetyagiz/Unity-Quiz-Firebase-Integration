[System.Serializable]
public class TriviaResponse
{
    public int response_code;
    public QuestionData[] results;
}

[System.Serializable]
public class QuestionData
{
    public string category;
    public string type;
    public string difficulty;
    public string question;
    public string correct_answer;
    public string[] incorrect_answers;
}
