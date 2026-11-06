import { useEffect, useState } from 'react';
import { useApp } from '@/lib/app-context';
import { t as tr } from '@/lib/i18n';
import { TopBar } from '@/components/layout/TopBar';
import { reportsApi, type ReportDetailResponse, type ReportAssetStateResponse, type ReportMetricValueResponse } from '@/lib/api/reports';
import { subjectsApi, type SubjectDto } from '@/lib/api/subjects';
import { ArrowLeft, FileText, Clock, User, CheckCircle, AlertTriangle, XCircle, Play, Loader2, Shield, History } from 'lucide-react';

const STATUS_MAP: Record<string, { label: string; color: string; dot: string }> = {
  Draft: { label: 'Draft', color: 'text-amber-700 bg-amber-50', dot: 'bg-amber-500' },
  Complete: { label: 'Complete', color: 'text-success-700 bg-success-50', dot: 'bg-success-500' },
  Archived: { label: 'Archived', color: 'text-ink-600 bg-ink-100', dot: 'bg-ink-400' },
};

const ASSET_STATUS_MAP: Record<string, { label: string; color: string; icon: typeof CheckCircle }> = {
  Ready: { label: 'Ready', color: 'text-success-700 bg-success-50', icon: CheckCircle },
  Failed: { label: 'Failed', color: 'text-red-700 bg-red-50', icon: XCircle },
  Pending: { label: 'Pending', color: 'text-ink-600 bg-ink-100', icon: Clock },
  Processing: { label: 'Processing', color: 'text-teal-700 bg-teal-50', icon: Loader2 },
};

