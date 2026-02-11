using System.Text;
using TMPro;
using UnityEngine;

public class ReportController : MonoBehaviour
{
    [Header("Top Summary")]
    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private TMP_Text _totalExpText;
    [SerializeField] private TMP_Text _totalKillsText;

    [Header("List")]
    [SerializeField] private Transform _rowsRoot;
    [SerializeField] private ReportRow _rowPrefab;

    public void Hide() => gameObject.SetActive(false);

    public void Show(GameStats stats, float elapsedTime)
    {
        gameObject.SetActive(true);

        // 생존 시간
        int totalSec = Mathf.FloorToInt(elapsedTime);
        int min = totalSec / 60;
        int sec = totalSec % 60;
        if (_timeText) _timeText.SetText("{0:00}:{1:00}", min, sec);
        if (_totalExpText) _totalExpText.SetText("{0}", stats.TotalExp);
        if (_totalKillsText) _totalKillsText.SetText("{0}", stats.TotalKills);

        // 기존 row 삭제
        if (_rowsRoot)
        {
            for (int i = _rowsRoot.childCount - 1; i >= 0; i--) Destroy(_rowsRoot.GetChild(i).gameObject);
        }

        // 무기별 row 생성
        foreach (var kv in stats.Weapon)
        {
            var weaponId = kv.Key;
            var s = kv.Value;

            var row = Instantiate(_rowPrefab, _rowsRoot);
            row.Bind(weaponId, s.damage, s.kills);
        }
    }

}