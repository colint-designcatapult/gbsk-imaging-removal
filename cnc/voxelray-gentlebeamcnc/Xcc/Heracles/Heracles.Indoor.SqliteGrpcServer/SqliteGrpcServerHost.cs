using Com.Empyreanmed.Heracles.ActualTreatmentFields.V1;
using Com.Empyreanmed.Heracles.CoilConfigurations.V1;
using Com.Empyreanmed.Heracles.CollimatorConfigurations.V1;
using Com.Empyreanmed.Heracles.Collimators.V1;
using Com.Empyreanmed.Heracles.CorrectionMatrix.V1;
using Com.Empyreanmed.Heracles.Diagnoses.V1;
using Com.Empyreanmed.Heracles.EmissionTreatmentFields.V1;
using Com.Empyreanmed.Heracles.Head.V1;
using Com.Empyreanmed.Heracles.HeaterCurrentConfigs.V1;
using Com.Empyreanmed.Heracles.Intensities.V1;
using Com.Empyreanmed.Heracles.Logs.V1;
using Com.Empyreanmed.Heracles.OutputFactors.V1;
using Com.Empyreanmed.Heracles.Patients.V1;
using Com.Empyreanmed.Heracles.Photos.V1;
using Com.Empyreanmed.Heracles.Plans.V1;
using Com.Empyreanmed.Heracles.Positions.V1;
using Com.Empyreanmed.Heracles.PresetConfigurations.V1;
using Com.Empyreanmed.Heracles.Prescriptions.V1;
using Com.Empyreanmed.Heracles.Qcsamples.V1;
using Com.Empyreanmed.Heracles.QcsampleFields.V1;
using Com.Empyreanmed.Heracles.ReferenceFields.V1;
using Com.Empyreanmed.Heracles.Roles.V1;
using Com.Empyreanmed.Heracles.RolesPermissions.V1;
using Com.Empyreanmed.Heracles.SafetyChecks.V1;
using Com.Empyreanmed.Heracles.Settings.V1;
using Com.Empyreanmed.Heracles.Simulations.V1;
using Com.Empyreanmed.Heracles.TreatmentDevices.V1;
using Com.Empyreanmed.Heracles.TreatmentFields.V1;
using Com.Empyreanmed.Heracles.Treatments.V1;
using Com.Empyreanmed.Heracles.UserRoles.V1;
using Com.Empyreanmed.Heracles.Users.V1;
using Com.Empyreanmed.Heracles.Visits.V1;
using Com.Empyreanmed.Heracles.Warmups.V1;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;
using Heracles.Indoor.SqliteGrpcServer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Heracles.Indoor.SqliteGrpcServer;

/// <summary>
/// Hosts an in-process ASP.NET Core gRPC server backed by SQLite.
/// Call <see cref="StartAsync"/> once at application startup and
/// <see cref="StopAsync"/> on shutdown.
/// </summary>
public sealed class SqliteGrpcServerHost : IAsyncDisposable
{
    public const int DefaultPort = 5199;

    private readonly WebApplication _app;

    public SqliteGrpcServerHost(string dbPath, int port = DefaultPort)
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.ConfigureKestrel(k =>
        {
            k.ListenAnyIP(port, o => o.Protocols = HttpProtocols.Http2);
        });

        var services = builder.Services;

