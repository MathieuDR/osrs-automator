namespace DiscordBot.Dashboard.Binders.RouteConstraints;

public class UlongRouteConstraint : IRouteConstraint {
	public static string UlongRouteConstraintName = "ulong";

	public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection) {
		if (values.TryGetValue("id", out var partValue)) {
			if (ulong.TryParse(partValue?.ToString(), out _)) {
				return true;
			}
		}

		return false;
	}
}
