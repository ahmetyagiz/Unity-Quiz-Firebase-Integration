using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class QuizManagerFirebase : MonoBehaviour
{
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;
    public TextMeshProUGUI scoreText;

    private List<string> answers = new List<string>();
    private int score = 0;

    // Firebase Database reference
    private DatabaseReference reference;

    public void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            reference = FirebaseDatabase.DefaultInstance.RootReference;
        });

        StartCoroutine(GetRandomQuestionFromFirebase());
    }

    // Firebase'den rastgele bir soru al
    IEnumerator GetRandomQuestionFromFirebase()
    {
        DatabaseReference questionRef = FirebaseDatabase.DefaultInstance.RootReference.Child("questions");
        FirebaseDatabase.DefaultInstance.GetReference("questions").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    ProcessQuestionData(snapshot);
                }
                else
                {
                    Debug.LogError("Veri bulunamadý.");
                }
            }
            else
            {
                Debug.LogError("Veri çekme hatasý: " + task.Exception);
            }
        });

        yield return null;
    }

    void ProcessQuestionData(DataSnapshot snapshot)
    {
        List<DataSnapshot> questionsList = new List<DataSnapshot>();
        foreach (var question in snapshot.Children)
        {
            questionsList.Add(question);
        }

        // Sorularý karýþtýr
        questionsList = questionsList.OrderBy(x => Random.value).ToList();

        // Ýlk soruyu al
        var firstQuestionData = questionsList.FirstOrDefault(); // Burada question yerine firstQuestionData kullandýk
        string questionText = firstQuestionData.Child("question").Value.ToString(); // Burada questionText kullandýk
        string correctAnswer = firstQuestionData.Child("correct_answer").Value.ToString();
        List<string> incorrectAnswers = new List<string>();

        foreach (var answer in firstQuestionData.Child("incorrect_answers").Children)
        {
            incorrectAnswers.Add(answer.Value.ToString());
        }

        // UI'yi güncelle
        this.questionText.text = questionText;
        answers.Clear();
        answers.Add(correctAnswer);
        answers.AddRange(incorrectAnswers);
        ShuffleAnswers();

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = answers[i];
            answerButtons[i].onClick.RemoveAllListeners();

            if (answers[i] == correctAnswer)
            {
                answerButtons[i].onClick.AddListener(CorrectAnswerSelected);
            }
            else
            {
                answerButtons[i].onClick.AddListener(WrongAnswerSelected);
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

        // Firebase'e puan kaydet
        SaveScoreToFirebase(score);

        StartCoroutine(GetRandomQuestionFromFirebase());
    }

    void WrongAnswerSelected()
    {
        Debug.Log("Yanlýþ cevap!");

        // Firebase'e puan kaydet
        SaveScoreToFirebase(score);

        StartCoroutine(GetRandomQuestionFromFirebase());
    }

    // Firebase'e puan kaydetme fonksiyonu
    void SaveScoreToFirebase(int newScore)
    {
        string userId = "user123";  // Örnek kullanýcý ID'si. Gerçek kullanýcý ID'sini kullanýn.
        string scoreKey = "score";

        reference.Child("users").Child(userId).Child(scoreKey).SetValueAsync(newScore).ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                Debug.Log("Puan Firebase'e baþarýyla kaydedildi.");
            }
            else
            {
                Debug.LogError("Puan kaydetme hatasý: " + task.Exception);
            }
        });
    }
}
