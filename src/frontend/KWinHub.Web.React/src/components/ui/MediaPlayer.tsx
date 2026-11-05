import { useEffect, useRef, useState } from 'react';
import { Play, Pause, RotateCcw, AlertCircle, Loader2, Film, CheckCircle, Clock, XCircle } from 'lucide-react';
import { assetsApi, type AssetDto, type AssetStatus } from '@/lib/api/assets';

const STATUS_LABEL: Record<AssetStatus, { en: string; cn: string }> = {
  PendingUpload: { en: 'Pending upload', cn: '等待上传' },
  UploadInProgress: { en: 'Uploading', cn: '上传中' },
  UploadedUnverified: { en: 'Awaiting verification', cn: '待验证' },
  UploadedVerified: { en: 'Verified', cn: '已验证' },
  Processing: { en: 'Processing', cn: '处理中' },
  Ready: { en: 'Ready', cn: '就绪' },
  UploadFailed: { en: 'Upload failed', cn: '上传失败' },
  VerificationFailed: { en: 'Verification failed', cn: '验证失败' },
  ProcessingFailed: { en: 'Processing failed', cn: '处理失败' },
};

function StatusIcon({ status }: { status: AssetStatus }) {
  const cls = 'w-4 h-4 shrink-0';
  switch (status) {
    case 'Ready':
      return <CheckCircle className={`${cls} text-success-600`} />;
    case 'Processing':
    case 'UploadInProgress':
      return <Clock className={`${cls} text-teal-600`} />;
    case 'UploadFailed':
    case 'VerificationFailed':
    case 'ProcessingFailed':
      return <XCircle className={`${cls} text-invalid-600`} />;
    default:
      return <Clock className={`${cls} text-ink-400`} />;
  }
}

interface MediaPlayerProps {
  src: string;
  mimeType?: string | null;
  onExpired?: () => void;
}

export function MediaPlayer({ src, mimeType, onExpired }: MediaPlayerProps) {
  const videoRef = useRef<HTMLVideoElement>(null);
  const [playing, setPlaying] = useState(false);
  const [error, setError] = useState(false);

  useEffect(() => {
    setError(false);
    setPlaying(false);
  }, [src]);

  const togglePlay = () => {
    const v = videoRef.current;
    if (!v) return;
    if (v.paused) {
      v.play().catch(() => setError(true));
    } else {
      v.pause();
    }
  };

  const restart = () => {
    const v = videoRef.current;
    if (!v) return;
    v.currentTime = 0;
    v.play().catch(() => setError(true));
  };

  if (error) {
    return (
      <div className="flex flex-col items-center justify-center gap-2 h-56 bg-ink-50 rounded-lg border border-ink-200 text-ink-500">
        <AlertCircle className="w-8 h-8 text-invalid-500" />
        <p className="text-sm">Playback unavailable — link may have expired.</p>
        {onExpired && (
          <button
            onClick={onExpired}
            className="text-sm text-teal-600 hover:text-teal-700 font-medium"
          >
            Refresh playback link
          </button>
        )}
      </div>
    );
  }

  return (
    <div className="rounded-lg overflow-hidden bg-black border border-ink-200">
      <video
        ref={videoRef}
        src={src}
        type={mimeType ?? undefined}
        className="w-full h-auto max-h-[60vh] bg-black"
        controls
        playsInline
        onPlay={() => setPlaying(true)}
        onPause={() => setPlaying(false)}
        onEnded={() => setPlaying(false)}
        onError={() => setError(true)}
      />
      <div className="hidden">
        <button onClick={togglePlay}>{playing ? <Pause /> : <Play />}</button>
        <button onClick={restart}><RotateCcw /></button>
      </div>
    </div>
  );
}

interface SessionMediaPanelProps {
  sessionId: string;
  lang: 'en' | 'cn';
}

