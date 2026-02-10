export type DashboardSummary = {
  projects: number;
  tasks: number;
  completedTasks: number;
  inProgressTasks: number;
  upcomingTasks: number;
};

export type AuthResponse = {
  accessToken: string;
  expiresAtUtc: string;
  userId: string;
  email: string;
};

export type ProjectStatus = 0 | 1 | 2;
export type TaskPriority = 0 | 1 | 2;
export type TaskState = 0 | 1 | 2;

export type Project = {
  id: string;
  name: string;
  description: string;
  status: ProjectStatus;
  createdOnUtc: string;
};

export type TaskItem = {
  id: string;
  title: string;
  priority: TaskPriority;
  state: TaskState;
  dueDate: string;
  projectId: string;
  createdOnUtc: string;
};

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:8080";

function getErrorMessage(payload: string, status: number): string {
  if (!payload) {
    return `Request failed with status ${status}`;
  }

  try {
    const parsed = JSON.parse(payload) as {
      message?: string;
      title?: string;
      errors?: Record<string, string[]>;
    };

    if (parsed.message) {
      return parsed.message;
    }

    if (parsed.errors) {
      const firstError = Object.values(parsed.errors).flat()[0];
      if (firstError) {
        return firstError;
      }
    }

    if (parsed.title) {
      return parsed.title;
    }
  } catch {
    return payload;
  }

  return `Request failed with status ${status}`;
}

async function requestJson<T>(path: string, options: RequestInit = {}): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(options.headers ?? {})
    }
  });

  if (!response.ok) {
    const payload = await response.text();
    throw new Error(getErrorMessage(payload, response.status));
  }

  return response.json() as Promise<T>;
}

async function requestVoid(path: string, options: RequestInit = {}): Promise<void> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(options.headers ?? {})
    }
  });

  if (!response.ok) {
    const payload = await response.text();
    throw new Error(getErrorMessage(payload, response.status));
  }
}

function authHeader(token: string): HeadersInit {
  return {
    Authorization: `Bearer ${token}`
  };
}

export function register(email: string, password: string) {
  return requestJson<AuthResponse>("/api/auth/register", {
    method: "POST",
    body: JSON.stringify({ email, password })
  });
}

export function login(email: string, password: string) {
  return requestJson<AuthResponse>("/api/auth/login", {
    method: "POST",
    body: JSON.stringify({ email, password })
  });
}

export function getSummary(token: string): Promise<DashboardSummary> {
  return requestJson<DashboardSummary>("/api/dashboard/summary", {
    headers: authHeader(token)
  });
}

export function getProjects(token: string): Promise<Project[]> {
  return requestJson<Project[]>("/api/projects", {
    headers: authHeader(token)
  });
}

export function createProject(token: string, payload: { name: string; description: string; status: ProjectStatus }): Promise<Project> {
  return requestJson<Project>("/api/projects", {
    method: "POST",
    headers: authHeader(token),
    body: JSON.stringify(payload)
  });
}

export function updateProject(token: string, projectId: string, payload: { name: string; description: string; status: ProjectStatus }): Promise<Project> {
  return requestJson<Project>(`/api/projects/${projectId}`, {
    method: "PUT",
    headers: authHeader(token),
    body: JSON.stringify(payload)
  });
}

export function deleteProject(token: string, projectId: string): Promise<void> {
  return requestVoid(`/api/projects/${projectId}`, {
    method: "DELETE",
    headers: authHeader(token)
  });
}

export function getTasks(token: string): Promise<TaskItem[]> {
  return requestJson<TaskItem[]>("/api/tasks", {
    headers: authHeader(token)
  });
}

export function createTask(
  token: string,
  payload: { title: string; priority: TaskPriority; state: TaskState; dueDate: string; projectId: string }
): Promise<TaskItem> {
  return requestJson<TaskItem>("/api/tasks", {
    method: "POST",
    headers: authHeader(token),
    body: JSON.stringify(payload)
  });
}

export function updateTask(
  token: string,
  taskId: string,
  payload: { title: string; priority: TaskPriority; state: TaskState; dueDate: string; projectId: string }
): Promise<TaskItem> {
  return requestJson<TaskItem>(`/api/tasks/${taskId}`, {
    method: "PUT",
    headers: authHeader(token),
    body: JSON.stringify(payload)
  });
}

export function deleteTask(token: string, taskId: string): Promise<void> {
  return requestVoid(`/api/tasks/${taskId}`, {
    method: "DELETE",
    headers: authHeader(token)
  });
}
