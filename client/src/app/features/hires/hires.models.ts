// Frontend mirrors of the backend tracking/publishing contracts.

export interface HireProgress {
  hireUserId: number;
  displayName: string;
  email: string;
  hasBoard: boolean;
  tasksDone: number;
  tasksTotal: number;
  readDone: number;
  readTotal: number;
}

export interface PublishCardRequest {
  title: string;
  description: string;
  type: 'Safety' | 'Resource';
  url: string | null;
}

export interface PublishCardResult {
  card: { id: number; title: string };
  boardsUpdated: number;
}
