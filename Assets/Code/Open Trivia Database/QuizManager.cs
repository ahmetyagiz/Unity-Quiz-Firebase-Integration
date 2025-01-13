using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Net;

public class QuizManager : MonoBehaviour
{
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;
    public TextMeshProUGUI scoreText;

    private List<string> answers = new List<string>();
    private int score = 0;

    private string apiUrl = "https://opentdb.com/api.php?amount=1&type=multiple";

    public void Start()
    {
        StartCoroutine(GetQuestionData());
    }

    IEnumerator GetQuestionData()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ProcessQuestionData(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Veri çekme hatasý: " + request.error);
            }
        }
    }

    void ProcessQuestionData(string jsonData)
    {
        TriviaResponse response = JsonUtility.FromJson<TriviaResponse>(jsonData);
        if (response.results.Length > 0)
        {
            var questionData = response.results[0];

            // HTML karakterlerini temizle
            questionText.text = WebUtility.HtmlDecode(questionData.question);

            for (int i = 0; i < questionData.incorrect_answers.Length; i++)
            {
                questionData.incorrect_answers[i] = WebUtility.HtmlDecode(questionData.incorrect_answers[i]);
            }

            // Doðru ve yanlýþ cevaplarý karýþtýr
            answers.Clear();
            answers.Add(questionData.correct_answer);
            answers.AddRange(questionData.incorrect_answers);
            ShuffleAnswers();

            // UI'yi güncelle
            for (int i = 0; i < answerButtons.Length; i++)
            {
                answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = answers[i];
                answerButtons[i].onClick.RemoveAllListeners();

                if (answers[i] == questionData.correct_answer)
                {
                    answerButtons[i].onClick.AddListener(CorrectAnswerSelected);
                }
                else
                {
                    answerButtons[i].onClick.AddListener(WrongAnswerSelected);
                }
            }
        }
    }

    void ShuffleAnswers()
    {
        for (int i = 0; i < answers.Count; i++)
        {
            int randomIndex = Random.Range(i, answers.Count);
            string temp = answers[i];
            answers[i] = answers[randomIndex];
            answers[randomIndex] = temp;
        }
    }

    void CorrectAnswerSelected()
    {
        score++;
        scoreText.text = "Puan: " + score;
        StartCoroutine(GetQuestionData());
    }

    void WrongAnswerSelected()
    {
        Debug.Log("Yanlýþ cevap!");
        StartCoroutine(GetQuestionData());
    }
}
