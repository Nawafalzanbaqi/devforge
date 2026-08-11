"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import { 
  ArrowLeft, RefreshCw, CheckCircle2, AlertTriangle, 
  Activity, GitBranch, FileCode, Clock, Terminal, 
  Sparkles, ShieldCheck, ChevronRight
} from "lucide-react";

interface RepairTicket {
  id: string;
  projectId: string;
  projectName: string;
  buildId: string;
  source: string;
  severity: string;
  status: string;
  failureType: string;
  errorMessage: string;
  filePath: string | null;
  lineNumber: number;
  attempts: number;
  createdAt: string;
  resolvedAt: string | null;
}

interface RepairStats {
  totalRepairs: number;
  successRate: number;
  avgAttempts: number;
  openTickets: number;
  resolvedTickets: number;
}

export default function RepairsDashboard() {
  const [loading, setLoading] = useState<boolean>(true);
  const [repairs, setRepairs] = useState<RepairTicket[]>([]);
  const [stats, setStats] = useState<RepairStats>({
    totalRepairs: 0,
    successRate: 100.0,
    avgAttempts: 0.0,
    openTickets: 0,
    resolvedTickets: 0
  });
  const [selectedTicket, setSelectedTicket] = useState<RepairTicket | null>(null);

  const fetchRepairsData = async () => {
    setLoading(true);
    try {
      const apiBase = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080";

      const listRes = await fetch(`${apiBase}/api/repairs`);
      if (listRes.ok) {
        const data = await listRes.json();
        setRepairs(data);
        if (data.length > 0 && !selectedTicket) {
          setSelectedTicket(data[0]);
        }
      }

      const statsRes = await fetch(`${apiBase}/api/repairs/stats`);
      if (statsRes.ok) {
        const data = await statsRes.json();
        setStats(data);
      }
    } catch (err) {
      console.warn("API offline, falling back to mock repair agent telemetry.", err);
      // Premium Mock Fallback
      const mockTickets: RepairTicket[] = [
        {
          id: "rep-101",
          projectId: "proj-1",
          projectName: "Acme Logistics Portal",
          buildId: "build-14",
          source: "Compiler",
          severity: "Critical",
          status: "Resolved",
          failureType: "CS0103",
          errorMessage: "The name 'customer' does not exist in the current context",
          filePath: "src/Domain/Entities/Customer.cs",
          lineNumber: 42,
          attempts: 1,
          createdAt: new Date(Date.now() - 300000).toISOString(),
          resolvedAt: new Date(Date.now() - 250000).toISOString()
        },
        {
          id: "rep-102",
          projectId: "proj-3",
          projectName: "Employee HR Platform",
          buildId: "build-7",
          source: "Playwright",
          severity: "Major",
          status: "Resolved",
          failureType: "ElementNotFound",
          errorMessage: "Assertion failed: await page.locator('button[type=submit]').toBeVisible() timeout 5000ms",
          filePath: "tests/auth.spec.ts",
          lineNumber: 19,
          attempts: 2,
          createdAt: new Date(Date.now() - 7200000).toISOString(),
          resolvedAt: new Date(Date.now() - 7100000).toISOString()
        },
        {
          id: "rep-103",
          projectId: "proj-2",
          projectName: "SaaS CRM Integration",
          buildId: "build-3",
          source: "Compiler",
          severity: "Blocker",
          status: "Failed",
          failureType: "CS0246",
          errorMessage: "The type or namespace name 'IOrderRepository' could not be found (are you missing a using directive or an assembly reference?)",
          filePath: "src/Application/Orders/CreateOrderCommand.cs",
          lineNumber: 5,
          attempts: 3,
          createdAt: new Date(Date.now() - 86400000).toISOString(),
          resolvedAt: null
        }
      ];

      setRepairs(mockTickets);
      setSelectedTicket(mockTickets[0]);
      setStats({
        totalRepairs: 3,
        successRate: 66.7,
        avgAttempts: 2.0,
        openTickets: 0,
        resolvedTickets: 2
      });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchRepairsData();
  }, []);

  const getStatusBadge = (status: string) => {
    switch (status) {
      case "Resolved":
        return <span className="px-2.5 py-1 text-xs font-semibold text-emerald-400 bg-emerald-950/40 border border-emerald-800/40 rounded-full flex items-center gap-1 w-fit">Resolved</span>;
      case "Failed":
        return <span className="px-2.5 py-1 text-xs font-semibold text-rose-400 bg-rose-950/40 border border-rose-800/40 rounded-full flex items-center gap-1 w-fit">Failed</span>;
      case "Open":
        return <span className="px-2.5 py-1 text-xs font-semibold text-zinc-400 bg-zinc-900 border border-zinc-700/50 rounded-full flex items-center gap-1 w-fit">Open</span>;
      case "Analyzing":
      case "Repairing":
      case "Validating":
        return <span className="px-2.5 py-1 text-xs font-semibold text-amber-400 bg-amber-950/40 border border-amber-800/40 rounded-full animate-pulse flex items-center gap-1 w-fit">{status}...</span>;
      default:
        return <span className="px-2.5 py-1 text-xs font-semibold text-zinc-400 bg-zinc-900 border border-zinc-700/50 rounded-full w-fit">{status}</span>;
    }
  };

  const getSourceIcon = (source: string) => {
    switch (source) {
      case "Compiler":
        return <Terminal className="w-4 h-4 text-rose-400" />;
      case "Playwright":
        return <Activity className="w-4 h-4 text-emerald-400" />;
      default:
        return <FileCode className="w-4 h-4 text-indigo-400" />;
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
          <h1 className="text-3xl font-extrabold tracking-tight bg-gradient-to-r from-indigo-400 via-purple-400 to-pink-400 bg-clip-text text-transparent flex items-center gap-2">
            <Sparkles className="w-8 h-8 text-indigo-400 animate-pulse" /> AI Repair Agent Dashboard
          </h1>
          <p className="text-zinc-400 text-sm mt-1">Autonomous Self-Healing Loop & Codebase Patch Pipeline</p>
        </div>
        <div>
          <button 
            onClick={fetchRepairsData} 
            className="flex items-center gap-2 px-4 py-2 bg-zinc-900 border border-zinc-800 hover:border-zinc-700 rounded-lg text-sm font-semibold text-zinc-200 transition-all"
            disabled={loading}
          >
            <RefreshCw className={`w-4 h-4 ${loading ? "animate-spin" : ""}`} /> Refresh Engine
          </button>
        </div>
      </header>

      {/* Stats Section */}
      <section className="grid grid-cols-1 md:grid-cols-5 gap-6 mb-12">
        <div className="bg-[#0b0c14] border border-zinc-800/60 p-6 rounded-xl flex items-center justify-between shadow-xl">
          <div>
            <span className="text-xs text-zinc-400 block uppercase tracking-wider font-semibold">Total Repairs</span>
            <span className="text-3xl font-bold mt-1 block text-indigo-400">{stats.totalRepairs}</span>
          </div>
          <Activity className="w-8 h-8 text-indigo-400" />
        </div>

        <div className="bg-[#0b0c14] border border-zinc-800/60 p-6 rounded-xl flex items-center justify-between shadow-xl">
          <div>
            <span className="text-xs text-zinc-400 block uppercase tracking-wider font-semibold">Healing Success</span>
            <span className="text-3xl font-bold mt-1 block text-emerald-400">{stats.successRate}%</span>
          </div>
          <ShieldCheck className="w-8 h-8 text-emerald-400" />
        </div>

        <div className="bg-[#0b0c14] border border-zinc-800/60 p-6 rounded-xl flex items-center justify-between shadow-xl">
          <div>
            <span className="text-xs text-zinc-400 block uppercase tracking-wider font-semibold">Avg Attempts</span>
            <span className="text-3xl font-bold mt-1 block text-purple-400">{stats.avgAttempts}</span>
          </div>
          <RefreshCw className="w-8 h-8 text-purple-400" />
        </div>

        <div className="bg-[#0b0c14] border border-zinc-800/60 p-6 rounded-xl flex items-center justify-between shadow-xl">
          <div>
            <span className="text-xs text-zinc-400 block uppercase tracking-wider font-semibold">Open Tickets</span>
            <span className="text-3xl font-bold mt-1 block text-amber-400">{stats.openTickets}</span>
          </div>
          <AlertTriangle className="w-8 h-8 text-amber-400" />
        </div>

        <div className="bg-[#0b0c14] border border-zinc-800/60 p-6 rounded-xl flex items-center justify-between shadow-xl">
          <div>
            <span className="text-xs text-zinc-400 block uppercase tracking-wider font-semibold">Resolved</span>
            <span className="text-3xl font-bold mt-1 block text-emerald-500">{stats.resolvedTickets}</span>
          </div>
          <CheckCircle2 className="w-8 h-8 text-emerald-500" />
        </div>
      </section>

      {/* Main Workspace split panel */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Left/Middle: Ticket History list */}
        <main className="lg:col-span-2 space-y-6">
          <div className="bg-[#0b0c14]/40 border border-zinc-800/80 rounded-xl p-6 shadow-xl">
            <h2 className="text-xl font-bold flex items-center gap-2 mb-6">
              <GitBranch className="w-5 h-5 text-indigo-400" /> Healing Run Audit Log
            </h2>

            {repairs.length === 0 ? (
              <div className="text-center py-12 text-zinc-500">
                No repair tickets found. The AI Repair Agent is monitoring for pipeline defects.
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-left border-collapse">
                  <thead>
                    <tr className="border-b border-zinc-800 text-xs uppercase tracking-wider text-zinc-400 font-semibold">
                      <th className="py-4 px-4">Project / Build</th>
                      <th className="py-4 px-4">Source</th>
                      <th className="py-4 px-4">Defect Type</th>
                      <th className="py-4 px-4">Attempts</th>
                      <th className="py-4 px-4">Status</th>
                      <th className="py-4 px-4"></th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-zinc-900 text-sm">
                    {repairs.map((r) => (
                      <tr 
                        key={r.id} 
                        className={`hover:bg-zinc-900/30 transition-colors cursor-pointer ${selectedTicket?.id === r.id ? "bg-indigo-950/20" : ""}`}
                        onClick={() => setSelectedTicket(r)}
                      >
                        <td className="py-4 px-4">
                          <div className="font-bold text-zinc-200">{r.projectName}</div>
                          <div className="text-xs text-zinc-500 font-mono mt-0.5">{r.buildId.split("-")[0]}</div>
                        </td>
                        <td className="py-4 px-4">
                          <div className="flex items-center gap-1.5 text-zinc-300">
                            {getSourceIcon(r.source)}
                            <span>{r.source}</span>
                          </div>
                        </td>
                        <td className="py-4 px-4">
                          <div className="font-semibold text-zinc-300">{r.failureType}</div>
                          <div className="text-xs text-zinc-500 truncate max-w-[200px] mt-0.5">{r.errorMessage}</div>
                        </td>
                        <td className="py-4 px-4 font-mono text-zinc-400">{r.attempts}/3</td>
                        <td className="py-4 px-4">{getStatusBadge(r.status)}</td>
                        <td className="py-4 px-4 text-zinc-500">
                          <ChevronRight className="w-5 h-5" />
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </main>

        {/* Right side details drawer/card */}
        <aside className="space-y-6">
          {selectedTicket ? (
            <div className="bg-[#0b0c14]/40 border border-zinc-800/80 rounded-xl p-6 shadow-xl space-y-6">
              <div className="flex justify-between items-start gap-4 border-b border-zinc-800/80 pb-4">
                <div>
                  <h3 className="text-lg font-bold text-zinc-200">Ticket Details</h3>
                  <span className="text-xs font-mono text-zinc-500">ID: {selectedTicket.id}</span>
                </div>
                {getStatusBadge(selectedTicket.status)}
              </div>

              <div className="space-y-4">
                <div>
                  <span className="text-xs text-zinc-500 block uppercase font-semibold">Defect Description</span>
                  <p className="text-sm text-zinc-300 mt-1 bg-[#050508] p-3 border border-zinc-900 rounded font-mono break-words">
                    {selectedTicket.errorMessage}
                  </p>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <span className="text-xs text-zinc-500 block uppercase font-semibold">Failure Source</span>
                    <span className="text-sm font-semibold mt-1 block flex items-center gap-1.5 text-zinc-300">
                      {getSourceIcon(selectedTicket.source)} {selectedTicket.source}
                    </span>
                  </div>
                  <div>
                    <span className="text-xs text-zinc-500 block uppercase font-semibold">Failure Code</span>
                    <span className="text-sm font-mono mt-1 block text-zinc-300">{selectedTicket.failureType}</span>
                  </div>
                </div>

                {selectedTicket.filePath && (
                  <div>
                    <span className="text-xs text-zinc-500 block uppercase font-semibold">Failing File</span>
                    <div className="text-xs font-mono text-indigo-400 mt-1 bg-indigo-950/20 border border-indigo-900/40 p-2.5 rounded flex items-center gap-1.5">
                      <FileCode className="w-3.5 h-3.5" />
                      <span className="break-all">{selectedTicket.filePath}:{selectedTicket.lineNumber}</span>
                    </div>
                  </div>
                )}

                <div className="border-t border-zinc-900 pt-4 space-y-3">
                  <div className="flex items-center gap-2 text-xs text-zinc-400">
                    <GitBranch className="w-3.5 h-3.5 text-zinc-500" />
                    <span>Repair Branch:</span>
                    <span className="font-mono text-zinc-200 bg-zinc-900 px-1.5 py-0.5 rounded">
                      repair/fix-{selectedTicket.id.split("-")[0]}
                    </span>
                  </div>
                  <div className="flex items-center gap-2 text-xs text-zinc-400">
                    <Clock className="w-3.5 h-3.5 text-zinc-500" />
                    <span>Created:</span>
                    <span className="text-zinc-300">
                      {new Date(selectedTicket.createdAt).toLocaleTimeString()}
                    </span>
                  </div>
                  {selectedTicket.resolvedAt && (
                    <div className="flex items-center gap-2 text-xs text-zinc-400">
                      <Clock className="w-3.5 h-3.5 text-zinc-500" />
                      <span>Resolved:</span>
                      <span className="text-zinc-300">
                        {new Date(selectedTicket.resolvedAt).toLocaleTimeString()}
                      </span>
                    </div>
                  )}
                </div>

                {selectedTicket.status === "Resolved" && (
                  <div className="bg-emerald-950/20 border border-emerald-800/40 p-4 rounded-lg">
                    <div className="flex items-center gap-2 text-emerald-400 font-semibold text-sm">
                      <CheckCircle2 className="w-4 h-4" /> AI Healing Successful
                    </div>
                    <p className="text-xs text-zinc-400 mt-1">
                      Gemini applied the patch successfully, all compiler check and E2E regression gates passed, and branch was merged.
                    </p>
                  </div>
                )}
              </div>
            </div>
          ) : (
            <div className="bg-[#0b0c14]/40 border border-zinc-800/80 rounded-xl p-6 shadow-xl text-center text-zinc-500 py-12">
              Select a healing log ticket to view repair audit details.
            </div>
          )}
        </aside>
      </div>
    </div>
  );
}
