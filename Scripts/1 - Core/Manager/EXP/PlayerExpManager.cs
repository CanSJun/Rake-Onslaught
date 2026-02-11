using System;

public class PlayerExpManager
{
    public int Level { get; private set; } = 1;
    public int CurrentExp { get; private set; }

    public int ExpToNext
    {

        get
        {
            const int baseExp = 2;
            const double growth = 1.12;
            const int linear = 4;
            return (int)Math.Ceiling(baseExp * Math.Pow(growth, Level - 1)) + (Level - 1) * linear;
        }
    }

    public float Normalized => ExpToNext <= 0 ? 0f : (float)CurrentExp / ExpToNext;

    public event Action<int, int, int> OnChanged; // Current, Next, Level
    public event Action<int> OnLevelUp;

    private float _expMultiplier = 1f;
    public void ApplyExpMultiplier(float mul)
    {
        if (mul <= 0f) mul = 0.01f;
        _expMultiplier *= mul;
    }
    public int AddExp(int exp)
    {
        if (exp <= 0) return 0;

        int gained = (int)Math.Ceiling(exp * _expMultiplier);
        if (gained <= 0) return 0;

        CurrentExp += gained;
        while (CurrentExp >= ExpToNext)
        {
            CurrentExp -= ExpToNext;
            Level++;
            OnLevelUp?.Invoke(Level);
        }

        OnChanged?.Invoke(CurrentExp, ExpToNext, Level);
        return gained;
    }

    public void Reset()
    {
        Level = 1;
        CurrentExp = 0;
        OnChanged?.Invoke(CurrentExp, ExpToNext, Level);
    }
}