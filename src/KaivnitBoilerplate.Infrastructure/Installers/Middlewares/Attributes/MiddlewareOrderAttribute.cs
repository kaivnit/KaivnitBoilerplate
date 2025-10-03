namespace KaivnitBoilerplate.Infrastructure.Installers;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class MiddlewareOrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}
