import { FormEvent, useEffect, useMemo, useState } from "react";
import {
  createProject,
  createTask,
  deleteProject,
  deleteTask,
  getProjects,
  getSummary,
  getTasks,
  login,
  register,
  updateProject,
  updateTask
} from "./api";
import type {
  DashboardSummary,
  Project,
  ProjectStatus,
  TaskItem,
  TaskPriority,
  TaskState
} from "./api";
import StatCard from "./components/StatCard";

const initialState: DashboardSummary = {
  projects: 0,
  tasks: 0,
  completedTasks: 0,
  inProgressTasks: 0,
  upcomingTasks: 0
};

const tokenKey = "interview_showcase_token";
const emailKey = "interview_showcase_email";

const projectStatusOptions: Array<{ label: string; value: ProjectStatus }> = [
  { label: "Not Started", value: 0 },
  { label: "In Progress", value: 1 },
  { label: "Done", value: 2 }
];

const taskPriorityOptions: Array<{ label: string; value: TaskPriority }> = [
  { label: "Low", value: 0 },
  { label: "Medium", value: 1 },
  { label: "High", value: 2 }
];

const taskStateOptions: Array<{ label: string; value: TaskState }> = [
  { label: "Not Started", value: 0 },
  { label: "In Progress", value: 1 },
  { label: "Done", value: 2 }
];

const today = new Date().toISOString().slice(0, 10);

function readErrorMessage(error: unknown, fallback: string): string {
  if (error instanceof Error && error.message.trim().length > 0) {
    return error.message;
  }

  return fallback;
}

