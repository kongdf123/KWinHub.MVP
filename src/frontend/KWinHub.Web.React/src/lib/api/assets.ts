import { http } from '@/lib/api/http';

export type AssetStatus =
  | 'PendingUpload'
  | 'UploadInProgress'
  | 'UploadedUnverified'
  | 'UploadedVerified'
  | 'Processing'
  | 'Ready'
  | 'UploadFailed'
  | 'VerificationFailed'
  | 'ProcessingFailed';

export interface AssetDto {
  id: string;
  sessionId: string;
  trialId: string | null;
  assetType: string;
  externalId: string | null;
  mimeType: string | null;
  size: number;
  checksum: string | null;
  revision: number;
  status: AssetStatus;
  uploadedAt: string | null;
  verifiedAt: string | null;
  failureReason: string | null;
  mediaProviderId: string | null;
  playbackUrl: string | null;
  processedAt: string | null;
}

export interface PlaybackCredentialDto {
  assetId: string;
  downloadUrl: string;
  expiresAt: string;
  mimeType: string | null;
}

export const assetsApi = {
  listBySession: (sessionId: string) =>
    http.get<AssetDto[]>(`/assets/by-session/${sessionId}`),
  getById: (id: string) =>
    http.get<AssetDto>(`/assets/${id}`),
  requestPlaybackCredential: (id: string) =>
    http.get<PlaybackCredentialDto>(`/assets/${id}/playback-credential`),
};