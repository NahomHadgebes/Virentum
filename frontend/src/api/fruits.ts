/** GET /api/fruits — Controllers/FruitsController.cs */
import type { FruitProfileResponse } from '../types/contracts';
import { get } from './client';

export function getFruits(): Promise<FruitProfileResponse[]> {
  return get<FruitProfileResponse[]>('/api/fruits');
}
