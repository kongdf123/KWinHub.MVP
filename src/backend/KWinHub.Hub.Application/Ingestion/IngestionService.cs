using System.Security.Cryptography;
using System.Text;
using KWinHub.Hub.Application.Common;
using KWinHub.Hub.Application.Ingestion;
using KWinHub.Hub.Application.Protocols;
using KWinHub.Hub.Application.Results;
using KWinHub.Hub.Application.Sessions;
using KWinHub.Hub.Application.Subjects;
using KWinHub.Hub.Contracts.Ingestion;
using KWinHub.Hub.Protocols;
using KWinHub.Hub.Results;
using KWinHub.Hub.Sessions;

namespace KWinHub.Hub.Application.Ingestion;

public class IngestionService : IIngestionService
{
    private const string SupportedContractVersion = "1.0";

    private readonly ISubjectRepository _subjectRepository;
    private readonly IProtocolRepository _protocolRepository;
    private readonly IMetricDefinitionRepository _metricDefinitionRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly ITrialRepository _trialRepository;
    private readonly IResultRepository _resultRepository;
    private readonly IIngestionOperationRepository _operationRepository;
    private readonly ITransactionManager _transactionManager;

    public IngestionService(
        ISubjectRepository subjectRepository,
        IProtocolRepository protocolRepository,
        IMetricDefinitionRepository metricDefinitionRepository,
        ISessionRepository sessionRepository,
        ITrialRepository trialRepository,
        IResultRepository resultRepository,
        IIngestionOperationRepository operationRepository,
        ITransactionManager transactionManager)
    {
        _subjectRepository = subjectRepository;
        _protocolRepository = protocolRepository;
        _metricDefinitionRepository = metricDefinitionRepository;
        _sessionRepository = sessionRepository;
        _trialRepository = trialRepository;
        _resultRepository = resultRepository;
        _operationRepository = operationRepository;
        _transactionManager = transactionManager;
    }