export function ReportDetailPage() {
  const { lang, viewParams, navigate, addToast } = useApp();
  const reportId = viewParams.reportId || '';
  const [report, setReport] = useState<ReportDetailResponse | null>(null);
  const [subject, setSubject] = useState<SubjectDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [playbackAssetId, setPlaybackAssetId] = useState<string | null>(null);
  const [playbackUrl, setPlaybackUrl] = useState<string | null>(null);

  const load = async () => {
    if (!reportId) return;
    setLoading(true);
    try {
      const r = await reportsApi.getById(reportId);
      setReport(r);
      subjectsApi.getById(r.subjectId).then(setSubject).catch(() => setSubject(null));
    } catch {
      addToast(lang === 'cn' ? '加载报告失败' : 'Failed to load report', 'error');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void load();
  }, [reportId]);

  const handleRequestPlayback = async (asset: ReportAssetStateResponse) => {
    if (!asset.playbackAuthorized || !asset.playbackUrl) {
      addToast(lang === 'cn' ? '该视频未就绪，无法播放' : 'This video is not ready for playback', 'error');
      return;
    }
    setPlaybackAssetId(asset.assetId);
    setPlaybackUrl(null);
    try {
      const auth = await reportsApi.requestPlayback(reportId, asset.assetId);
      if (auth.authorized && auth.playbackUrl) {
        setPlaybackUrl(auth.playbackUrl);
      } else {
        addToast(auth.reason || (lang === 'cn' ? '播放授权失败' : 'Playback authorization failed'), 'error');
        setPlaybackAssetId(null);
      }
    } catch (e) {
      addToast(e instanceof Error ? e.message : (lang === 'cn' ? '播放授权失败' : 'Playback authorization failed'), 'error');
      setPlaybackAssetId(null);
    }
  };

  if (loading) {
    return (
      <div>
        <TopBar />
        <div className="flex items-center justify-center py-20">
          <Loader2 className="w-6 h-6 animate-spin text-ink-400" />
        </div>
      </div>
    );
  }

  if (!report) {
    return (
      <div>
        <TopBar />
        <div className="px-6 py-10 text-center">
          <p className="text-ink-500">{lang === 'cn' ? '报告未找到' : 'Report not found'}</p>
          <button onClick={() => navigate('reports')} className="mt-4 text-teal-600 hover:text-teal-700 text-sm font-medium">
            {lang === 'cn' ? '返回报告列表' : 'Back to reports'}
          </button>
        </div>
      </div>
    );
  }

  const cfg = STATUS_MAP[report.status] ?? STATUS_MAP.Draft;
  const subjectName = subject ? `${subject.firstName} ${subject.lastName}` : report.subjectId.slice(0, 8);

  return (
    <div>
      <TopBar />

      <div className="px-6 pt-6">
        <button
          onClick={() => navigate('reports')}
          className="flex items-center gap-1 text-sm text-ink-500 hover:text-ink-700 mb-4"
        >
          <ArrowLeft className="w-4 h-4" /> {lang === 'cn' ? '返回报告列表' : 'Back to reports'}
        </button>

        <div className="flex items-start justify-between gap-4">
          <div>
            <h2 className="text-xl font-semibold text-content-primary">{report.title}</h2>
            <p className="text-sm text-ink-500 mt-1">
              {report.planName} · {subjectName}
            </p>
          </div>
          <span className={`inline-flex items-center gap-1.5 text-xs px-2.5 py-1 rounded-full font-medium ${cfg.color}`}>
            <span className={`w-1.5 h-1.5 rounded-full ${cfg.dot}`} />
            {cfg.label}
          </span>
        </div>
      </div>

      <div className="px-6 py-6 space-y-6">
        {/* Provenance */}
        <section className="bg-white rounded-xl border border-ink-200 p-5">
          <div className="flex items-center gap-2 mb-4">
            <Shield className="w-4 h-4 text-ink-500" />
            <h3 className="text-sm font-semibold text-ink-900">{lang === 'cn' ? '来源与版本' : 'Provenance'}</h3>
          </div>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <ProvenanceField icon={FileText} label={lang === 'cn' ? '评估计划' : 'Plan'} value={`${report.planCode} v${report.planVersion}`} />
            <ProvenanceField icon={User} label={lang === 'cn' ? '生成人' : 'Generated By'} value={report.generatedByUserId.slice(0, 8)} />
            <ProvenanceField icon={Clock} label={lang === 'cn' ? '生成时间' : 'Generated At'} value={new Date(report.generatedAt).toLocaleString()} />
            <ProvenanceField icon={History} label={lang === 'cn' ? '报告状态' : 'Status'} value={cfg.label} />
          </div>
          {report.notes && (
            <div className="mt-4 pt-4 border-t border-ink-100">
              <p className="text-xs text-ink-500 uppercase tracking-wider mb-1">{lang === 'cn' ? '备注' : 'Notes'}</p>
              <p className="text-sm text-ink-700">{report.notes}</p>
            </div>
          )}
        </section>

        {/* Metric Values */}
        <section className="bg-white rounded-xl border border-ink-200 overflow-hidden">
          <div className="flex items-center gap-2 px-5 py-4 border-b border-ink-100">
            <FileText className="w-4 h-4 text-ink-500" />
            <h3 className="text-sm font-semibold text-ink-900">{lang === 'cn' ? '指标值快照' : 'Metric Values'}</h3>
          </div>
          {report.metricValues.length === 0 ? (
            <p className="px-5 py-8 text-center text-sm text-ink-400">{lang === 'cn' ? '无指标值' : 'No metric values'}</p>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-ink-50 text-left text-xs text-ink-500 uppercase tracking-wider">
                    <th className="px-5 py-3 font-medium">{lang === 'cn' ? '指标' : 'Metric'}</th>
                    <th className="px-5 py-3 font-medium">{lang === 'cn' ? '值' : 'Value'}</th>
                    <th className="px-5 py-3 font-medium">{lang === 'cn' ? '单位' : 'Unit'}</th>
                    <th className="px-5 py-3 font-medium">{lang === 'cn' ? '来源会话' : 'Source Session'}</th>
                    <th className="px-5 py-3 font-medium">{lang === 'cn' ? '测试' : 'Trial'}</th>
                    <th className="px-5 py-3 font-medium">{lang === 'cn' ? '必需' : 'Required'}</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-ink-100">
                  {report.metricValues
                    .slice()
                    .sort((a, b) => a.planItemSortOrder - b.planItemSortOrder)
                    .map((mv) => (
                      <MetricValueRow key={mv.id} mv={mv} lang={lang} />
                    ))}
                </tbody>
              </table>
            </div>
          )}
        </section>

        {/* Asset States / Videos */}
        <section className="bg-white rounded-xl border border-ink-200 overflow-hidden">
          <div className="flex items-center gap-2 px-5 py-4 border-b border-ink-100">
            <Play className="w-4 h-4 text-ink-500" />
            <h3 className="text-sm font-semibold text-ink-900">{lang === 'cn' ? '视频素材' : 'Video Assets'}</h3>
            <span className="ml-2 text-xs text-ink-400">({report.assetStates.length})</span>
          </div>
          {report.assetStates.length === 0 ? (
            <p className="px-5 py-8 text-center text-sm text-ink-400">{lang === 'cn' ? '无视频素材' : 'No video assets'}</p>
          ) : (
            <div className="divide-y divide-ink-100">
              {report.assetStates.map((asset) => (
                <AssetStateRow
                  key={asset.id}
                  asset={asset}
                  isPlaying={playbackAssetId === asset.assetId}
                  playbackUrl={playbackAssetId === asset.assetId ? playbackUrl : null}
                  onPlay={() => handleRequestPlayback(asset)}
                  lang={lang}
                />
              ))}
            </div>
          )}
        </section>
      </div>
    </div>
  );
}

