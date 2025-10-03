namespace KaivnitBoilerplate.Infrastructure.Installers;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ServiceOrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}