export default function App() {
  const [summary, setSummary] = useState<DashboardSummary>(initialState);
  const [projects, setProjects] = useState<Project[]>([]);
  const [tasks, setTasks] = useState<TaskItem[]>([]);

  const [token, setToken] = useState<string | null>(() => localStorage.getItem(tokenKey));
  const [email, setEmail] = useState<string>(() => localStorage.getItem(emailKey) ?? "demo@interview.dev");
  const [password, setPassword] = useState<string>("Demo123!");

  const [projectForm, setProjectForm] = useState({ name: "", description: "", status: 1 as ProjectStatus });
  const [taskForm, setTaskForm] = useState({ title: "", priority: 1 as TaskPriority, state: 0 as TaskState, dueDate: today, projectId: "" });

  const [editingProjectId, setEditingProjectId] = useState<string | null>(null);
  const [editingTaskId, setEditingTaskId] = useState<string | null>(null);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const projectLookup = useMemo(() => {
    const map = new Map<string, Project>();
    for (const project of projects) {
      map.set(project.id, project);
    }
    return map;
  }, [projects]);

  useEffect(() => {
    if (!token) {
      setSummary(initialState);
      setProjects([]);
      setTasks([]);
      return;
    }

    loadAppData(token);
  }, [token]);

  async function loadAppData(accessToken: string) {
    setLoading(true);
    try {
      const [summaryData, projectData, taskData] = await Promise.all([
        getSummary(accessToken),
        getProjects(accessToken),
        getTasks(accessToken)
      ]);

      setSummary(summaryData);
      setProjects(projectData);
      setTasks(taskData);
      setTaskForm((prev) => ({ ...prev, projectId: prev.projectId || projectData[0]?.id || "" }));
      setError(null);
    } catch {
      setError("Session expired or API unavailable. Please sign in again.");
      localStorage.removeItem(tokenKey);
      setToken(null);
    } finally {
      setLoading(false);
    }
  }

  async function handleLogin(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const response = await login(email, password);
      localStorage.setItem(tokenKey, response.accessToken);
      localStorage.setItem(emailKey, response.email);
      setToken(response.accessToken);
    } catch (error: unknown) {
      setError(readErrorMessage(error, "Login failed. Use demo@interview.dev / Demo123! or register a new account."));
    } finally {
      setLoading(false);
    }
  }

  async function handleRegister() {
    setLoading(true);
    setError(null);

    try {
      const response = await register(email, password);
      localStorage.setItem(tokenKey, response.accessToken);
      localStorage.setItem(emailKey, response.email);
      setToken(response.accessToken);
    } catch (error: unknown) {
      setError(readErrorMessage(error, "Registration failed. Ensure email format is valid and password is at least 8 characters."));
    } finally {
      setLoading(false);
    }
  }

  function handleLogout() {
    localStorage.removeItem(tokenKey);
    setToken(null);
    setSummary(initialState);
    setProjects([]);
    setTasks([]);
  }

  async function handleProjectSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!token) {
      return;
    }

    setLoading(true);
    setError(null);

    try {
      if (editingProjectId) {
        await updateProject(token, editingProjectId, projectForm);
      } else {
        await createProject(token, projectForm);
      }

      setProjectForm({ name: "", description: "", status: 1 });
      setEditingProjectId(null);
      await loadAppData(token);
    } catch (error: unknown) {
      setError(readErrorMessage(error, "Could not save project. Check values and try again."));
      setLoading(false);
    }
  }

  function startEditProject(project: Project) {
    setEditingProjectId(project.id);
    setProjectForm({
      name: project.name,
      description: project.description,
      status: project.status
    });
  }

  async function handleDeleteProject(projectId: string) {
    if (!token) {
      return;
    }

    setLoading(true);
    setError(null);
    try {
      await deleteProject(token, projectId);
      if (editingProjectId === projectId) {
        setEditingProjectId(null);
        setProjectForm({ name: "", description: "", status: 1 });
      }
      await loadAppData(token);
    } catch (error: unknown) {
      setError(readErrorMessage(error, "Could not delete project. Delete related tasks first if needed."));
      setLoading(false);
    }
  }

  async function handleTaskSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!token) {
      return;
    }

    setLoading(true);
    setError(null);

    try {
      if (editingTaskId) {
        await updateTask(token, editingTaskId, taskForm);
      } else {
        await createTask(token, taskForm);
      }

      setTaskForm({ title: "", priority: 1, state: 0, dueDate: today, projectId: projects[0]?.id ?? "" });
      setEditingTaskId(null);
      await loadAppData(token);
    } catch (error: unknown) {
      setError(readErrorMessage(error, "Could not save task. Ensure a valid project is selected."));
      setLoading(false);
    }
  }

  function startEditTask(task: TaskItem) {
    setEditingTaskId(task.id);
    setTaskForm({
      title: task.title,
      priority: task.priority,
      state: task.state,
      dueDate: task.dueDate,
      projectId: task.projectId
    });
  }

  async function handleDeleteTask(taskId: string) {
    if (!token) {
      return;
    }

    setLoading(true);
    setError(null);
    try {
      await deleteTask(token, taskId);
      if (editingTaskId === taskId) {
        setEditingTaskId(null);
        setTaskForm({ title: "", priority: 1, state: 0, dueDate: today, projectId: projects[0]?.id ?? "" });
      }
      await loadAppData(token);
    } catch (error: unknown) {
      setError(readErrorMessage(error, "Could not delete task."));
      setLoading(false);
    }
  }

  return (
    <main className="layout">
      <section className="hero">
        <p className="eyebrow">Full-Stack Interview Project</p>
        <h1>Team Task + Analytics Platform</h1>
        <p>Demonstrates ASP.NET Core + EF Core + JWT auth + React + Docker + AWS IaC.</p>
      </section>

      {!token ? (
        <section className="auth-card">
          <h2>Authenticate</h2>
          <p>Demo account: demo@interview.dev / Demo123!</p>
          <form onSubmit={handleLogin} className="auth-form">
            <label>
              Email
              <input value={email} onChange={(e) => setEmail(e.target.value)} type="email" required />
            </label>
            <label>
              Password
              <input value={password} onChange={(e) => setPassword(e.target.value)} type="password" required />
            </label>
            <div className="auth-actions">
              <button type="submit" disabled={loading}>Sign In</button>
              <button type="button" disabled={loading} onClick={handleRegister}>Register</button>
            </div>
          </form>
        </section>
      ) : (
        <section className="session-bar">
          <p>Signed in as {localStorage.getItem(emailKey)}</p>
          <button onClick={handleLogout}>Log out</button>
        </section>
      )}

      {loading ? <p className="status">Loading...</p> : null}
      {error ? <p className="status error">{error}</p> : null}

      <section className="stats-grid">
        <StatCard label="Projects" value={summary.projects} />
        <StatCard label="Tasks" value={summary.tasks} />
        <StatCard label="Completed" value={summary.completedTasks} />
        <StatCard label="In Progress" value={summary.inProgressTasks} />
        <StatCard label="Upcoming" value={summary.upcomingTasks} />
      </section>

      {token ? (
        <section className="manager-grid">
          <article className="manager-card">
            <h2>{editingProjectId ? "Edit Project" : "Create Project"}</h2>
            <form className="manager-form" onSubmit={handleProjectSubmit}>
              <label>
                Name
                <input
                  value={projectForm.name}
                  onChange={(e) => setProjectForm((prev) => ({ ...prev, name: e.target.value }))}
                  minLength={3}
                  maxLength={100}
                  required
                />
              </label>
              <label>
                Description
                <textarea
                  value={projectForm.description}
                  onChange={(e) => setProjectForm((prev) => ({ ...prev, description: e.target.value }))}
                  minLength={10}
                  maxLength={300}
                  required
                />
              </label>
              <label>
                Status
                <select
                  value={projectForm.status}
                  onChange={(e) => setProjectForm((prev) => ({ ...prev, status: Number(e.target.value) as ProjectStatus }))}
                >
                  {projectStatusOptions.map((option) => (
                    <option key={option.value} value={option.value}>{option.label}</option>
                  ))}
                </select>
              </label>
              <div className="auth-actions">
                <button type="submit" disabled={loading}>{editingProjectId ? "Update" : "Create"}</button>
                {editingProjectId ? <button type="button" onClick={() => { setEditingProjectId(null); setProjectForm({ name: "", description: "", status: 1 }); }}>Cancel</button> : null}
              </div>
            </form>
            <div className="list-wrap">
              {projects.map((project) => (
                <div key={project.id} className="list-row">
                  <div>
                    <strong>{project.name}</strong>
                    <p>{project.description}</p>
                  </div>
                  <div className="row-actions">
                    <button type="button" onClick={() => startEditProject(project)}>Edit</button>
                    <button type="button" onClick={() => handleDeleteProject(project.id)}>Delete</button>
                  </div>
                </div>
              ))}
              {!projects.length ? <p className="muted">No projects yet.</p> : null}
            </div>
          </article>

          <article className="manager-card">
            <h2>{editingTaskId ? "Edit Task" : "Create Task"}</h2>
            <form className="manager-form" onSubmit={handleTaskSubmit}>
              <label>
                Title
                <input
                  value={taskForm.title}
                  onChange={(e) => setTaskForm((prev) => ({ ...prev, title: e.target.value }))}
                  minLength={3}
                  maxLength={120}
                  required
                />
              </label>
              <label>
                Project
                <select
                  value={taskForm.projectId}
                  onChange={(e) => setTaskForm((prev) => ({ ...prev, projectId: e.target.value }))}
                  required
                >
                  {!projects.length ? <option value="">Create a project first</option> : null}
                  {projects.map((project) => (
                    <option key={project.id} value={project.id}>{project.name}</option>
                  ))}
                </select>
              </label>
              <label>
                Priority
                <select
                  value={taskForm.priority}
                  onChange={(e) => setTaskForm((prev) => ({ ...prev, priority: Number(e.target.value) as TaskPriority }))}
                >
                  {taskPriorityOptions.map((option) => (
                    <option key={option.value} value={option.value}>{option.label}</option>
                  ))}
                </select>
              </label>
              <label>
                State
                <select
                  value={taskForm.state}
                  onChange={(e) => setTaskForm((prev) => ({ ...prev, state: Number(e.target.value) as TaskState }))}
                >
                  {taskStateOptions.map((option) => (
                    <option key={option.value} value={option.value}>{option.label}</option>
                  ))}
                </select>
              </label>
              <label>
                Due date
                <input
                  type="date"
                  value={taskForm.dueDate}
                  onChange={(e) => setTaskForm((prev) => ({ ...prev, dueDate: e.target.value }))}
                  required
                />
              </label>
              <div className="auth-actions">
                <button type="submit" disabled={loading || !projects.length}>{editingTaskId ? "Update" : "Create"}</button>
                {editingTaskId ? <button type="button" onClick={() => { setEditingTaskId(null); setTaskForm({ title: "", priority: 1, state: 0, dueDate: today, projectId: projects[0]?.id ?? "" }); }}>Cancel</button> : null}
              </div>
            </form>
            <div className="list-wrap">
              {tasks.map((task) => (
                <div key={task.id} className="list-row">
                  <div>
                    <strong>{task.title}</strong>
                    <p>
                      {projectLookup.get(task.projectId)?.name ?? "Unknown Project"} | Due {task.dueDate}
                    </p>
                  </div>
                  <div className="row-actions">
                    <button type="button" onClick={() => startEditTask(task)}>Edit</button>
                    <button type="button" onClick={() => handleDeleteTask(task.id)}>Delete</button>
                  </div>
                </div>
              ))}
              {!tasks.length ? <p className="muted">No tasks yet.</p> : null}
            </div>
          </article>
        </section>
      ) : null}
    </main>
  );
}
