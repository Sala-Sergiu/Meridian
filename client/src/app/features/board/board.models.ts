// Frontend mirrors of the backend board contract (PagedResult<BoardCardDto>),
// matched to the actual Swagger shapes. Note: the card's category field is
// serialized as `type` (from BoardCardDto.Type) — not `cardType`.

export type CardStatus = 'ToDo' | 'InProgress' | 'Done';

export type CardType = 'Resource' | 'Safety' | 'Contact';

export interface BoardCard {
  id: number;
  title: string;
  description: string;
  type: CardType;
  url: string | null;
  order: number;
  status: CardStatus;
}

// TotalCount reflects the filtered set before paging (backend guarantee).
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
