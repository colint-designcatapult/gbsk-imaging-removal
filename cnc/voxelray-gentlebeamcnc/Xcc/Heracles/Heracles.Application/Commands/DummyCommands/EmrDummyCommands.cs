using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Empyrean.Common.Core.Domain.DataManagement.Common;
using Heracles.Application.Infra.DataManagement.EMR.DataAccess;
using Heracles.Application.Models;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Core.Commands;
using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Core.Enums;
using Xcc.Core.Exceptions;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Core.Logging;
using Xcc.Infra.Persistence.DataAccess.Dummy;

namespace Heracles.Application.Commands.DummyCommands
{
    public class EmrDummyPatientCommands : DummyRootEntryCommands<IPatient, Patient>, IEmrPatientCommands
    {
    }
    
    public class EmrDummyDiagnosisCommands : DummyChildEntryCommands<IDiagnosis, Diagnosis>, IEmrDiagnosisCommands
    {
        public EmrDummyDiagnosisCommands()
            : base(p => p.PatientId)
        {
        }
    }
    public class EmrDummySimulationCommands : DummyChildEntryCommands<ISimulation, Simulation>, IEmrSimulationCommands
    {
        public EmrDummySimulationCommands()
            : base(p => p.DiagnosisId)
        {
        }
    }

    public class EmrDummyPrescriptionCommands : DummyChildEntryCommands<IPrescription, Prescription>, IEmrPrescriptionCommands
    {
        public EmrDummyPrescriptionCommands()
            : base(p => p.SimulationId)
        {
        }
    }

    public class EmrDummyVisitCommands : DummyChildEntryCommands<IVisit, Visit>, IEmrVisitCommands
    {
        public EmrDummyVisitCommands()
            : base(p => p.PatientId)
        {
        }
    }

    public class EmrDummyTreatmentDeviceCommands : DummyChildEntryCommands<ITreatmentDevice, TreatmentDevice>, IEmrTreatmentDeviceCommands
    {
        public EmrDummyTreatmentDeviceCommands()
            : base(p => p.SimulationId)
        {
        }
    }


    public class EmrDummyPatientPositionCommands : DummyChildEntryCommands<IPatientPosition, PatientPositionEntry>, IEmrPatientPositionCommands
    {
        public EmrDummyPatientPositionCommands()
            : base(p => p.SimulationId)
        {
        }
    }

    public class EmrDummyTreatmentFieldCommands : DummyChildEntryCommands<ITreatmentField, TreatmentField>, IEmrTreatmentFieldCommands
    {
        public EmrDummyTreatmentFieldCommands()
            : base(p => p.PlanId)
        {
        }

        public async Task<ICollection<ITreatmentField>> CreateBunchAsync(ICollection<ITreatmentField> fields)
        {
            var result = new List<ITreatmentField>(fields.Count);

            foreach (var field in fields)
            {
                var created = await CreateAsync(field);
                result.Add(created);
            }

            return result;
        }
    }
    
    public class EmrDummyActualTreatmentFieldCommands : DummyChildEntryCommands<IActualTreatmentField, ActualTreatmentField>, IEmrActualTreatmentFieldCommands
    {
        public EmrDummyActualTreatmentFieldCommands()
            : base(p => p.TreatmentId)
        {
        }
    }

    public class EmrDummyEmissionTreatmentFieldCommands : DummyChildEntryCommands<IEmissionTreatmentField, EmissionTreatmentField>, IEmrEmissionTreatmentFieldCommands
    {
        public EmrDummyEmissionTreatmentFieldCommands()
            : base(p => p.ActualTreatmentFieldId)
        {
        }
    }
    
    public class EmrDummyTreatmentCommands : DummyChildEntryCommands<ITreatment, Treatment>, IEmrTreatmentCommands
    {
        public EmrDummyTreatmentCommands()
            : base(p => p.PlanId)
        {
        }
    }

