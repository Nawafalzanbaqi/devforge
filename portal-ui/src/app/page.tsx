"use client";

import { useState } from "react";
import Link from "next/link";
import { Plus, Database, Terminal, Settings, CreditCard, Play, CheckCircle2, AlertTriangle, Activity, Cpu } from "lucide-react";

interface Project {
  id: string;
  name: string;
  description: string;
  status: "Draft" | "Generating" | "Compiling" | "Testing" | "Completed" | "Failed";
  updatedAt: string;
  buildsCount: number;
}

export default function Dashboard() {
  const [projects] = useState<Project[]>([
    {
      id: "proj-1",
      name: "Acme Logistics Portal",
      description: "Interconnected billing, fleet management, and inventory tracking system.",
      status: "Completed",
      updatedAt: "2 hours ago",
      buildsCount: 14
    },
    {
      id: "proj-2",
      name: "SaaS CRM Integration",
      description: "Customer pipeline, authentication single-sign-on (SSO), and custom reports.",
      status: "Generating",
      updatedAt: "Just now",
      buildsCount: 3
    },
    {
      id: "proj-3",
      name: "Employee HR Platform",
      description: "Employee records schema with automated Playwright CRUD tests.",
      status: "Failed",
      updatedAt: "1 day ago",
      buildsCount: 7
    }
  ]);

  const getStatusBadge = (status: Project["status"]) => {
    switch (status) {
      case "Completed":
        return <span className="flex items-center gap-1.5 px-2.5 py-1 text-xs font-semibold text-emerald-400 bg-emerald-950/40 border border-emerald-800/50 rounded-full"><CheckCircle2 className="w-3.5 h-3.5" /> Live</span>;
      case "Generating":
      case "Compiling":
      case "Testing":
        return <span className="flex items-center gap-1.5 px-2.5 py-1 text-xs font-semibold text-amber-400 bg-amber-950/40 border border-amber-800/50 rounded-full animate-pulse"><Activity className="w-3.5 h-3.5" /> {status}...</span>;
      case "Failed":
        return <span className="flex items-center gap-1.5 px-2.5 py-1 text-xs font-semibold text-rose-400 bg-rose-950/40 border border-rose-800/50 rounded-full"><AlertTriangle className="w-3.5 h-3.5" /> Failed</span>;
      default:
        return <span className="flex items-center gap-1.5 px-2.5 py-1 text-xs font-semibold text-zinc-400 bg-zinc-900 border border-zinc-700/50 rounded-full">Draft</span>;
    }
  };

  return (
    <div className="min-h-screen bg-[#050508] text-zinc-100 font-sans p-6 md:p-12 selection:bg-indigo-500 selection:text-white">
      {/* Header */}
      <header className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 border-b border-zinc-800/80 pb-8 mb-12">
        <div>
          <h1 className="text-3xl font-extrabold tracking-tight bg-gradient-to-r from-indigo-400 via-purple-400 to-pink-400 bg-clip-text text-transparent">
            DevForge Dashboard
          </h1>
          <p className="text-zinc-400 text-sm mt-1">Autonomous Software Manufacturing Platform</p>
        </div>
        <div className="flex flex-wrap gap-4">
          <Link href="/quality" className="flex items-center gap-2 px-4 py-2.5 bg-zinc-900 border border-zinc-800 hover:border-zinc-700 transition-all font-semibold rounded-lg text-sm text-zinc-200">
            QA Gate
          </Link>
          <Link href="/deployments" className="flex items-center gap-2 px-4 py-2.5 bg-zinc-900 border border-zinc-800 hover:border-zinc-700 transition-all font-semibold rounded-lg text-sm text-zinc-200">
            Deployments
          </Link>
          <Link href="/repairs" className="flex items-center gap-2 px-4 py-2.5 bg-zinc-900 border border-zinc-800 hover:border-zinc-700 transition-all font-semibold rounded-lg text-sm text-zinc-200">
            Self-Healing
          </Link>
          <Link href="/projects/create" className="flex items-center gap-2 px-4 py-2.5 bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-500 hover:to-purple-500 transition-all font-semibold rounded-lg shadow-lg shadow-indigo-950/50 text-sm">
            <Plus className="w-4 h-4" /> Manufacture App
          </Link>
        </div>
      </header>

      {/* Main Stats Grid */}
      <section className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-12">
        <div className="bg-[#0b0c14] border border-zinc-800/60 p-6 rounded-xl flex items-center justify-between shadow-xl">
          <div>
            <span className="text-xs text-zinc-400 block uppercase tracking-wider font-semibold">Active Services</span>
            <span className="text-3xl font-bold mt-1 block">18</span>
          </div>
          <Cpu className="w-8 h-8 text-indigo-400" />
        </div>
        
        <div className="bg-[#0b0c14] border border-zinc-800/60 p-6 rounded-xl flex items-center justify-between shadow-xl">
          <div>
            <span className="text-xs text-zinc-400 block uppercase tracking-wider font-semibold">Build Success Rate</span>
            <span className="text-3xl font-bold mt-1 block text-emerald-400">92.4%</span>
          </div>
          <CheckCircle2 className="w-8 h-8 text-emerald-400" />
        </div>

        <div className="bg-[#0b0c14] border border-zinc-800/60 p-6 rounded-xl flex items-center justify-between shadow-xl">
          <div>
            <span className="text-xs text-zinc-400 block uppercase tracking-wider font-semibold">Gemini Tokens Spent</span>
            <span className="text-3xl font-bold mt-1 block">2.4M</span>
          </div>
          <Database className="w-8 h-8 text-purple-400" />
        </div>

        <div className="bg-[#0b0c14] border border-zinc-800/60 p-6 rounded-xl flex items-center justify-between shadow-xl">
          <div>
            <span className="text-xs text-zinc-400 block uppercase tracking-wider font-semibold">Est. Savings API</span>
            <span className="text-3xl font-bold mt-1 block text-indigo-400">$1,840.00</span>
          </div>
          <CreditCard className="w-8 h-8 text-indigo-400" />
        </div>
      </section>

      {/* Workspace Panel */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Left Column: Projects */}
        <main className="lg:col-span-2 space-y-6">
          <div className="flex items-center justify-between mb-2">
            <h2 className="text-xl font-bold flex items-center gap-2">
              <Database className="w-5 h-5 text-indigo-500" /> Manufactured Projects
            </h2>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {projects.map((p) => (
              <div key={p.id} className="group relative bg-[#0b0c14]/50 border border-zinc-800/80 hover:border-zinc-700/80 transition-all p-6 rounded-xl shadow-xl flex flex-col justify-between h-[220px]">
                <div>
                  <div className="flex items-start justify-between gap-4">
                    <h3 className="text-lg font-bold text-zinc-100 group-hover:text-indigo-400 transition-colors">
                      {p.name}
                    </h3>
                    {getStatusBadge(p.status)}
                  </div>
                  <p className="text-zinc-400 text-sm mt-3 line-clamp-2">
                    {p.description}
                  </p>
                </div>

                <div className="flex items-center justify-between border-t border-zinc-900 pt-4 mt-4">
                  <span className="text-xs text-zinc-500">Updated {p.updatedAt}</span>
                  <div className="flex gap-2">
                    <Link href={`/projects/${p.id}/schema`} className="px-3 py-1.5 text-xs bg-zinc-900 border border-zinc-800 hover:border-zinc-700 font-semibold rounded-md">
                      Edit Schema
                    </Link>
                    <Link href={`/projects/${p.id}/build`} className="flex items-center gap-1 px-3 py-1.5 text-xs bg-indigo-950/60 border border-indigo-800/50 hover:bg-indigo-900 text-indigo-300 font-semibold rounded-md">
                      <Play className="w-3.5 h-3.5 fill-current" /> Build Logs
                    </Link>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </main>

        {/* Right Column: Console Log / Operations Activities */}
        <aside className="space-y-6">
          <h2 className="text-xl font-bold flex items-center gap-2">
            <Terminal className="w-5 h-5 text-purple-500" /> Platform Operations Log
          </h2>
          
          <div className="bg-[#050508] border border-zinc-800/80 p-6 rounded-xl font-mono text-xs text-zinc-400 space-y-4 shadow-xl max-h-[460px] overflow-y-auto">
            <div className="border-l-2 border-indigo-500 pl-3">
              <span className="text-zinc-600">[11:42:01]</span> <span className="text-indigo-400">[Orchestrator]</span> Active monitoring loop mapping logistics subdomain rules...
            </div>
            <div className="border-l-2 border-emerald-500 pl-3">
              <span className="text-zinc-600">[11:39:15]</span> <span className="text-emerald-400">[QA Agent]</span> Playwright assertions complete. 12/12 screens passing.
            </div>
            <div className="border-l-2 border-purple-500 pl-3">
              <span className="text-zinc-600">[11:37:04]</span> <span className="text-purple-400">[AI Repair]</span> Resolved missing migration key in Order.cs. Redeploying build.
            </div>
            <div className="border-l-2 border-amber-500 pl-3">
              <span className="text-zinc-600">[11:36:20]</span> <span className="text-amber-400">[Compiler]</span> Warning C4996: Duplicate identity keys detected. Re-checking...
            </div>
            <div className="border-l-2 border-zinc-700 pl-3">
              <span className="text-zinc-600">[11:30:00]</span> <span className="text-zinc-500">[Nginx]</span> Proxy resolution re-routing to orderapp.local.devforge
            </div>
          </div>
        </aside>
      </div>
    </div>
  );
}
