using UnityEngine;

[System.Diagnostics.DebuggerStepThrough]
internal class RuntimePrefImpl : IPreferenceImpl
{
    private readonly string prefix;

    public RuntimePrefImpl(string prefix)
    {
        this.prefix = prefix;
    }

    private string GetPrefixed(string id)
    {
        return string.IsNullOrEmpty(prefix) ? id : prefix + id;
    }

    public float GetFloat(string key, float @default = 0) => PlayerPrefs.GetFloat(key, @default);
    public int GetInt(string key, int @default = 0) => PlayerPrefs.GetInt(key, @default);
    public string GetString(string key, string @default = null) => PlayerPrefs.GetString(key, @default);
    public void Set(string key, int value) => PlayerPrefs.SetInt(key, value);
    public void Set(string key, float value) => PlayerPrefs.SetFloat(key, value);
    public void Set(string key, string value) => PlayerPrefs.SetString(key, value);
    public void Delete(string key) => PlayerPrefs.DeleteKey(key);
    public void DeleteAll() => PlayerPrefs.DeleteAll();
    public bool Exists(string key) => PlayerPrefs.HasKey(key);
    public void Save() => PlayerPrefs.Save();
}
