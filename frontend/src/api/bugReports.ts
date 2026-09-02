import { client } from './client';

/**
 * BugReport phía người dùng — POST /bug-reports (BugReportsController, yêu cầu đăng nhập).
 * Dùng chung cho form /help (C7): Description = nội dung, Context = JSON phụ (name/email/page).
 */
export interface BugReportCreatePayload {
  description: string;
  context?: string;
}

export async function createBugReport(payload: BugReportCreatePayload): Promise<{ id: number }> {
  const res = await client.post('/bug-reports', payload);
  return res.data;
}
