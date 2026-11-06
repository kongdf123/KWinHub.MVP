import type { TestSession } from './types';

let generatedSessions: TestSession[] = [];
const idempotencyIndex = new Map<string, TestSession>();

export function appendSession(session: TestSession): TestSession {
  const existing = idempotencyIndex.get(session.idempotencyKey);
  if (existing) {
    return existing;
  }
  generatedSessions = [session, ...generatedSessions];
  idempotencyIndex.set(session.idempotencyKey, session);
  return session;
}

export function findByIdempotencyKey(key: string): TestSession | undefined {
  return idempotencyIndex.get(key);
}

export function getGeneratedSessions(): TestSession[] {
  return [...generatedSessions];
}

export function clearGeneratedSessions(): void {
  generatedSessions = [];
  idempotencyIndex.clear();
}

export function generatedSessionCount(): number {
  return generatedSessions.length;
}