    public class DummyUserCommands : DummyRootEntryCommands<IUser, User>, IUserCommands
    {
        public DummyUserCommands(
            IUserRoleMappingCommands userRoleMappingCommands,
            IRoleCommands roleCommands,
            IPermissionCommands permissionCommands,
            ILogWriter logWriter
            )
        {
            _userRoleMappingCommands = userRoleMappingCommands;
            _roleCommands = roleCommands;
            _permissionCommands = permissionCommands;
            _logWriter = logWriter;

            var roleRecords = AddRolesAndPermissions().GetAwaiter().GetResult();

            CreateUserWithRoleMappingAsync(new User()
            {
                FirstName = "Admin",
                Username = "Admin",
                LastName = "Admin",
                Password = "password",
                EmailAddress = "admin@admin.com"
            },
            roleRecords.First(x => x.Name == "Administrator")
            ).GetAwaiter().GetResult();


            CreateUserWithRoleMappingAsync(new User
            {
                CreationDate = DateTime.Now,
                EmailAddress = "serviceman@example.com",
                FirstName = "Jack",
                LastName = "Jackson",
                MiddleName = "J.",
                Password = "password",
                LastAccessed = DateTime.Now,
                Picture = "pic",
                Username = "Jack",
            },
            roleRecords.First(x => x.Name == "Service")).GetAwaiter().GetResult();

            CreateUserWithRoleMappingAsync(new User
            {
                CreationDate = DateTime.Now,
                EmailAddress = "rttst@example.com",
                FirstName = "Jane",
                LastName = "Jamesdottir",
                MiddleName = "J.",
                Password = "password",
                LastAccessed = DateTime.Now,
                Picture = "pic",
                Username = "Jane",
            },
            roleRecords.First(x => x.Name == "RTT")).GetAwaiter().GetResult();
        }

        private async Task CreateUserWithRoleMappingAsync(User user, RoleRecord role)
        {
            var storedUser = await CreateAsync(user);

            await _userRoleMappingCommands.CreateAsync(new UserRoleRecord { RoleId = role.Id, UserId = storedUser.Id, UserEmail = storedUser.EmailAddress });
        }

        private readonly IUserRoleMappingCommands _userRoleMappingCommands;
        private readonly IRoleCommands _roleCommands;
        private readonly IPermissionCommands _permissionCommands;
        private readonly ILogWriter _logWriter;

        private async Task<ICollection<RoleRecord>> AddRolesAndPermissions()
        {
            var roleRecords = new List<RoleRecord>();
            try
            {
                ICollection<UserRole> predefinedRoles = [
                    new UserRole("RTT") { Permissions = {ClinicalData = true, Treatment = true, QualityAssurance = true} },
                    new UserRole("Physicist"){ Permissions = {ClinicalData = true, SystemCalibration = true, QualityAssurance = true} },
                    new UserRole("Service"){ Permissions = {QualityAssurance = true, Services = true} },
                    new UserRole("Administrator") { Permissions = { ClinicalData = true, Treatment = true, SystemCalibration = true, QualityAssurance = true, SystemSettings = true, UserManagement = true, Services = true} }
                ];

                foreach (UserRole role in predefinedRoles)
                {
                    var storedRole = await _roleCommands.CreateAsync(new RoleRecord { Name = role.Name, Description = role.Name });
                    if (role.Permissions.ClinicalData)
                        await _permissionCommands.CreateAsync(new PermissionRecord { RoleId = storedRole.Id, Type = PermissionType.ClinicalData });
                    if (role.Permissions.Treatment)
                        await _permissionCommands.CreateAsync(new PermissionRecord { RoleId = storedRole.Id, Type = PermissionType.Treatment });
                    if (role.Permissions.SystemCalibration)
                        await _permissionCommands.CreateAsync(new PermissionRecord { RoleId = storedRole.Id, Type = PermissionType.SystemCalibration });
                    if (role.Permissions.QualityAssurance)
                        await _permissionCommands.CreateAsync(new PermissionRecord { RoleId = storedRole.Id, Type = PermissionType.QualityAssurance });
                    if (role.Permissions.SystemSettings)
                        await _permissionCommands.CreateAsync(new PermissionRecord { RoleId = storedRole.Id, Type = PermissionType.SystemSettings });
                    if (role.Permissions.UserManagement)
                        await _permissionCommands.CreateAsync(new PermissionRecord { RoleId = storedRole.Id, Type = PermissionType.UserManagement });
                    if (role.Permissions.Services)
                        await _permissionCommands.CreateAsync(new PermissionRecord { RoleId = storedRole.Id, Type = PermissionType.Services });
                    roleRecords.Add(storedRole);
                }
            }
            catch (Exception ex)
            {
                _ = _logWriter.LogAsync($"DummySystemData error: failed to write roles/permissions to the DB. {ex.Message}",
                    LogRecordSeverity.Error,
                    LogRecordType.Error);
            }
            return roleRecords;
        }
    }

