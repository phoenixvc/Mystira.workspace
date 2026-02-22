// User types that mirror backend API contracts

export type UserRole = 'author' | 'illustrator' | 'publisher' | 'admin' | 'legal';

export interface User {
  id: string;
  name: string;
  email: string;
  roles: UserRole[];
  avatarUrl?: string;
  createdAt: string;
}

export interface AuthUser extends User {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  user: User;
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface UserSearchParams {
  query: string;
  limit?: number;
}
