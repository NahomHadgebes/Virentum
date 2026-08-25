/** POST /api/auth/login — Controllers/AuthController.cs */
import type { LoginRequest, LoginResponse } from '../types/contracts';
import { post } from './client';

export function login(request: LoginRequest): Promise<LoginResponse> {
  return post<LoginResponse>({
    path: '/api/auth/login',
    body: request,
    authenticated: false,
  });
}
