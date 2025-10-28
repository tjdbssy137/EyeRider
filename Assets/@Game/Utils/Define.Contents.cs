
public static partial class Define
{
    public enum EScene
    {
        Unknown,
        InGameScene
    }

    public enum EJoystickState
    {
        PointerDown,
        PointerUp,
        Drag
    }

    public enum EPlayerState
    {
        Ready = -2,
        Relax = -1,
        Idle = 0,
        Move = 1,
        Run = 2,
        Die = 3,
    }

    public enum EStatType
    {
        Hp,
        MaxHp,
        Mp,
        MaxMp,
        AttackPower,
        DefensePower,
        CriticalChance,
        CriticalDamage,
        MovementSpeed,
        AttackSpeed,
        SkillPower,
        SkillCooldownReduction,
        GoldFind,
        ExperienceGain,
    }

}
