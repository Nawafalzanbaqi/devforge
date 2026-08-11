"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { ArrowLeft, Check, Database, Zap, BookOpen, Layers } from "lucide-react";

export default function CreateProject() {
  const router = useRouter();
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [template, setTemplate] = useState("crm");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;

    // In a real app we hit POST /api/projects, get a Project ID, and redirect
    const dummyProjectId = "proj-4";
    router.push(`/projects/${dummyProjectId}/schema`);
  };

  const templates = [
    {
      id: "crm",
      name: "SaaS CRM Starter",
      description: "Includes customers, pipelines, accounts, authorization layers, and transaction logs.",
      icon: Zap
    },
    {
      id: "helpdesk",
      name: "IT Helpdesk & Tickets",
      description: "Includes ticketing database schemas, comments, priority queues, and customer fields.",
      icon: BookOpen
    },
    {
      id: "empty",
      name: "Empty Visual Canvas",
      description: "Start from a blank canvas. Drop tables, relationships, and configure validation fields.",
      icon: Layers
    }
  ];

  return (
    <div className="min-h-screen bg-[#050508] text-zinc-100 font-sans p-6 md:p-12 flex items-center justify-center">
      <div className="w-full max-w-2xl bg-[#0b0c14]/50 border border-zinc-800/80 p-8 rounded-2xl shadow-2xl relative">
        {/* Back Link */}
        <Link href="/" className="flex items-center gap-2 text-zinc-400 hover:text-zinc-100 transition-colors text-sm mb-6 absolute top-6 left-8">
          <ArrowLeft className="w-4 h-4" /> Dashboard
        </Link>

        <div className="mt-8 mb-8 text-center">
          <h1 className="text-2xl font-extrabold tracking-tight bg-gradient-to-r from-indigo-400 to-purple-400 bg-clip-text text-transparent">
            Manufacture New Platform
          </h1>
          <p className="text-zinc-400 text-sm mt-1">Specify configurations to initialize the code generation pipeline</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-6">
          <div className="space-y-2">
            <label className="text-sm font-semibold text-zinc-300 block">Project Name</label>
            <input
              type="text"
              required
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="e.g. Acme Inventory Manager"
              className="w-full bg-[#050508] border border-zinc-800 focus:border-indigo-500 rounded-lg px-4 py-3 text-zinc-100 placeholder-zinc-600 outline-none transition-colors"
            />
          </div>

          <div className="space-y-2">
            <label className="text-sm font-semibold text-zinc-300 block">Description (Optional)</label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Describe the application features and capabilities..."
              rows={3}
              className="w-full bg-[#050508] border border-zinc-800 focus:border-indigo-500 rounded-lg px-4 py-3 text-zinc-100 placeholder-zinc-600 outline-none transition-colors resize-none"
            />
          </div>

          {/* Select Template */}
          <div className="space-y-3">
            <label className="text-sm font-semibold text-zinc-300 block">Select Base Schema Template</label>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              {templates.map((t) => {
                const Icon = t.icon;
                return (
                  <button
                    key={t.id}
                    type="button"
                    onClick={() => setTemplate(t.id)}
                    className={`p-4 border rounded-xl flex flex-col justify-between text-start transition-all relative ${
                      template === t.id
                        ? "border-indigo-500 bg-indigo-950/20"
                        : "border-zinc-800/80 bg-[#050508] hover:border-zinc-700/80"
                    }`}
                  >
                    {template === t.id && (
                      <span className="absolute top-3 right-3 p-0.5 bg-indigo-500 text-white rounded-full">
                        <Check className="w-3.5 h-3.5" />
                      </span>
                    )}
                    <Icon className={`w-6 h-6 mb-4 ${template === t.id ? "text-indigo-400" : "text-zinc-500"}`} />
                    <div>
                      <h3 className="font-bold text-sm mb-1">{t.name}</h3>
                      <p className="text-zinc-500 text-xs line-clamp-3">{t.description}</p>
                    </div>
                  </button>
                );
              })}
            </div>
          </div>

          {/* Submit */}
          <div className="border-t border-zinc-900 pt-6 mt-6 flex justify-end">
            <button type="submit" className="flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-500 hover:to-purple-500 font-semibold rounded-lg shadow-lg transition-all text-sm">
              Create & Edit Schema
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
