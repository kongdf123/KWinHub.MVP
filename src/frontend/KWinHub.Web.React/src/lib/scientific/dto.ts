export type ScientificRevisionStatus = 'Draft' | 'Validated' | 'Published' | 'Superseded';

export type ScientificSide = 'left' | 'right' | 'front' | 'rear' | 'bilateral' | 'total';

export interface ScientificAlgorithmVersion {
  code: string;
  version: string;
  name: string;
  nameCn: string;
  domain: string;
  description: string;
  status: 'Draft' | 'Approved' | 'Deprecated';
  supportedMetricCodes: string[];
}

export interface ScientificMetricDefinition {
  code: string;
  version: string;
  name: string;
  nameCn: string;
  unit: string;
  method: string;
  methodCn: string;
  side: ScientificSide;
  description: string;
  descriptionCn: string;
  category: string;
}

export interface ScientificMetricValue {
  metricCode: string;
  definitionVersion: string;
  method: string;
  value: number;
  unit: string;
  side?: ScientificSide;
  confidence: number;
  qualityFlags: string[];
}

export interface ScientificEventMarker {
  code: string;
  codeCn: string;
  offsetMs: number;
  detectionMethod: string;
  confidence: number;
}

export interface ScientificSegment {
  code: string;
  startMs: number;
  endMs: number;
}

export interface ScientificSignalChannel {
  label: string;
  labelCn: string;
  deviceCode: string;
  side?: ScientificSide;
  sampleRateHz: number;
  durationMs: number;
  samples: number[];
  events: ScientificEventMarker[];
}

export interface ScientificTrialInput {
  trialNumber: number;
  bodyMassKg: number;
  bodyMassSource: string;
  channels: ScientificSignalChannel[];
}

export interface ScientificTrialResult {
  trialNumber: number;
  revisionNumber: number;
  algorithmVersion: { code: string; version: string };
  metrics: ScientificMetricValue[];
  events: ScientificEventMarker[];
  segments: ScientificSegment[];
  qualityFlags: string[];
}

export interface ScientificResultRevision {
  id: string;
  trialId: string;
  revisionNumber: number;
  status: ScientificRevisionStatus;
  algorithmCode: string;
  algorithmVersion: string;
  metricDefinitions: ScientificMetricDefinition[];
  metricValues: ScientificMetricValue[];
  events: ScientificEventMarker[];
  segments: ScientificSegment[];
  inputFingerprint: string;
  supersedesId: string | null;
  calculatedAt: string;
  publishedAt: string | null;
  publishedBy: string | null;
  qualityFlags: string[];
  provenance: {
    parameters: Record<string, string | number | boolean>;
    inputSchemaVersion: string;
  };
}

export interface ScientificCalculationJob {
  id: string;
  trialId: string;
  algorithmCode: string;
  algorithmVersion: string;
  status: 'Queued' | 'Leased' | 'Running' | 'Succeeded' | 'Failed' | 'Cancelled';
  reason: string;
  retryCount: number;
  maxRetries: number;
  createdAt: string;
  leaseExpiresAt: string | null;
  errorCode: string | null;
  resultRevisionId: string | null;
}

export interface ScientificScenario {
  id: string;
  label: string;
  labelCn: string;
  description: string;
  descriptionCn: string;
  athleteId: string;
  protocolCode: string;
  trials: {
    number: number;
    status: 'valid' | 'uploading' | 'validating' | 'calculating' | 'calculated' | 'publishing' | 'published' | 'failed';
    input: ScientificTrialInput;
    revision: ScientificResultRevision | null;
    calculationJob: ScientificCalculationJob | null;
  }[];
  timelineSummary: {
    upload: string;
    validation: string;
    calculation: string;
    publication: string;
  };
}