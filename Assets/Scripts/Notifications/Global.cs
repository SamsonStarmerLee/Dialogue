using UnityEngine;

namespace Notifications
{
    /// NOTE: 
    /// These are used to append an event type name to the end of the classname for an observer.
    /// Example:
    ///     this.AddObserver(OnValidateAttackAction, Global.ValidateNotification<AttackAction>());
    ///     this.AddObserver(OnPerformAttackAction, Global.PerformNotification<AttackAction>());

    //public static class Global
    //{
    //    public static int GenerateID<T>() => GenerateID(typeof(T));

    //    public static int GenerateID(System.Type type) => Animator.StringToHash(type.Name);

    //    public static string PrepareNotification<T>() => PrepareNotification(typeof(T));

    //    public static string PrepareNotification(System.Type type) => $"{type.Name}.PrepareNotification";

    //    public static string PerformNotification<T>() => PerformNotification(typeof(T));

    //    public static string PerformNotification(System.Type type) => $"{type.Name}.PerformNotification";

    //    public static string ValidateNotification<T>() => ValidateNotification(typeof(T));

    //    public static string ValidateNotification(System.Type type) => $"{type.Name}.ValidateNotification";

    //    public static string CancelNotification<T>() => CancelNotification(typeof(T));

    //    public static string CancelNotification(System.Type type) => $"{type.Name}.CancelNotification";
    //}
    }
