public interface ITrigger
{
    bool isInSensor { get; set; }
    bool isWithinAttackRange { get; set; }

    void SetInSensor(bool isTriggered);
    void SetWithinAttackRange(bool isWithinAttackRange);
}
