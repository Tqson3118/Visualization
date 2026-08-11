




import { API_BASE_URL } from '@/services/apiConfig';

const API_BASE = API_BASE_URL;

export interface OrderDto {
  id: string;
  userId: string;
  paymentCode: string;
  amount: number;
  status: string; 
  createdAt: string;
  completedAt: string | null;
  bankId: string;
  bankAccount: string;
  accountName: string;
  qrUrl: string;
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const body = await response.json().catch(() => null);
    throw new Error(body?.message ?? `HTTP ${response.status}: ${response.statusText}`);
  }
  return response.json() as Promise<T>;
}


export async function createOrder(accessToken: string): Promise<OrderDto> {
  const res = await fetch(`${API_BASE}/api/v1/payments/order`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${accessToken}`,
      'Content-Type': 'application/json'
    }
  });
  return handleResponse<OrderDto>(res);
}


export async function getOrderStatus(orderId: string, accessToken: string): Promise<OrderDto> {
  const res = await fetch(`${API_BASE}/api/v1/payments/orders/${orderId}/status`, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${accessToken}`
    }
  });
  return handleResponse<OrderDto>(res);
}


export async function simulateDemoWebhook(orderId: string, accessToken: string): Promise<{ success: boolean; message?: string }> {
  const res = await fetch(`${API_BASE}/api/v1/payments/simulate-demo-webhook`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${accessToken}`
    },
    body: JSON.stringify({ orderId })
  });
  return handleResponse<{ success: boolean; message?: string }>(res);
}