    public class DummyRoleCommands : DummyRootEntryCommands<RoleRecord, RoleRecord>, IRoleCommands
    {
    }

    public class DummyPermissionCommands : DummyChildEntryCommands<PermissionRecord, PermissionRecord>, IPermissionCommands
    {
        public DummyPermissionCommands()
            : base(p => p.RoleId)
        {
        }
    }

    public class DummyUserRoleMappingCommands() 
        : DummyChildEntryCommands<UserRoleRecord, UserRoleRecord>(m => m.UserId)
        , IUserRoleMappingCommandsExt
    {
        public Task<ICollection<UserRoleRecord>> ReadListAsync(string userEmail)
        {
            var list = new List<UserRoleRecord>();
            foreach(var values in Entries.Values)
            {
                list.AddRange(values.Where(m => m.UserEmail == userEmail));
            }
            return Task.FromResult((ICollection<UserRoleRecord>)list);
        }
    }


    public class EmrDummyPhotoCommands : DummyChildEntryCommands<IPhotoDescription, PhotoDescription>, IEmrPhotoCommands
    {
        public EmrDummyPhotoCommands()
            : base(p => p.DiagnosisId)
        {
        }

        public Task SendPhotoAsync(IPhoto photo, int chunkSize, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public async Task<IPhoto?> ReceivePhotoAsync(IPhotoDescription photoDescription, CancellationToken token)
        {
            return await _photoStreamReader.ReceivePhotoAsync(photoDescription, token);
        }

        private readonly IPhotoStreamReader _photoStreamReader = new DummyPhotoStreamReader();
    }

