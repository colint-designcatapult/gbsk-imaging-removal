using Grpc.Core;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Domain.DataManagement.System.Physics;
using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using Heracles.Application.Models.CollimatorConfiguration;
using Heracles.Application.Models.Settings;
using Heracles.Core.Models;
using Heracles.Core.Models.RDBMS;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xcc.Application.Commands;
using Xcc.Application.Domain.QualityAssurance;
using Xcc.Application.Domain.System;
using Xcc.Application.Models;
using Xcc.Application.Models.RDBMS;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Core.Domain.DataManagement.System.QualityAssurance;
using Xcc.Core.Exceptions;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Core.Models;
using Xcc.Core.Models.RDBMS;
using Xcc.Infra.Persistence.DataAccess.Dummy;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.Dummy
{
    public class SystemDummyHeadCommands : DummyRootEntryCommands<IHead, Head>, IHeadCommands
    {
    }

    public class SystemDummyCollimatorCommands
        : DummyChildEntryCommands<ICollimator, Collimator>
        , ICollimatorCommands
    {
        public SystemDummyCollimatorCommands() : base(c => c.CollimatorConfigurationId)
        {
        }
    }

    public class SystemDummyCollimatorConfigurationCommands 
        : DummyRootEntryCommands<ICollimatorConfiguration, CollimatorConfiguration>
        , ICollimatorConfigurationCommands
    {
    }

    public class SystemDummyPresetConfigurationCommands
        : DummyChildEntryCommands<IPresetConfiguration, PresetConfiguration>
        , IPresetConfigurationCommands
    {
        private readonly IUserCommands _userCommands;
        private readonly IAuthCommands _authCommands;

        public SystemDummyPresetConfigurationCommands(
            IUserCommands userCommands,
            IAuthCommands authCommands) : base(p => p.CollimatorConfigurationId)
        {
            this._userCommands = userCommands;
            this._authCommands = authCommands;
        }
        public async Task<IPresetConfiguration> ApproveAsync(long entryId, string username, string password)
        {
            var token = await _authCommands.AuthenticateUserAsync(username, password);
            if (token != null)
            {
                var user = (await _userCommands.ReadAllAsync()).FirstOrDefault(u => u.Username == username);
                if (user != null)
                {
                    var sampleToUpdate = await ReadAsync(entryId);
                    sampleToUpdate.ApprovedBy = user.EmailAddress;
                    return await UpdateAsync(null!, sampleToUpdate);
                }
            }
            throw new DataServiceException($"Cannot update qcSample: authentication failed for username={username}");
        }
    }

    /// <summary>
    /// Abstract class for dummy commands child to a preset type with approval procedure
    /// </summary>
    /// <typeparam name="TClientType"></typeparam>
    /// <typeparam name="TStorageType"></typeparam>
    public class DummyPresetChildEntryCommands<TClientType, TStorageType> : DummyChildEntryCommands<TClientType, TStorageType>
        where TClientType : class, IEntry
        where TStorageType : TClientType, new()
    {
        private readonly IPresetConfigurationCommands _presetConfigurationCommands;
        protected DummyPresetChildEntryCommands(
            IPresetConfigurationCommands presetConfigurationCommands, 
            Func<TClientType, long> parentIdFunc) 
            : base(parentIdFunc)
        {
            _presetConfigurationCommands = presetConfigurationCommands;
        }

        public override async Task<TClientType> CreateAsync(TClientType entry)
        {
            var createdEntry = await base.CreateAsync(entry);
            await InvalidatePresetApproval(GetParentId(createdEntry));
            return createdEntry;
        }

        protected async Task InvalidatePresetApproval(long presetId)
        {
            var preset = await _presetConfigurationCommands.ReadAsync(presetId);
            if (preset != null)
            {
                preset.ApprovedBy = string.Empty;
                await _presetConfigurationCommands.UpdateAsync(null!, preset);
            }
        }

        public override async Task<TClientType> UpdateAsync(TClientType oldEntry, TClientType newEntry)
        {
            var updatedEntry = await base.UpdateAsync(oldEntry, newEntry);
            await InvalidatePresetApproval(GetParentId(updatedEntry));
            return updatedEntry;
        }
        public override async Task<bool> DeleteAsync(long entryId)
        {
            var entry = await base.ReadAsync(entryId);
            var result = await base.DeleteAsync(entryId);
            if (result)
            {
                await InvalidatePresetApproval(GetParentId(entry));
            }
            return result;
        }
    }


    public class SystemDummyCoilConfigurationCommands 
        : DummyPresetChildEntryCommands<ICoilConfigurationEntry, CoilConfigurationEntry>
        , ICoilConfigurationCommands
    {
        public SystemDummyCoilConfigurationCommands(
            IPresetConfigurationCommands presetConfigurationCommands) 
            : base(presetConfigurationCommands, p => p.PresetConfigurationId)
        {
        }
    }

    public class SystemDummyCorrectionMatrixCommands
        : DummyPresetChildEntryCommands<ICorrectionMatrixEntry, CorrectionMatrixEntry>
        , ICorrectionMatrixCommands
    {
        public SystemDummyCorrectionMatrixCommands(
            IPresetConfigurationCommands presetConfigurationCommands)
            : base(presetConfigurationCommands, p => p.PresetConfigurationId)
        {
        }
    }

    public class SystemDummyHeaterCurrentConfigCommands
        : DummyPresetChildEntryCommands<IHeaterCurrentConfig, HeaterCurrentConfig>
        , IHeaterCurrentConfigCommands
    {
        public SystemDummyHeaterCurrentConfigCommands(
            IPresetConfigurationCommands presetConfigurationCommands)
            : base(presetConfigurationCommands, p => p.PresetConfigurationId)
        {
        }
    }

    public class SystemDummyReferenceFieldCommands
        : DummyPresetChildEntryCommands<IReferenceFieldEntry, ReferenceFieldEntry>
        , IReferenceFieldCommands
    {
        public SystemDummyReferenceFieldCommands(
            IPresetConfigurationCommands presetConfigurationCommands)
            : base(presetConfigurationCommands, p => p.PresetConfigurationId)
        {
        }
    }

    public class SystemDummyOutputFactorCommands 
        : DummyPresetChildEntryCommands<IOutputFactor, OutputFactor>
        , IOutputFactorCommands
    {
        public SystemDummyOutputFactorCommands(
            IPresetConfigurationCommands presetConfigurationCommands)
            : base(presetConfigurationCommands, p => p.PresetConfigurationId)
        {
        }
    }

    public class SystemDummyWarmupCommands : DummyChildEntryCommands<IWarmUp, WarmUp>, IWarmupCommands
    {
        public SystemDummyWarmupCommands()
            : base(warmup => warmup.HeadId)
        {
        }
    }

    public class SystemDummySafetyCheckCommands : DummyRootEntryCommands<ISafetyCheck, Models.QualityCheck.SafetyCheck>, ISafetyCheckCommands
    {
    }

    public class SystemDummyQcSampleCommands(
        IUserCommands userCommands,
        IAuthCommands authCommands) 
        : DummyChildEntryCommands<IQcSampleHeader, QcSampleHeader>(qcs => qcs.CollimatorConfigurationId)
        , IQcSampleCommands
    {
        public async Task<IQcSampleHeader> ApproveAsync(long entryId, string username, string password)
        {
            var token = await authCommands.AuthenticateUserAsync(username, password);
            if (token != null)
            {
                var user = (await userCommands.ReadAllAsync()).FirstOrDefault(u => u.Username == username);
                if (user != null)
                {
                    var sampleToUpdate = await ReadAsync(entryId);
                    sampleToUpdate.ApprovedBy = user.EmailAddress;
                    return await UpdateAsync(null!, sampleToUpdate);
                }
            }
            throw new DataServiceException($"Cannot update qcSample: authentication failed for username={username}");
        }
    }

    public class SystemDummyIntensityCommands : DummyChildEntryCommands<IIntensity, Intensity>, IIntensityCommands
    {
        public SystemDummyIntensityCommands()
            : base(x => x.QcSampleFieldId)
        {
        }
    }

    public class SystemDummyQcSampleFieldCommands : DummyChildEntryCommands<IQcSampleField, QcSampleField>, IQcSampleFieldCommands
    {
        public SystemDummyQcSampleFieldCommands()
            : base(qcsf => qcsf.QcSampleId)
        {
        }
    }

    public class SystemDummyAuthCommands : IAuthCommands
    {
        private IUserCommands UserCommands { get; }

        public Task<string> AuthenticateUserAsync(string username, string password)
        {
            var users = UserCommands.ReadAllAsync().GetAwaiter().GetResult();
            var matchingUser = users.FirstOrDefault(u => u.Username.Equals(username));
            if (matchingUser is null)
            {
                throw new RpcException(new Status(statusCode: StatusCode.Internal, "Dummy Authentication Error"));
            }
            else if (matchingUser.Password.Equals(password))
            {
                return Task.FromResult("DummyToken");
            }
            else
            {
                throw new Exception("Invalid username or password");
            }
        }
        public SystemDummyAuthCommands(IUserCommands userCommands)
        {
            UserCommands = userCommands;
        }
    }

    public class SystemDummySettingsCommands(IHeraclesCoreSettings heraclesCoreSettings, IDebugSettings debugSettings) : ISettingsCommands
    {
        public Task<ISystemSettings> GetSettingsAsync()
        {
            // Fill settings fields from appSettings by default:
            var endpoints = new EndPointsConfiguration
            {
                RecordAndVerifyEndPoint = new SystemEndPoint(heraclesCoreSettings.DataCommandsEndPoint), // Moses
                DatabaseEndpoint = new SystemEndPoint("127.0.0.1:5433"),
                TreatmentHeadCamEndPoint = new SystemEndPoint("127.0.0.1:50003"),
                GCBTelemetryEndPoint = new SystemEndPoint(heraclesCoreSettings.GCBTelemetryEndPoint?.Address() ?? "127.0.0.1:50020"),
                GCBCommandsEndPoint = new SystemEndPoint(heraclesCoreSettings.GCBCommandsEndPoint?.Address() ?? "127.0.0.1:50007")
            };

            var settings = new SystemSettings { DeviceSerial = debugSettings.DummyDeviceSerial, EndPointsConfiguration = endpoints };

            return Task.FromResult(settings as ISystemSettings);
        }

        public Task<ISystemSettings> UpdateSettingsAsync(ISystemSettings oldValue, ISystemSettings newValue)
        {
            return Task.FromResult(new SystemSettings(newValue) as ISystemSettings);
        }
    }
}
