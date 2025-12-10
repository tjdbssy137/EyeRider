using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Data
{


    [Serializable]
    public class DifficultyData : IData
    {
        public int Id;

        int IData.Id
        {
            get => Id;
        }

        public int Level;

        public float DamagePerSec;
        public float StormSpeed;
        public float EyeSize;
        public float ObstacleDensity;

        public float RandomMoveMul;
        public float PullMul;
        public float ApproachMul;
        public float RepelMul;
        public float DistanceM;
    }

    [Serializable]
    public class DifficultyDataLoader : ILoader<int, DifficultyData>
    {
        public List<DifficultyData> datas = new List<DifficultyData>();

        public Dictionary<int, DifficultyData> MakeDict()
        {
            Dictionary<int, DifficultyData> dict = new Dictionary<int, DifficultyData>();
            foreach (DifficultyData data in datas)
                dict.Add(data.Id, data);

            return dict;
        }
    }


    // [Serializable]
    // public class AbilityData : IData
    // {
    //     public enum EType
    //     {
    //         Static,
    //         Around,
    //         Follow,
    //     }

    //     [JsonProperty("Id")]
    //     public int Id { get; set; }
    //     public int WeaponId { get; set; }

    //     //WEAPON을 어떻게 생성할건지에 대한
    //     public EType Type { get; set; }
    //     public int Count { get; set; }
    // }


    // [Serializable]
    // public class WeaponData : IData
    // {
    //     public enum EType
    //     {
    //         Missile,
    //         GuidedMissile,
    //         MagneticField,
    //         Laser,
    //         //드릴
    //     }

    //     [JsonProperty("Id")]
    //     public int Id { get; set; }
    //     public int BulletId { get; set; }

    //     //BULLET을 어떻게 생성할건지에 대한
    //     public EType Type { get; set; }
    //     public int Count { get; set; }
    //     public float CoolDown { get; set; }

    // }


    // [Serializable]
    // public class BulletData : IData
    // {
    //     public enum EPositionType
    //     {
    //         Parent,
    //         World,
    //     }

    //     [JsonProperty("Id")]
    //     public int Id { get; set; }

    //     //BULLET을 어떻게 생성할건지에 대한
    //     public EPositionType Type { get; set; }
    //     public float LifeTime { get; set; }
    //     public float Speed { get; set; }
    //     public float Damage { get; set; }
    // }

    // public class DummyData
    // {
    //     private Dictionary<int, AbilityData> _abilityDataDict = new Dictionary<int, AbilityData>()
    //     {
    //         { 10001, new AbilityData() { Id = 10001, WeaponId = 10101, Type = AbilityData.EType.Static, Count = 1 } },
    //         { 10002, new AbilityData() { Id = 10002, WeaponId = 10102, Type = AbilityData.EType.Around, Count = 5 } },
    //         { 10003, new AbilityData() { Id = 10003, WeaponId = 10103, Type = AbilityData.EType.Follow, Count = 3 } },
    //         { 10004, new AbilityData() { Id = 10004, WeaponId = 10104, Type = AbilityData.EType.Static, Count = 1 } },
    //         { 10005, new AbilityData() { Id = 10005, WeaponId = 10105, Type = AbilityData.EType.Static, Count = 1 } },
    //         // enemy
    //         { 20001, new AbilityData() { Id = 20001, WeaponId = 20101, Type = AbilityData.EType.Static, Count = 1 } },


    //     };

    //     private Dictionary<int, WeaponData> _weaponDataDict = new Dictionary<int, WeaponData>()
    //     {
    //         { 10101, new WeaponData() { Id = 10101, BulletId = 10201, Type = WeaponData.EType.Missile, Count = 1, CoolDown = 2.5f } },
    //         { 10102, new WeaponData() { Id = 10102, BulletId = 10202, Type = WeaponData.EType.Missile, Count = 7, CoolDown = 0.3f } },
    //         { 10103, new WeaponData() { Id = 10103, BulletId = 10203, Type = WeaponData.EType.GuidedMissile, Count = 1, CoolDown = 4.0f } },
    //         { 10104, new WeaponData() { Id = 10104, BulletId = 10204, Type = WeaponData.EType.MagneticField, Count = 1, CoolDown = 10.0f } },
    //         { 10105, new WeaponData() { Id = 10105, BulletId = 10205, Type = WeaponData.EType.Laser, Count = 1, CoolDown = 3.0f } },
    //         // enemy
    //         { 20101, new WeaponData() { Id = 20101, BulletId = 20201, Type = WeaponData.EType.Missile, Count = 1, CoolDown = 0.5f } },

    //     };

    //     private Dictionary<int, BulletData> _bulletDataDict = new Dictionary<int, BulletData>()
    //     {
    //         { 10201, new BulletData() { Id = 10201, Type = BulletData.EPositionType.Parent, LifeTime = 2.0f, Speed = 10.0f, Damage = 5.0f } },
    //         { 10202, new BulletData() { Id = 10202, Type = BulletData.EPositionType.World, LifeTime = 0.2f, Speed = 20.0f, Damage = 7.0f } },
    //         { 10203, new BulletData() { Id = 10203, Type = BulletData.EPositionType.Parent, LifeTime = 3.0f, Speed = 7.0f, Damage = 10.0f } },
    //         { 10204, new BulletData() { Id = 10204, Type = BulletData.EPositionType.Parent, LifeTime = 3.0f, Speed = 10.0f, Damage = 6.0f } },
    //         { 10205, new BulletData() { Id = 10205, Type = BulletData.EPositionType.World, LifeTime = 1.5f, Speed = 7.0f, Damage = 8.0f } },
    //         // enemy
    //         { 20201, new BulletData() { Id = 20201, Type = BulletData.EPositionType.World, LifeTime = 1.0f, Speed = 25.0f, Damage = 5.0f } },
    //     };

    //     private Dictionary<int, System.Type> _bulletTypeMap = new Dictionary<int, System.Type>()
    //     {
    //         { 10201, typeof(Waterspell) },
    //         { 10202, typeof(Waterspell) },
    //         { 10203, typeof(Waterspell) },
    //         { 10204, typeof(MagneticField) },
    //         { 10205, typeof(Waterspell) },
    //         // 다른 탄환 타입들 추가
    //         { 20201, typeof(Projectile7fire) },

    //     };


    //     // 딕셔너리 접근자 프로퍼티
    //     public IReadOnlyDictionary<int, AbilityData> AbilityDataDict => _abilityDataDict;

    //     public IReadOnlyDictionary<int, WeaponData> WeaponDataDict => _weaponDataDict;

    //     public IReadOnlyDictionary<int, BulletData> BulletDataDict => _bulletDataDict;

    //     public IReadOnlyDictionary<int, System.Type> BulletTypeMap => _bulletTypeMap;

    // }



}