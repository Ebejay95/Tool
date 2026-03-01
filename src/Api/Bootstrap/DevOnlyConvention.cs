using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Api.Bootstrap;

/// <summary>
/// Markiert einen Controller als Development-only.
/// In allen anderen Environments werden seine Routen nicht registriert —
/// der Endpunkt existiert in Production physisch nicht.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class DevOnlyAttribute : Attribute { }

/// <summary>
/// MVC-Convention: entfernt alle Action-Selektoren von <see cref="DevOnlyAttribute"/>-
/// Controllern außerhalb der Development-Umgebung → keine Route, kein 404, kein Swagger-Eintrag.
/// </summary>
public sealed class DevOnlyConvention(IWebHostEnvironment env) : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        if (env.IsDevelopment()) return;
        if (!controller.Attributes.OfType<DevOnlyAttribute>().Any()) return;

        foreach (var action in controller.Actions)
            action.Selectors.Clear();
    }
}
