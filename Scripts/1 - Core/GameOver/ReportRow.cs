using TMPro;
using UnityEngine;

public class ReportRow : MonoBehaviour
{
    [SerializeField] private TMP_Text _weaponName;
    [SerializeField] private TMP_Text _damage;
    [SerializeField] private TMP_Text _kills;

    public void Bind(string weaponId, float damage, int kills)
    {
        if (_weaponName) _weaponName.SetText(weaponId);
        if (_damage) _damage.SetText("{0:0}", damage);
        if (_kills) _kills.SetText("{0}", kills);
    }
}