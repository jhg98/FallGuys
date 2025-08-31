using System.Threading.Tasks;

[System.Serializable]
public class Appearance
{
    public string color;
    public string pattern;
}

[System.Serializable]
public class Record
{
    public int wins;
    public float playTime;
}

[System.Serializable]
public class UserData
{
    public string uid;
    public string nickname;
    public Appearance appearance;
    public Record record;

    // 각 필드 경로를 상수로 지정
    private const string UidPath = "/uid";
    private const string NicknamePath = "/nickname";
    private const string AppearancePath = "/appearance";
    // 외형은 한번에 처리되므로 단일 경로 불필요
    private const string RecordPath = "/record";
    private const string WinsPath = RecordPath + "/wins";
    private const string PlayTimePath = RecordPath + "/playTime";

    public UserData(string uid)
    {
        this.uid = uid;
    }
    public UserData(string uid, string nickname, Appearance appearance, Record record)
    {
        this.uid = uid;
        this.nickname = nickname;
        this.appearance = appearance;
        this.record = record;
    }

    #region 저장
    // 사용자 전체 정보 저장
    public void SaveUserData(FirebaseDBManager dbManager)
    {
        //dbManager.SetJsonData(GetPath(), this);
        UpdateFieldByJson(dbManager, "", this);
    }
    // 닉네임 업데이트
    public void UpdateNickname(FirebaseDBManager dbManager, string nickname)
    {
        UpdateFieldValue(dbManager, NicknamePath, nickname);
    }
    // 외형 업데이트
    public void UpdateAppearance(FirebaseDBManager dbManager, Appearance appearance)
    {
        UpdateFieldByJson(dbManager, AppearancePath, appearance);
    }
    // 우승 횟수 업데이트
    public void UpdateWins(FirebaseDBManager dbManager, int wins)
    {
        UpdateFieldValue(dbManager, WinsPath, wins);
    }
    // 플레이 시간 업데이트
    public void UpdatePlayTime(FirebaseDBManager dbManager, float playTime)
    {
        UpdateFieldValue(dbManager, PlayTimePath, playTime);
    }

    // 특정 필드 업데이트(JSON)
    private void UpdateFieldByJson<T>(FirebaseDBManager dbManager, string fieldPath, T data)
    {
        dbManager.SetJsonData(GetPath() + fieldPath, data);
    }
    // 특정 필드 업데이트(값)
    private void UpdateFieldValue(FirebaseDBManager dbManager, string fieldPath, object value)
    {
        dbManager.SetValueData(GetPath() + fieldPath, value);
    }
    #endregion

    #region 가져오기
    // 전체 유저 정보 가져오기
    public async Task<UserData> GetUserData(FirebaseDBManager dbManager)
    {
        return await GetFieldData<UserData>(dbManager, "");
    }

    // 특정 데이터 가져오기
    private async Task<T> GetFieldData<T>(FirebaseDBManager dbManager, string fieldPath)
    {
        return await dbManager.GetData<T>(GetPath() + fieldPath);
    }
    #endregion

    private string GetPath()
    {
        return $"users/{uid}";
    }
}
