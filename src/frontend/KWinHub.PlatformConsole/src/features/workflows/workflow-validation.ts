import type { WorkflowDefinition } from './types';

export interface ValidationError {
  workflowId: string;
  stepId?: string;
  edgeId?: string;
  message: string;
}

export function validateWorkflow(workflow: WorkflowDefinition): ValidationError[] {
  const errors: ValidationError[] = [];
  const stepIds = new Set<string>();

  for (const step of workflow.steps) {
    if (stepIds.has(step.id)) {
      errors.push({
        workflowId: workflow.id,
        stepId: step.id,
        message: `Duplicate step id: ${step.id}`,
      });
    }
    stepIds.add(step.id);

    if (step.specificationRefs.length === 0) {
      errors.push({
        workflowId: workflow.id,
        stepId: step.id,
        message: `Step ${step.id} has no specification references`,
      });
    }
  }

  for (const edge of workflow.edges) {
    if (!stepIds.has(edge.from)) {
      errors.push({
        workflowId: workflow.id,
        edgeId: edge.id,
        message: `Edge ${edge.id} references unknown source step: ${edge.from}`,
      });
    }
    if (!stepIds.has(edge.to)) {
      errors.push({
        workflowId: workflow.id,
        edgeId: edge.id,
        message: `Edge ${edge.id} references unknown target step: ${edge.to}`,
      });
    }
  }

  for (const stepId of workflow.happyPath) {
    if (!stepIds.has(stepId)) {
      errors.push({
        workflowId: workflow.id,
        stepId,
        message: `Happy path references unknown step: ${stepId}`,
      });
    }
  }

  if (workflow.recoveryBranch) {
    for (const stepId of workflow.recoveryBranch) {
      if (!stepIds.has(stepId)) {
        errors.push({
          workflowId: workflow.id,
          stepId,
          message: `Recovery branch references unknown step: ${stepId}`,
        });
      }
    }
  }

  return errors;
}

export function validateAllWorkflows(workflows: WorkflowDefinition[]): ValidationError[] {
  const errors: ValidationError[] = [];
  const workflowIds = new Set<string>();

  for (const workflow of workflows) {
    if (workflowIds.has(workflow.id)) {
      errors.push({
        workflowId: workflow.id,
        message: `Duplicate workflow id: ${workflow.id}`,
      });
    }
    workflowIds.add(workflow.id);
    errors.push(...validateWorkflow(workflow));
  }

  return errors;
}