    public class EmrDummyPlanCommands 
        : DummyChildEntryCommands<IPlan, Plan>
        , IEmrPlanCommands
        , ILoadForTreatmentEventStream
        , IPlanEventStream
    {
        public EmrDummyPlanCommands()
            : base(p => p.PrescriptionId)
        {
        }

        public Task LoadForTreatmentAsync(long planId, bool isPartial)
        {
            if (BaseEntry.IsBlankId(_loadForTreatmentPlanId))
            {
                foreach (var plans in Entries.Values)
                {
                    foreach (var plan in plans)
                    {
                        if (plan.Id == planId)
                        {
                            var pendingState = isPartial ? TreatmentLoadingState.PartialPendingLoad : TreatmentLoadingState.PendingLoad;
                            var pendingPlan = new Plan(plan) { TreatmentLoadingState = pendingState };
                            plans[plans.IndexOf(plan)] = pendingPlan;
                            loadForTreatmentPlanEvents.Enqueue(pendingPlan);

                            return Task.FromResult(_loadForTreatmentPlanId = planId);
                        }
                    }
                }
                throw new DataServiceException($"Cannot load plan for treatment: no such plan in the DB");
            }
            else
            {
                throw new DataServiceException($"Cannot load plan for treatment: there's another pending plan (id={_loadForTreatmentPlanId}");
            }
        }

        public Task TreatmentLoadAcknowledgeAsync(long planId)
        {
            foreach (var plans in Entries.Values)
            {
                IPlan? plan = plans.FirstOrDefault(p => p.Id == planId);
                if (plan is not null)
                {
                    var loadedPlan = new Plan(plan) { TreatmentLoadingState = TreatmentLoadingState.Loaded };
                    plans[plans.IndexOf(plan)] = loadedPlan;
                    planEvents.Enqueue(loadedPlan);
                    break;
                }
            }
            return Task.CompletedTask;
        }

        public Task UnloadFromTreatmentAsync(long planId)
        {
            if (!BaseEntry.IsBlankId(_loadForTreatmentPlanId))
            {
                if (planId == _loadForTreatmentPlanId)
                {
                    foreach (var plans in Entries.Values)
                    {
                        foreach (var plan in plans)
                        {
                            if (plan.Id == planId)
                            {
                                var unloadedPlan = new Plan(plan) { TreatmentLoadingState = TreatmentLoadingState.Unloaded };
                                plans[plans.IndexOf(plan)] = unloadedPlan;
                                planEvents.Enqueue(unloadedPlan);

                                return Task.FromResult(_loadForTreatmentPlanId = BaseEntry.NewEntryId);
                            }
                        }
                    }
                    throw new DataServiceException($"Cannot unload plan from treatment: no such plan in the DB");
                }
                else
                {
                    throw new DataServiceException($"Cannot unload plan from treatment: wrong plan Id");
                }
            }
            else
            {
                throw new DataServiceException($"Cannot unload plan for treatment: there's no pending plans");
            }

        }

        public Task<IPlan?> FindPendingPlanAsync()
        {
            return Task.Run(IPlan? () =>
            {
                foreach (var plans in Entries.Values)
                {
                    foreach (var plan in plans)
                    {
                        if (plan.TreatmentLoadingState is TreatmentLoadingState.PendingLoad
                                or TreatmentLoadingState.PartialPendingLoad)
                            return plan;
                    }
                }

                return null;
            });
        }

        public Task<IPlan?> FindLoadedPlanAsync()
        {
            return Task.Run(IPlan? () =>
            {
                foreach (var plans in Entries.Values)
                {
                    foreach (var plan in plans)
                    {
                        if (plan.TreatmentLoadingState == TreatmentLoadingState.Loaded)
                            return plan;
                    }
                }

                return null;
            });
        }

        public Task<IPlan?> UpdateStatusAsync(string email, string password, long planId, PlanStatus status)
        {
            IPlan? p = null;

            foreach (var plans in Entries.Values)
            {
                foreach (var plan in plans)
                {
                    if (plan.Id == planId)
                    {
                        plans[plans.IndexOf(plan)] = new Plan(plan) { Status = status };
                        //return Task.FromResult(plans[plans.IndexOf(plan)] = new Plan(plan) { Status = status });
                        // TODO: moses temporarily doesn't return any plan:
                        return Task.FromResult(p);
                    }
                }
            }

            return Task.FromResult(p);
        }

        public Task RunStreamAsync(Action<LoadForTreatmentEventsStreamArgs> planReceivedCallback, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationToken);
                    if (loadForTreatmentPlanEvents.TryDequeue(out IPlan? plan))
                    {
                        planReceivedCallback(new LoadForTreatmentEventsStreamArgs(plan, patient: null));
                    }
                }
            }, cancellationToken);
        }

        public Task RunStreamAsync(Action<IPlan> streamCallback, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationToken);
                    IPlan? plan = null;
                    if (planEvents.TryDequeue(out plan))
                    {
                        streamCallback(plan);
                    }
                }
            }, cancellationToken);
        }

        private long _loadForTreatmentPlanId = BaseEntry.NewEntryId;
        private Queue<IPlan> loadForTreatmentPlanEvents = new();
        private Queue<IPlan> planEvents = new();
    }

    public class DummySystemCommands : ISystemCommands
    {
        public Task<MosesSystemInfo> GetSystemInfoAsync()
        {
            return Task.FromResult(new MosesSystemInfo("Dummy 1.2.3"));
        }
    }

}
