// Frontend mirrors of the backend tracking/publishing contracts.

import { BoardCard } from '../board/board.models';

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

// Mirror of OnboardingBoardDto — one hire's full board as HR/Manager sees it.
export interface HireBoard {
  id: number;
  hireUserId: number;
  assignedAt: string;
  cards: BoardCard[];
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
