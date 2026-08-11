"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import { 
  ArrowLeft, CheckCircle2, AlertTriangle, ShieldCheck, 
  BarChart3, ShieldAlert, AlertCircle, RefreshCw, Layers 
} from "lucide-react";

interface QaExecution {
  id: string;
  projectId: string;
  buildId: string;
  startedAt: string;
  completedAt: string | null;
  status: string;
  coveragePercent: number;
  qualityScore: number;
  securityScore: number;
  bugCount: number;
  warningCount: number;
  passed: boolean;
}

interface QaIssue {
  id: string;
  qaExecutionId: string;
  severity: "Info" | "Minor" | "Major" | "Critical" | "Blocker";
  type: "Sonar" | "Playwright" | "Compiler";
  title: string;
  description: string;
  filePath: string | null;
  lineNumber: number;
  createdAt: string;
}

interface HistoryItem {
  id: string;
  buildId: string;
  startedAt: string;
  completedAt: string | null;
  status: string;
  qualityScore: number;
  passed: boolean;
}

export default function QualityDashboard() {
  const [projectId] = useState<string>("proj-1"); // Use dummy project id for preview
  const [loading, setLoading] = useState<boolean>(true);
  const [scorecard, setScorecard] = useState<QaExecution | null>(null);
  const [issues, setIssues] = useState<QaIssue[]>([]);
  const [history, setHistory] = useState<HistoryItem[]>([]);

  const fetchQualityData = async () => {
    setLoading(true);
    try {
      const apiBase = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080";
      
      const scorecardRes = await fetch(`${apiBase}/api/quality/${projectId}`);
      if (scorecardRes.ok) {
        const data = await scorecardRes.json();
        setScorecard(data);
      }

      const issuesRes = await fetch(`${apiBase}/api/quality/${projectId}/issues`);
      if (issuesRes.ok) {
        const data = await issuesRes.json();
        setIssues(data);
      }

      const historyRes = await fetch(`${apiBase}/api/quality/${projectId}/history`);
      if (historyRes.ok) {
        const data = await historyRes.json();
        setHistory(data);
      }
    } catch (err) {
      console.warn("API offline, falling back to mock quality dashboard telemetry.", err);
      // Premium Mock Fallback
      setScorecard({
        id: "exec-1",
        projectId: "proj-1",
        buildId: "build-14",
        startedAt: new Date(Date.now() - 300000).toISOString(),
        completedAt: new Date().toISOString(),
        status: "Passed",
        coveragePercent: 78.4,
        qualityScore: 92,
        securityScore: 95,
        bugCount: 1,
        warningCount: 3,
        passed: true
      });

      setIssues([
        {
          id: "issue-101",
          qaExecutionId: "exec-1",
          severity: "Critical",
          type: "Playwright",
          title: "CRUD: Manage Entities failed",
          description: "Assertion error: Customer 'Acme Corp' deleted row still present in Table.",
          filePath: "tests/playwright-smoke.spec.js",
          lineNumber: 48,
          createdAt: new Date().toISOString()
        },
        {
          id: "issue-102",
          qaExecutionId: "exec-1",
          severity: "Major",
          type: "Sonar",
          title: "csharpsquid:S112",
          description: "Avoid throwing generic exception System.Exception. Use specific custom exceptions.",
          filePath: "src/Application/Features/Orders/Commands/CreateOrder.cs",
          lineNumber: 48,
          createdAt: new Date().toISOString()
        },
        {
          id: "issue-103",
          qaExecutionId: "exec-1",
          severity: "Minor",
          type: "Sonar",
          title: "csharpsquid:S1172",
          description: "Remove unused parameter 'args' or make use of it.",
          filePath: "src/BuildWorker/Program.cs",
          lineNumber: 8,
          createdAt: new Date().toISOString()
        }
      ]);

      setHistory([
        { id: "exec-1", buildId: "job-14", startedAt: "2026-06-02T12:00:00Z", completedAt: "2026-06-02T12:05:00Z", status: "Passed", qualityScore: 92, passed: true },
        { id: "exec-2", buildId: "job-13", startedAt: "2026-06-02T10:00:00Z", completedAt: "2026-06-02T10:03:00Z", status: "Passed", qualityScore: 89, passed: true },
        { id: "exec-3", buildId: "job-12", startedAt: "2026-06-01T15:00:00Z", completedAt: "2026-06-01T15:06:00Z", status: "Failed", qualityScore: 54, passed: false }
      ]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchQualityData();
  }, [projectId]);

  const getSeverityBadge = (severity: QaIssue["severity"]) => {
    switch (severity) {
      case "Blocker":
      case "Critical":
        return <span className="px-2 py-0.5 text-xs font-semibold bg-rose-950/40 text-rose-400 border border-rose-800/40 rounded">Critical</span>;
      case "Major":
        return <span className="px-2 py-0.5 text-xs font-semibold bg-amber-950/40 text-amber-400 border border-amber-800/40 rounded">Major</span>;
      default:
        return <span className="px-2 py-0.5 text-xs font-semibold bg-zinc-900 text-zinc-400 border border-zinc-700/40 rounded">Minor</span>;
    }
  };

  const getTypeIcon = (type: QaIssue["type"]) => {
    switch (type) {
      case "Playwright":
        return <CheckCircle2 className="w-4 h-4 text-emerald-400" />;
      case "Compiler":
        return <ShieldAlert className="w-4 h-4 text-rose-400" />;
      default:
        return <Layers className="w-4 h-4 text-indigo-400" />;
    }
  };

  return (
    <div className="min-h-screen bg-[#050508] text-zinc-100 font-sans p-6 md:p-12 selection:bg-indigo-500 selection:text-white">
      {/* Back link */}
      <div className="mb-6">
        <Link href="/" className="inline-flex items-center gap-2 text-zinc-400 hover:text-indigo-400 transition-colors text-sm font-semibold">
          <ArrowLeft className="w-4 h-4" /> Back to Dashboard
        </Link>
      </div>

      {/* Header */}
      <header className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 border-b border-zinc-800/80 pb-8 mb-12">
        <div>
          <h1 className="text-3xl font-extrabold tracking-tight bg-gradient-to-r from-indigo-400 via-purple-400 to-pink-400 bg-clip-text text-transparent">
            Quality Assurance Scorecard
          </h1>
          <p className="text-zinc-400 text-sm mt-1">Automated QA Metrics & Code Verification Gates</p>
        </div>
        <div>
          <button 
            onClick={fetchQualityData} 
            className="flex items-center gap-2 px-4 py-2 bg-zinc-900 border border-zinc-800 hover:border-zinc-700 rounded-lg text-sm font-semibold text-zinc-200 transition-all"
            disabled={loading}
          >
            <RefreshCw className={`w-4 h-4 ${loading ? "animate-spin" : ""}`} /> Refresh Report
          </button>
        </div>
      </header>

      {scorecard && (
        <>
          {/* Metrics Grid */}
          <section className="grid grid-cols-1 md:grid-cols-5 gap-6 mb-12">
            <div className="bg-[#0b0c14] border border-zinc-800/60 p-6 rounded-xl flex flex-col justify-between shadow-xl relative overflow-hidden">
              <div className="flex items-center justify-between mb-2">
                <span className="text-xs text-zinc-400 uppercase tracking-wider font-semibold">Quality Score</span>
                <BarChart3 className="w-5 h-5 text-indigo-400" />
              </div>
              <div className="mt-4">
                <span className="text-4xl font-extrabold text-indigo-400">{scorecard.qualityScore}<span className="text-lg text-zinc-500">/100</span></span>
                <span className={`text-xs block mt-2 font-medium ${scorecard.passed ? "text-emerald-400" : "text-rose-400"}`}>
                  {scorecard.passed ? "Passed QA Gate" : "Failed QA Gate"}
                </span>
              </div>
            </div>

            <div className="bg-[#0b0c14] border border-zinc-800/60 p-6 rounded-xl flex flex-col justify-between shadow-xl">
              <div className="flex items-center justify-between mb-2">
                <span className="text-xs text-zinc-400 uppercase tracking-wider font-semibold">Code Coverage</span>
                <CheckCircle2 className="w-5 h-5 text-emerald-400" />
              </div>
              <div className="mt-4">
                <span className="text-4xl font-extrabold text-emerald-400">{scorecard.coveragePercent}%</span>
                <span className="text-xs text-zinc-500 block mt-2">Target minimum 70%</span>
              </div>
            </div>

            <div className="bg-[#0b0c14] border border-zinc-800/60 p-6 rounded-xl flex flex-col justify-between shadow-xl">
              <div className="flex items-center justify-between mb-2">
                <span className="text-xs text-zinc-400 uppercase tracking-wider font-semibold">Security Rating</span>
                <ShieldCheck className="w-5 h-5 text-purple-400" />
              </div>
              <div className="mt-4">
                <span className="text-4xl font-extrabold text-purple-400">A</span>
                <span className="text-xs text-zinc-500 block mt-2">Security Score: {scorecard.securityScore}%</span>
              </div>
            </div>

            <div className="bg-[#0b0c14] border border-zinc-800/60 p-6 rounded-xl flex flex-col justify-between shadow-xl">
              <div className="flex items-center justify-between mb-2">
                <span className="text-xs text-zinc-400 uppercase tracking-wider font-semibold">Verification Build</span>
                <AlertCircle className="w-5 h-5 text-pink-400" />
              </div>
              <div className="mt-4">
                <span className="text-lg font-bold block overflow-hidden text-ellipsis whitespace-nowrap">{scorecard.buildId.split("-")[0] || "job-14"}</span>
                <span className="text-xs text-zinc-500 block mt-2">Status: {scorecard.status}</span>
              </div>
            </div>

            <div className="bg-[#0b0c14] border border-zinc-800/60 p-6 rounded-xl flex flex-col justify-between shadow-xl">
              <div className="flex items-center justify-between mb-2">
                <span className="text-xs text-zinc-400 uppercase tracking-wider font-semibold">Active Issues</span>
                <ShieldAlert className="w-5 h-5 text-rose-400" />
              </div>
              <div className="mt-4">
                <span className="text-4xl font-extrabold text-rose-400">{issues.length}</span>
                <span className="text-xs text-zinc-500 block mt-2">{scorecard.bugCount} bugs, {scorecard.warningCount} warnings</span>
              </div>
            </div>
          </section>
        </>
      )}

      {/* Workspace Panel */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Left Column: Issues List */}
        <main className="lg:col-span-2 space-y-6">
          <div className="bg-[#0b0c14]/40 border border-zinc-800/80 rounded-xl p-6 shadow-xl">
            <h2 className="text-xl font-bold flex items-center gap-2 mb-6">
              <AlertTriangle className="w-5 h-5 text-indigo-400" /> Quality Defect Records
            </h2>

            {issues.length === 0 ? (
              <div className="text-center py-12 text-zinc-500">
                No issues found. Codebase matches all Quality Gate rules.
              </div>
            ) : (
              <div className="divide-y divide-zinc-900">
                {issues.map((i) => (
                  <div key={i.id} className="py-4 flex gap-4 first:pt-0 last:pb-0">
                    <div className="mt-1">{getTypeIcon(i.type)}</div>
                    <div className="flex-1">
                      <div className="flex items-center gap-2 flex-wrap">
                        <span className="font-semibold text-zinc-200">{i.title}</span>
                        {getSeverityBadge(i.severity)}
                        <span className="text-xs font-mono text-zinc-500">[{i.type}]</span>
                      </div>
                      <p className="text-sm text-zinc-400 mt-2">{i.description}</p>
                      {i.filePath && (
                        <div className="text-xs font-mono text-zinc-600 mt-2">
                          File: {i.filePath}:{i.lineNumber}
                        </div>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </main>

        {/* Right Column: Historical QA Runs & Criteria */}
        <aside className="space-y-6">
          <div className="bg-[#0b0c14]/40 border border-zinc-800/80 rounded-xl p-6 shadow-xl">
            <h2 className="text-xl font-bold flex items-center gap-2 mb-6">
              <RefreshCw className="w-5 h-5 text-purple-400" /> Historical Runs
            </h2>

            <div className="space-y-4">
              {history.map((h) => (
                <div key={h.id} className="flex justify-between items-center bg-[#050508] border border-zinc-900 p-4 rounded-lg">
                  <div>
                    <span className="text-sm font-semibold block">Build: {h.buildId.split("-")[0]}</span>
                    <span className="text-xs text-zinc-500">Run completed</span>
                  </div>
                  <div className="text-right">
                    <span className={`text-sm font-bold block ${h.passed ? "text-emerald-400" : "text-rose-400"}`}>
                      {h.qualityScore}/100
                    </span>
                    <span className="text-xs text-zinc-600">{h.status}</span>
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-[#0b0c14]/40 border border-zinc-800/80 rounded-xl p-6 shadow-xl">
            <h2 className="text-sm uppercase tracking-wider text-zinc-400 font-bold mb-4">Gate Promotion Criteria</h2>
            <ul className="text-xs text-zinc-500 space-y-3 font-medium">
              <li className="flex items-center gap-2">
                <span className="w-1.5 h-1.5 bg-indigo-400 rounded-full"></span> Code Coverage must exceed 70%.
              </li>
              <li className="flex items-center gap-2">
                <span className="w-1.5 h-1.5 bg-indigo-400 rounded-full"></span> Zero critical vulnerabilities permitted.
              </li>
              <li className="flex items-center gap-2">
                <span className="w-1.5 h-1.5 bg-indigo-400 rounded-full"></span> Blocker defects must be resolved.
              </li>
              <li className="flex items-center gap-2">
                <span className="w-1.5 h-1.5 bg-indigo-400 rounded-full"></span> Playwright regression tests must pass.
              </li>
            </ul>
          </div>
        </aside>
      </div>
    </div>
  );
}