    public async Task<IngestSessionResponse> IngestSessionAsync(
        Guid organizationId,
        IngestSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateContractVersion(request.Version);

        var contentHash = ComputeContentHash(request);

        var existingOperation = await _operationRepository.GetByClientOperationIdAsync(
            organizationId, request.ClientOperationId, cancellationToken);

        if (existingOperation is not null)
        {
            if (existingOperation.Status == IngestionOperationStatus.Pending)
            {
                return BuildPendingResponse(existingOperation);
            }

            if (existingOperation.Status == IngestionOperationStatus.Failed)
            {
                return ToResponse(existingOperation);
            }

            if (!string.Equals(existingOperation.ContentHash, contentHash, StringComparison.Ordinal))
            {
                throw new IngestionException(
                    IngestionErrorCodes.Conflict,
                    $"Operation '{request.ClientOperationId}' already accepted with different content. " +
                    "Use a new ClientOperationId for corrections or set IsCorrection=true with a matching ExternalId.",
                    "clientOperationId");
            }

            return ToResponse(existingOperation);
        }

        var operation = new IngestionOperation(organizationId, request.ClientOperationId, contentHash);

        await using var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
        try
        {
            await _operationRepository.AddAsync(operation, cancellationToken);

            var subject = await _subjectRepository.GetByIdAsync(organizationId, request.SubjectId, cancellationToken);
            if (subject is null)
            {
                throw new IngestionException(
                    IngestionErrorCodes.UnknownSubject,
                    $"Subject {request.SubjectId} not found in organization.",
                    "subjectId");
            }

            var protocol = await _protocolRepository.GetByIdAsync(organizationId, request.ProtocolId, cancellationToken);
            if (protocol is null)
            {
                throw new IngestionException(
                    IngestionErrorCodes.InvalidProtocol,
                    $"Protocol {request.ProtocolId} not found in organization.",
                    "protocolId");
            }

            if (!string.Equals(protocol.Version, request.ProtocolVersion, StringComparison.Ordinal))
            {
                throw new IngestionException(
                    IngestionErrorCodes.InvalidProtocolVersion,
                    $"Protocol version '{request.ProtocolVersion}' does not match protocol '{protocol.Code}' version '{protocol.Version}'.",
                    "protocolVersion");
            }

            var session = await GetOrCreateSessionAsync(organizationId, request, cancellationToken);

            var trialResults = new List<IngestTrialResult>();
            if (request.Trials is not null)
            {
                foreach (var trialRequest in request.Trials)
                {
                    var trialId = await IngestTrialAsync(
                        organizationId,
                        session.Id,
                        session.ProtocolId,
                        session.ProtocolVersion,
                        trialRequest,
                        cancellationToken);
                    trialResults.Add(new IngestTrialResult(trialRequest.TrialIndex, trialId, "Accepted"));
                }
            }

            var assetResults = new List<IngestAssetPlaceholderResult>();
            if (request.AssetPlaceholders is not null)
            {
                foreach (var asset in request.AssetPlaceholders)
                {
                    assetResults.Add(new IngestAssetPlaceholderResult(
                        asset.AssetType,
                        asset.ExternalId,
                        asset.Size,
                        "PendingUpload"));
                }
            }

            operation.MarkAccepted(session.Id, session.ExternalId);
            await _operationRepository.UpdateAsync(operation, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return new IngestSessionResponse(
                SessionId: session.Id,
                SessionExternalId: session.ExternalId,
                Status: "Accepted",
                Trials: trialResults,
                AssetPlaceholders: assetResults);
        }
        catch (IngestionException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new IngestionException(
                IngestionErrorCodes.InternalError,
                $"Ingestion failed: {ex.Message}");
        }
    }

    public async Task<IngestionOperationStatusResponse?> GetOperationStatusAsync(
        Guid organizationId,
        string clientOperationId,
        CancellationToken cancellationToken = default)
    {
        var operation = await _operationRepository.GetByClientOperationIdAsync(
            organizationId, clientOperationId, cancellationToken);
        return operation is null ? null : ToStatusResponse(operation);
    }

    public async Task<IngestionOperationStatusResponse?> GetSessionStatusAsync(
        Guid organizationId,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var operation = await _operationRepository.GetBySessionIdAsync(
            organizationId, sessionId, cancellationToken);
        return operation is null ? null : ToStatusResponse(operation);
    }

    private async Task<MeasurementSession> GetOrCreateSessionAsync(
        Guid organizationId,
        IngestSessionRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.ExternalId))
        {
            var existing = await _sessionRepository.GetByExternalIdAsync(
                organizationId, request.ExternalId, cancellationToken);
            if (existing is not null)
            {
                if (!request.IsCorrection)
                {
                    throw new IngestionException(
                        IngestionErrorCodes.DuplicateSession,
                        $"Session with external id '{request.ExternalId}' already exists. " +
                        "Set IsCorrection=true to update an existing session.",
                        "externalId");
                }

                return existing;
            }
        }

        var session = new MeasurementSession(
            organizationId: organizationId,
            subjectId: request.SubjectId,
            protocolId: request.ProtocolId,
            protocolVersion: request.ProtocolVersion,
            sourceId: request.SourceId,
            deviceId: request.DeviceId,
            clientApplication: request.ClientApplication,
            clientVersion: request.ClientVersion,
            externalId: request.ExternalId,
            timezone: request.Timezone);

        if (request.StartedAt.HasValue)
        {
            session.SetStartedAt(request.StartedAt.Value);
        }

