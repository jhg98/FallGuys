using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

// Firebase DB 처리 클래스
public class FirebaseDBManager
{
    private DatabaseReference reference;

    public FirebaseDBManager()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    // 해당 경로에 JSON 형식으로 데이터 저장
    public void SetJsonData<T>(string path, T data)
    {
        string json = JsonUtility.ToJson(data);
        reference.Child(path).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"데이터가 {path}에 성공적으로 저장되었습니다.");
            }
            else
            {
                Debug.LogError($"데이터 저장 실패: {path}, 오류: {task.Exception}");
            }
        });
    }

    // 해당 경로에 값 저장
    public void SetValueData(string path, object value)
    {
        reference.Child(path).SetValueAsync(value).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"값이 {path}에 성공적으로 저장되었습니다: {value}");
            }
            else
            {
                Debug.LogError($"값 저장 실패: {path}, 오류: {task.Exception}");
            }
        });
    }

    // 데이터 가져오기
    public async Task<T> GetData<T>(string path)
    {
        var dataSnapshot = await reference.Child(path).GetValueAsync();
        if (dataSnapshot.Exists)
        {
            return JsonUtility.FromJson<T>(dataSnapshot.GetRawJsonValue());
        }
        else
        {
            Debug.LogWarning($"{path}에 데이터가 존재하지 않습니다.");
            return default;
        }

    }
}
