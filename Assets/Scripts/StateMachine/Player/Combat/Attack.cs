using UnityEngine;

[CreateAssetMenu(fileName = "Attack", menuName = "Scriptable Objects/Attack")]
public class Attack : ScriptableObject
{
    [field: SerializeField]
    public string AnimationName { get; private set; }
    [field: SerializeField]
    public float TransitionDuration { get; private set; }
    [field: SerializeField]
    public int ComboStateIndex { get; private set; } = -1;
    [field: SerializeField]
    public float ComboAttackTime { get; private set; }

}
