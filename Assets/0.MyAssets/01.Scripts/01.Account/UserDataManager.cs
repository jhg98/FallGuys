using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 현재 유저의 정보를 가지고 있는 클래스
public class UserDataManager : MonoBehaviour
{
    // 게임 전체에 하나만 존재하며, 어디서든 접근 가능해야하므로 싱글톤으로 구현
    public static UserDataManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public UserData CurrentUserData { get; private set; }

    // 유저 데이터 세팅
    public void SetUserData(UserData data)
    {
        CurrentUserData = data;
    }
}