        await _sessionRepository.AddAsync(session, cancellationToken);
        return session;
    }

    private async Task<Guid> IngestTrialAsync(
        Guid organizationId,
        Guid sessionId,
        Guid protocolId,
        string protocolVersion,
        IngestTrialRequest trialRequest,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(trialRequest.ExternalId))
        {
            var existing = await _trialRepository.GetByExternalIdAsync(
                organizationId, trialRequest.ExternalId, cancellationToken);
            if (existing is not null)
            {
                return existing.Id;
            }
        }

        var trial = new Trial(
            organizationId: organizationId,
            sessionId: sessionId,
            trialIndex: trialRequest.TrialIndex,
            externalId: trialRequest.ExternalId,
            notes: trialRequest.Notes);
        trial.Complete();
        await _trialRepository.AddAsync(trial, cancellationToken);

        var result = new Result(
            organizationId: organizationId,
            sessionId: sessionId,
            protocolId: protocolId,
            protocolVersion: protocolVersion,
            algorithmVersion: "ingested",
            trialId: trial.Id);
        result.MarkPublished();

        if (trialRequest.Metrics is not null)
        {
            foreach (var metricRequest in trialRequest.Metrics)
            {
                var metric = await BuildMetricAsync(
                    organizationId,
                    result.Id,
                    metricRequest,
                    cancellationToken);
                result.AddMetricDirect(metric);
            }
        }

        await _resultRepository.AddAsync(result, cancellationToken);
        return trial.Id;
    }

    private async Task<Metric> BuildMetricAsync(
        Guid organizationId,
        Guid resultId,
        IngestMetricRequest request,
        CancellationToken cancellationToken)
    {
        var definition = await _metricDefinitionRepository.GetByCodeAsync(
            organizationId, request.MetricDefinitionCode, cancellationToken);
        if (definition is null)
        {
            throw new IngestionException(
                IngestionErrorCodes.UnsupportedMetric,
                $"Metric definition '{request.MetricDefinitionCode}' not found.",
                $"metrics.{request.MetricDefinitionCode}");
        }

        if (!Enum.TryParse<MetricValueType>(request.ValueType, ignoreCase: true, out var valueType))
        {
            throw new IngestionException(
                IngestionErrorCodes.InvalidValueType,
                $"Invalid value type '{request.ValueType}'.",
                $"metrics.{request.MetricDefinitionCode}.valueType");
        }

        if (definition.ValueType != valueType)
        {
            throw new IngestionException(
                IngestionErrorCodes.InvalidValueType,
                $"Value type '{request.ValueType}' does not match metric definition value type '{definition.ValueType}'.",
                $"metrics.{request.MetricDefinitionCode}.valueType");
        }

        if (!string.IsNullOrWhiteSpace(request.Unit) &&
            !string.Equals(request.Unit, definition.Unit, StringComparison.OrdinalIgnoreCase))
        {
            throw new IngestionException(
                IngestionErrorCodes.UnitMismatch,
                $"Unit '{request.Unit}' does not match definition unit '{definition.Unit}'.",
                $"metrics.{request.MetricDefinitionCode}.unit");
        }

        var unit = string.IsNullOrWhiteSpace(request.Unit) ? definition.Unit : request.Unit;

        return valueType switch
        {
            MetricValueType.Scalar => new Metric(
                resultId, definition.Code, definition.Name, request.ScalarValue ?? 0, unit, request.Side, definition.Id),
            MetricValueType.Bilateral => Metric.CreateBilateral(
                resultId, definition.Code, definition.Name,
                request.LeftValue ?? 0, request.RightValue ?? 0, unit, definition.Id),
            MetricValueType.Categorical => Metric.CreateCategorical(
                resultId, definition.Code, definition.Name,
                request.CategoricalValue ?? string.Empty, unit, definition.Id),
            MetricValueType.Structured => Metric.CreateStructured(
                resultId, definition.Code, definition.Name,
                request.StructuredValue ?? "{}", unit, definition.Id),
            _ => throw new IngestionException(
                IngestionErrorCodes.InvalidValueType,
                $"Unsupported value type '{request.ValueType}'.",
                $"metrics.{request.MetricDefinitionCode}.valueType")
        };
    }

    private static void ValidateContractVersion(string version)
    {
        if (!string.Equals(version, SupportedContractVersion, StringComparison.Ordinal))
        {
            throw new IngestionException(
                IngestionErrorCodes.ValidationFailed,
                $"Unsupported ingestion contract version '{version}'. Supported: '{SupportedContractVersion}'.",
                "version");
        }
    }

    private static string ComputeContentHash(IngestSessionRequest request)
    {
        var builder = new StringBuilder();
        builder.Append(request.Version);
        builder.Append('|');
        builder.Append(request.SubjectId);
        builder.Append('|');
        builder.Append(request.ProtocolId);
        builder.Append('|');
        builder.Append(request.ProtocolVersion);
        builder.Append('|');
        builder.Append(request.ExternalId ?? string.Empty);
        builder.Append('|');
        builder.Append(request.SourceId?.ToString() ?? string.Empty);
        builder.Append('|');
        builder.Append(request.DeviceId?.ToString() ?? string.Empty);
        builder.Append('|');
        builder.Append(request.Timezone ?? string.Empty);
        builder.Append('|');
        builder.Append(request.ClientApplication ?? string.Empty);
        builder.Append('|');
        builder.Append(request.ClientVersion ?? string.Empty);

        if (request.Trials is not null)
        {
            foreach (var trial in request.Trials)
            {
                builder.Append("|T:");
                builder.Append(trial.TrialIndex);
                builder.Append(':');
                builder.Append(trial.ExternalId ?? string.Empty);
                builder.Append(':');
                builder.Append(trial.Notes ?? string.Empty);

                if (trial.Metrics is not null)
                {
                    foreach (var metric in trial.Metrics)
                    {
                        builder.Append("|M:");
                        builder.Append(metric.MetricDefinitionCode);
                        builder.Append(':');
                        builder.Append(metric.ValueType);
                        builder.Append(':');
                        builder.Append(metric.ScalarValue?.ToString("R") ?? string.Empty);
                        builder.Append(':');
                        builder.Append(metric.LeftValue?.ToString("R") ?? string.Empty);
                        builder.Append(':');
                        builder.Append(metric.RightValue?.ToString("R") ?? string.Empty);
                        builder.Append(':');
                        builder.Append(metric.CategoricalValue ?? string.Empty);
                        builder.Append(':');
                        builder.Append(metric.StructuredValue ?? string.Empty);
                        builder.Append(':');
                        builder.Append(metric.Unit ?? string.Empty);
                        builder.Append(':');
                        builder.Append(metric.Side ?? string.Empty);
                    }
                }
            }
        }

        if (request.AssetPlaceholders is not null)
        {
            foreach (var asset in request.AssetPlaceholders)
            {
                builder.Append("|A:");
                builder.Append(asset.AssetType);
                builder.Append(':');
                builder.Append(asset.ExternalId);
                builder.Append(':');
                builder.Append(asset.Size);
                builder.Append(':');
                builder.Append(asset.MimeType ?? string.Empty);
                builder.Append(':');
                builder.Append(asset.Checksum ?? string.Empty);
            }
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString()));
        return Convert.ToHexString(bytes);
    }

    private static IngestSessionResponse BuildPendingResponse(IngestionOperation operation)
    {
        return new IngestSessionResponse(
            SessionId: operation.SessionId ?? Guid.Empty,
            SessionExternalId: operation.SessionExternalId,
            Status: "Pending",
            Trials: null,
            AssetPlaceholders: null);
    }

    private static IngestSessionResponse ToResponse(IngestionOperation operation)
    {
        var errors = operation.Status == IngestionOperationStatus.Failed
            ? new List<IngestionError>
            {
                new(operation.ErrorCode ?? IngestionErrorCodes.InternalError,
                    operation.ErrorDetail ?? "Ingestion failed.")
            }
            : null;

        return new IngestSessionResponse(
            SessionId: operation.SessionId ?? Guid.Empty,
            SessionExternalId: operation.SessionExternalId,
            Status: operation.Status.ToString(),
            Trials: null,
            AssetPlaceholders: null,
            Errors: errors);
    }

    private static IngestionOperationStatusResponse ToStatusResponse(IngestionOperation operation) =>
        new(
            OperationId: operation.Id,
            ClientOperationId: operation.ClientOperationId,
            Status: operation.Status.ToString(),
            SessionId: operation.SessionId,
            SessionExternalId: operation.SessionExternalId,
            ErrorCode: operation.ErrorCode,
            ErrorDetail: operation.ErrorDetail,
            CompletedAt: operation.CompletedAt);
}