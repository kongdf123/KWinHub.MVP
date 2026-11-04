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

        var existingOperation = await _operationRepository.GetByClientOperationIdAsync(
            organizationId, request.ClientOperationId, cancellationToken);
        if (existingOperation is not null)
        {
            return ToResponse(existingOperation);
        }

        var operation = new IngestionOperation(organizationId, request.ClientOperationId);

        await using var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
        try
        {
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

            await transaction.CommitAsync(cancellationToken);

            operation.MarkAccepted(session.Id, session.ExternalId);
            await _operationRepository.UpdateAsync(operation, cancellationToken);

            return new IngestSessionResponse(
                SessionId: session.Id,
                SessionExternalId: session.ExternalId,
                Status: "Accepted",
                Trials: trialResults);
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