        // Repositories — one per entity type
        services.AddSingleton(_ => new SqliteProtoRepository<Patient>(dbPath, "patients"));
        services.AddSingleton(_ => new SqliteProtoRepository<Diagnosis>(dbPath, "diagnoses", hasParentId: true));
        services.AddSingleton(_ => new SqliteProtoRepository<Simulation>(dbPath, "simulations", hasParentId: true));
        services.AddSingleton(_ => new SqliteProtoRepository<Prescription>(dbPath, "prescriptions", hasParentId: true));
        services.AddSingleton(_ => new SqliteProtoRepository<Visit>(dbPath, "visits", hasParentId: true));
        services.AddSingleton(_ => new SqliteProtoRepository<Plan>(dbPath, "plans", hasParentId: true));
        services.AddSingleton(_ => new SqliteProtoRepository<TreatmentDevice>(dbPath, "treatment_devices", hasParentId: true));
        services.AddSingleton(_ => new SqliteProtoRepository<Position>(dbPath, "positions", hasParentId: true));
        services.AddSingleton(_ => new SqliteProtoRepository<TreatmentField>(dbPath, "treatment_fields", hasParentId: true));
        services.AddSingleton(_ => new SqliteProtoRepository<ActualTreatmentField>(dbPath, "actual_treatment_fields", hasParentId: true));
        services.AddSingleton(_ => new SqliteProtoRepository<EmissionTreatmentField>(dbPath, "emission_treatment_fields", hasParentId: true));
        services.AddSingleton(_ => new SqliteProtoRepository<Treatment>(dbPath, "treatments"));
        services.AddSingleton(_ => new SqliteProtoRepository<Photo>(dbPath, "photos", hasParentId: true));
        services.AddSingleton(_ => new SqliteProtoRepository<Com.Empyreanmed.Heracles.Users.V1.User>(dbPath, "users"));
        services.AddSingleton(_ => new SqliteProtoRepository<Role>(dbPath, "roles"));
        services.AddSingleton(_ => new SqliteProtoRepository<RolesPermissions>(dbPath, "roles_permissions"));
        services.AddSingleton(_ => new SqliteProtoRepository<UserRole>(dbPath, "user_roles"));
        services.AddSingleton(_ => new SqliteProtoRepository<Head>(dbPath, "heads"));
        services.AddSingleton(_ => new SqliteProtoRepository<Collimator>(dbPath, "collimators", hasParentId: true));
        services.AddSingleton(_ => new SqliteProtoRepository<CollimatorConfiguration>(dbPath, "collimator_configurations"));
        services.AddSingleton(_ => new SqliteProtoRepository<CoilConfiguration>(dbPath, "coil_configurations"));
        services.AddSingleton(_ => new SqliteProtoRepository<CorrectionMatrix>(dbPath, "correction_matrices"));
        services.AddSingleton(_ => new SqliteProtoRepository<HeaterCurrentConfig>(dbPath, "heater_current_configs"));
        services.AddSingleton(_ => new SqliteProtoRepository<OutputFactor>(dbPath, "output_factors"));
        services.AddSingleton(_ => new SqliteProtoRepository<ReferenceField>(dbPath, "reference_fields"));
        services.AddSingleton(_ => new SqliteProtoRepository<PresetConfiguration>(dbPath, "preset_configurations"));
        services.AddSingleton(_ => new SqliteProtoRepository<QCSample>(dbPath, "qcsamples"));
        services.AddSingleton(_ => new SqliteProtoRepository<QCSampleField>(dbPath, "qcsample_fields"));
        services.AddSingleton(_ => new SqliteProtoRepository<Intensity>(dbPath, "intensities"));
        services.AddSingleton(_ => new SqliteProtoRepository<SafetyCheck>(dbPath, "safety_checks"));
        services.AddSingleton(_ => new SqliteProtoRepository<Warmup>(dbPath, "warmups"));
        services.AddSingleton(_ => new SqliteProtoRepository<Log>(dbPath, "logs"));
        services.AddSingleton(_ => new SqliteProtoRepository<Settings>(dbPath, "settings"));

