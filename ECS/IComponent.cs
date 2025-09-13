using Unity.Entities;

public interface QG_IComponent : IComponentData
{
}

public interface QG_IEnableComponent : IComponentData, IEnableableComponent
{
}