export function SessionMediaPanel({ sessionId, lang }: SessionMediaPanelProps) {
  const [assets, setAssets] = useState<AssetDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [credentialUrl, setCredentialUrl] = useState<string | null>(null);
  const [credentialMime, setCredentialMime] = useState<string | null>(null);
  const [credentialLoading, setCredentialLoading] = useState(false);
  const [selectedAssetId, setSelectedAssetId] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);
    assetsApi
      .listBySession(sessionId)
      .then((list) => {
        if (cancelled) return;
        setAssets(list);
      })
      .catch(() => {
        if (!cancelled) setError(lang === 'cn' ? '加载媒体资源失败' : 'Failed to load media assets');
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [sessionId, lang]);

  const requestPlayback = async (assetId: string) => {
    setCredentialLoading(true);
    setCredentialUrl(null);
    setSelectedAssetId(assetId);
    try {
      const cred = await assetsApi.requestPlaybackCredential(assetId);
      setCredentialUrl(cred.downloadUrl);
      setCredentialMime(cred.mimeType);
    } catch {
      setCredentialUrl(null);
    } finally {
      setCredentialLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-40 text-ink-400">
        <Loader2 className="w-5 h-5 animate-spin mr-2" />
        {lang === 'cn' ? '加载中...' : 'Loading...'}
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center h-40 text-invalid-600">
        <AlertCircle className="w-5 h-5 mr-2" />
        {error}
      </div>
    );
  }

  const videoAssets = assets.filter((a) => a.assetType === 'video' || (a.mimeType ?? '').startsWith('video/'));

  if (videoAssets.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center gap-2 h-40 text-ink-400">
        <Film className="w-8 h-8" />
        <p className="text-sm">{lang === 'cn' ? '该测试记录暂无视频资源' : 'No video assets for this session'}</p>
      </div>
    );
  }

  const selectedAsset = videoAssets.find((a) => a.id === selectedAssetId) ?? videoAssets[0];
  const isReady = selectedAsset.status === 'Ready';

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap gap-2">
        {videoAssets.map((asset) => {
          const label = STATUS_LABEL[asset.status];
          const isSelected = selectedAsset.id === asset.id;
          return (
            <button
              key={asset.id}
              onClick={() => {
                setSelectedAssetId(asset.id);
                setCredentialUrl(null);
              }}
              className={`flex items-center gap-2 px-3 py-1.5 rounded-lg text-sm font-medium border transition-colors ${
                isSelected
                  ? 'bg-teal-50 border-teal-300 text-teal-700'
                  : 'bg-white border-ink-200 text-ink-700 hover:bg-ink-50'
              }`}
            >
              <StatusIcon status={asset.status} />
              <span>{asset.externalId || (lang === 'cn' ? '视频' : 'Video')}</span>
              <span className="text-xs text-ink-400">· {lang === 'cn' ? label.cn : label.en}</span>
            </button>
          );
        })}
      </div>

      {isReady ? (
        <div className="space-y-3">
          {credentialUrl ? (
            <MediaPlayer
              src={credentialUrl}
              mimeType={credentialMime}
              onExpired={() => requestPlayback(selectedAsset.id)}
            />
          ) : (
            <div className="flex flex-col items-center justify-center gap-3 h-56 bg-ink-50 rounded-lg border border-ink-200">
              {credentialLoading ? (
                <>
                  <Loader2 className="w-6 h-6 animate-spin text-teal-600" />
                  <p className="text-sm text-ink-500">{lang === 'cn' ? '正在获取播放授权...' : 'Requesting playback credential...'}</p>
                </>
              ) : (
                <>
                  <Film className="w-8 h-8 text-ink-400" />
                  <button
                    onClick={() => requestPlayback(selectedAsset.id)}
                    className="flex items-center gap-2 px-4 py-2 bg-teal-600 hover:bg-teal-700 text-white text-sm font-medium rounded-lg transition-colors"
                  >
                    <Play className="w-4 h-4" />
                    {lang === 'cn' ? '播放视频' : 'Play video'}
                  </button>
                </>
              )}
            </div>
          )}
        </div>
      ) : (
        <div className="flex flex-col items-center justify-center gap-2 h-56 bg-ink-50 rounded-lg border border-ink-200 text-ink-500">
          <StatusIcon status={selectedAsset.status} />
          <p className="text-sm font-medium">
            {lang === 'cn' ? STATUS_LABEL[selectedAsset.status].cn : STATUS_LABEL[selectedAsset.status].en}
          </p>
          {selectedAsset.failureReason && (
            <p className="text-xs text-invalid-600">{selectedAsset.failureReason}</p>
          )}
          <p className="text-xs text-ink-400 mt-1">
            {lang === 'cn' ? '视频尚未就绪，不影响测试指标查看。' : 'Video is not ready yet — metrics remain available.'}
          </p>
        </div>
      )}
    </div>
  );
}