        // Service implementations
        services.AddGrpc();
        services.AddSingleton<AuthServiceImpl>();
        services.AddSingleton<PatientServiceImpl>();
        services.AddSingleton<DiagnosisServiceImpl>();
        services.AddSingleton<SimulationServiceImpl>();
        services.AddSingleton<PrescriptionServiceImpl>();
        services.AddSingleton<VisitServiceImpl>();
        services.AddSingleton<PlanServiceImpl>();
        services.AddSingleton<TreatmentDeviceServiceImpl>();
        services.AddSingleton<PositionServiceImpl>();
        services.AddSingleton<TreatmentFieldServiceImpl>();
        services.AddSingleton<ActualTreatmentFieldServiceImpl>();
        services.AddSingleton<EmissionTreatmentFieldServiceImpl>();
        services.AddSingleton<TreatmentServiceImpl>();
        services.AddSingleton<PhotosServiceImpl>();
        services.AddSingleton<UsersServiceImpl>();
        services.AddSingleton<RoleServiceImpl>();
        services.AddSingleton<RolesPermissionsServiceImpl>();
        services.AddSingleton<UserRoleServiceImpl>();
        services.AddSingleton<HeadServiceImpl>();
        services.AddSingleton<CollimatorServiceImpl>();
        services.AddSingleton<CollimatorConfigurationServiceImpl>();
        services.AddSingleton<CoilConfigurationServiceImpl>();
        services.AddSingleton<CorrectionMatrixServiceImpl>();
        services.AddSingleton<HeaterCurrentConfigServiceImpl>();
        services.AddSingleton<OutputFactorServiceImpl>();
        services.AddSingleton<ReferenceFieldServiceImpl>();
        services.AddSingleton<PresetConfigurationServiceImpl>();
        services.AddSingleton<QCSampleServiceImpl>();
        services.AddSingleton<QCSampleFieldServiceImpl>();
        services.AddSingleton<IntensityServiceImpl>();
        services.AddSingleton<SafetyCheckServiceImpl>();
        services.AddSingleton<WarmupServiceImpl>();
        services.AddSingleton<LogServiceImpl>();
        services.AddSingleton<SettingsServiceImpl>();
        services.AddSingleton<SystemServiceImpl>();

        _app = builder.Build();

        _app.MapGrpcService<AuthServiceImpl>();
        _app.MapGrpcService<PatientServiceImpl>();
        _app.MapGrpcService<DiagnosisServiceImpl>();
        _app.MapGrpcService<SimulationServiceImpl>();
        _app.MapGrpcService<PrescriptionServiceImpl>();
        _app.MapGrpcService<VisitServiceImpl>();
        _app.MapGrpcService<PlanServiceImpl>();
        _app.MapGrpcService<TreatmentDeviceServiceImpl>();
        _app.MapGrpcService<PositionServiceImpl>();
        _app.MapGrpcService<TreatmentFieldServiceImpl>();
        _app.MapGrpcService<ActualTreatmentFieldServiceImpl>();
        _app.MapGrpcService<EmissionTreatmentFieldServiceImpl>();
        _app.MapGrpcService<TreatmentServiceImpl>();
        _app.MapGrpcService<PhotosServiceImpl>();
        _app.MapGrpcService<UsersServiceImpl>();
        _app.MapGrpcService<RoleServiceImpl>();
        _app.MapGrpcService<RolesPermissionsServiceImpl>();
        _app.MapGrpcService<UserRoleServiceImpl>();
        _app.MapGrpcService<HeadServiceImpl>();
        _app.MapGrpcService<CollimatorServiceImpl>();
        _app.MapGrpcService<CollimatorConfigurationServiceImpl>();
        _app.MapGrpcService<CoilConfigurationServiceImpl>();
        _app.MapGrpcService<CorrectionMatrixServiceImpl>();
        _app.MapGrpcService<HeaterCurrentConfigServiceImpl>();
        _app.MapGrpcService<OutputFactorServiceImpl>();
        _app.MapGrpcService<ReferenceFieldServiceImpl>();
        _app.MapGrpcService<PresetConfigurationServiceImpl>();
        _app.MapGrpcService<QCSampleServiceImpl>();
        _app.MapGrpcService<QCSampleFieldServiceImpl>();
        _app.MapGrpcService<IntensityServiceImpl>();
        _app.MapGrpcService<SafetyCheckServiceImpl>();
        _app.MapGrpcService<WarmupServiceImpl>();
        _app.MapGrpcService<LogServiceImpl>();
        _app.MapGrpcService<SettingsServiceImpl>();
        _app.MapGrpcService<SystemServiceImpl>();
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
        => _app.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken = default)
        => _app.StopAsync(cancellationToken);

    public async ValueTask DisposeAsync()
        => await _app.DisposeAsync();
}
