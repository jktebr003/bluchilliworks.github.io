using Shared.Models;
using Shared.Enums;

namespace WebServer.Features.Pricing;

public record LoadPackagesAction;
public record LoadPackagesSuccessAction(List<PackageResponse> Packages);
public record LoadPackagesFailedAction(string ErrorMessage);
public record FilterPackagesByTypeAction(PackageType PackageType);
public record SelectPackageAction(PackageResponse Package);
