export type DecisionStatus =
  | 'AGREED'
  | 'PROPOSED'
  | 'ASSUMPTION'
  | 'OPEN QUESTION'
  | 'OUT OF SCOPE';

export type ImplementationStatus =
  | 'unknown'
  | 'not-started'
  | 'prototype'
  | 'integrated'
  | 'verified';

export type DemoBehavior = 'unknown' | 'mock' | 'mixed' | 'live';

export interface LocalizedText {
  en: string;
  zhCN: string;
}

export interface SpecificationRef {
  documentPath: string;
  section?: string;
  requirementId?: string;
}

export interface WorkflowStep {
  id: string;
  title: LocalizedText;
  description: LocalizedText;
  actor: LocalizedText;
  preconditions?: LocalizedText[];
  expectedOutcome?: LocalizedText;
  alternateFlows?: LocalizedText[];
  acceptanceCriteria?: LocalizedText[];
  requirementStatus: DecisionStatus;
  implementationStatus: ImplementationStatus;
  demoBehavior: DemoBehavior;
  specificationRefs: SpecificationRef[];
  demoRoute?: string;
}

export interface WorkflowEdge {
  id: string;
  from: string;
  to: string;
  label?: LocalizedText;
}

export interface WorkflowDefinition {
  id: string;
  name: LocalizedText;
  persona: LocalizedText;
  summary: LocalizedText;
  happyPath: string[];
  recoveryBranch?: string[];
  steps: WorkflowStep[];
  edges: WorkflowEdge[];
}