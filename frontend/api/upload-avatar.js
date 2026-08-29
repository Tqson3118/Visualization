export const config = {
  api: {
    bodyParser: {
      sizeLimit: '4mb',
    },
  },
};

export default async function handler(req, res) {
  if (req.method !== 'POST') {
    return res.status(405).json({ error: 'Method Not Allowed' });
  }

  try {
    let base64 = req.body?.image;
    let fileName = req.body?.name || 'avatar.png';

    if (!base64) {
      return res.status(400).json({ error: 'Thiếu dữ liệu ảnh.' });
    }

    if (base64.includes(';base64,')) {
      base64 = base64.split(';base64,')[1];
    }

    const buffer = Buffer.from(base64, 'base64');
    if (buffer.length > 3 * 1024 * 1024) {
      return res.status(400).json({ error: 'Kích thước ảnh không được vượt quá 3MB.' });
    }

    // 1. Try Catbox (Permanent CDN)
    try {
      const fd = new FormData();
      fd.append('reqtype', 'fileupload');
      fd.append('fileToUpload', new Blob([buffer], { type: 'image/png' }), fileName);

      const catboxRes = await fetch('https://catbox.moe/user/api.php', {
        method: 'POST',
        body: fd,
      });

      const url = (await catboxRes.text()).trim();
      if (url.startsWith('http')) {
        return res.status(200).json({ success: true, url });
      }
    } catch {
      // Fallback
    }

    // 2. Fallback to Litterbox
    const fd2 = new FormData();
    fd2.append('reqtype', 'fileupload');
    fd2.append('time', '72h');
    fd2.append('fileToUpload', new Blob([buffer], { type: 'image/png' }), fileName);
    const litterRes = await fetch('https://litterbox.catbox.moe/resources/internals/api.php', {
      method: 'POST',
      body: fd2,
    });
    const litterUrl = (await litterRes.text()).trim();
    if (litterUrl.startsWith('http')) {
      return res.status(200).json({ success: true, url: litterUrl });
    }

    throw new Error('Không thể tải ảnh lên máy chủ lưu trữ.');
  } catch (err) {
    return res.status(500).json({ error: err?.message || 'Lỗi xử lý ảnh.' });
  }
}
