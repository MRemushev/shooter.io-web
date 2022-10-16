using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitch : MonoBehaviour
{
    public List<string> weaponsName = new();

    public int WeaponLevel { get; private set; }

    private void Awake()
    {
        foreach (Transform weapon in transform) weaponsName.Add(weapon.gameObject.name);
    }

    public void ChangeWeapon(int level)
    {
        var index = 0;
        WeaponLevel = level;
        foreach (Transform weapon in transform)
        {
            weapon.gameObject.SetActive(index == WeaponLevel);
            index++;
        }
    }
}
