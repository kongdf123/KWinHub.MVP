export interface Subject {
  id: string
  organizationId: string
  firstName: string
  lastName: string
  subjectType: string
  externalId: string | null
  displayName: string | null
  dateOfBirth: string | null
  sex: string | null
  status: string
  createdAt: string
  updatedAt: string | null
}

export interface CreateSubjectRequest {
  firstName: string
  lastName: string
  subjectType: string
  externalId?: string
  displayName?: string
  dateOfBirth?: string
  sex?: string
}