function ProvenanceField({ icon: Icon, label, value }: { icon: typeof FileText; label: string; value: string }) {
  return (
    <div>
      <div className="flex items-center gap-1.5 text-xs text-ink-500 uppercase tracking-wider mb-1">
        <Icon className="w-3.5 h-3.5" />
        {label}
      </div>
      <p className="text-sm font-medium text-ink-800 break-all">{value}</p>
    </div>
  );
}

function MetricValueRow({ mv, lang }: { mv: ReportMetricValueResponse; lang: 'cn' | 'en' }) {
  return (
    <tr className="hover:bg-ink-50">
      <td className="px-5 py-3">
        <div className="font-medium text-ink-800">{mv.metricName}</div>
        <div className="text-xs text-ink-400">{mv.metricCode}</div>
      </td>
      <td className="px-5 py-3 font-mono text-ink-800">
        {mv.value !== null ? mv.value : <span className="text-ink-300">—</span>}
      </td>
      <td className="px-5 py-3 text-ink-600">{mv.unit || '—'}</td>
      <td className="px-5 py-3">
        <code className="text-xs bg-ink-100 px-1.5 py-0.5 rounded text-ink-600">{mv.sessionId.slice(0, 8)}</code>
      </td>
      <td className="px-5 py-3">
        {mv.trialId ? <code className="text-xs bg-ink-100 px-1.5 py-0.5 rounded text-ink-600">{mv.trialId.slice(0, 8)}</code> : <span className="text-ink-300">—</span>}
      </td>
      <td className="px-5 py-3">
        {mv.required ? (
          <span className="inline-flex items-center gap-1 text-xs text-amber-700"><AlertTriangle className="w-3 h-3" /> {lang === 'cn' ? '必需' : 'Required'}</span>
        ) : (
          <span className="text-xs text-ink-400">{lang === 'cn' ? '可选' : 'Optional'}</span>
        )}
      </td>
    </tr>
  );
}

function AssetStateRow({
  asset,
  isPlaying,
  playbackUrl,
  onPlay,
  lang,
}: {
  asset: ReportAssetStateResponse;
  isPlaying: boolean;
  playbackUrl: string | null;
  onPlay: () => void;
  lang: 'cn' | 'en';
}) {
  const statusCfg = ASSET_STATUS_MAP[asset.status] ?? ASSET_STATUS_MAP.Pending;
  const StatusIcon = statusCfg.icon;

  return (
    <div className="px-5 py-4">
      <div className="flex items-center justify-between gap-4">
        <div className="flex items-center gap-3 min-w-0">
          <div className="w-10 h-10 rounded-lg bg-ink-100 flex items-center justify-center shrink-0">
            <Play className="w-4 h-4 text-ink-500" />
          </div>
          <div className="min-w-0">
            <div className="flex items-center gap-2">
              <span className="text-sm font-medium text-ink-800">{asset.assetType}</span>
              <span className={`inline-flex items-center gap-1 text-xs px-2 py-0.5 rounded-full font-medium ${statusCfg.color}`}>
                <StatusIcon className={`w-3 h-3 ${asset.status === 'Processing' ? 'animate-spin' : ''}`} />
                {statusCfg.label}
              </span>
            </div>
            <p className="text-xs text-ink-500 mt-0.5">
              {lang === 'cn' ? '会话' : 'Session'}: <code className="bg-ink-100 px-1 rounded">{asset.sessionId.slice(0, 8)}</code>
              {asset.trialId && <> · {lang === 'cn' ? '测试' : 'Trial'}: <code className="bg-ink-100 px-1 rounded">{asset.trialId.slice(0, 8)}</code></>}
              {asset.processedAt && <> · {new Date(asset.processedAt).toLocaleDateString()}</>}
            </p>
            {asset.failureReason && (
              <p className="text-xs text-red-600 mt-1">{lang === 'cn' ? '失败原因' : 'Reason'}: {asset.failureReason}</p>
            )}
          </div>
        </div>
        <button
          onClick={onPlay}
          disabled={!asset.playbackAuthorized}
          className={`flex items-center gap-1.5 px-3 py-2 text-sm font-medium rounded-lg transition-colors shrink-0 ${
            asset.playbackAuthorized
              ? 'bg-teal-600 hover:bg-teal-700 text-white'
              : 'bg-ink-100 text-ink-400 cursor-not-allowed'
          }`}
        >
          <Play className="w-4 h-4" />
          {lang === 'cn' ? '播放' : 'Play'}
        </button>
      </div>

      {isPlaying && playbackUrl && (
        <div className="mt-4 rounded-lg overflow-hidden bg-black aspect-video">
          <video src={playbackUrl} controls autoPlay className="w-full h-full" />
        </div>
      )}
    </div>
  );
}