using System.Net;

namespace DsaVisual.Application.Services;

/// <summary>
/// Trình tạo nội dung HTML email chuẩn UI/UX sang trọng (Cyberpunk / Cosmic dark theme)
/// đồng bộ nhận diện thương hiệu DSA Visual.
/// </summary>
public static class EmailTemplateService
{
    public static string BuildRegisterOtpEmail(string code, int lifetimeMinutes = 5)
    {
        var encodedCode = WebUtility.HtmlEncode(code);
        return $@"<!DOCTYPE html>
<html lang=""vi"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Mã xác thực đăng ký — DSA Visual</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #0b0a12; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; color: #e2e8f0; -webkit-font-smoothing: antialiased;"">
  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #0b0a12; padding: 40px 16px;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 540px; background: linear-gradient(180deg, #161426 0%, #0f0e1a 100%); border: 1px solid #2d2948; border-radius: 20px; box-shadow: 0 20px 50px rgba(0, 0, 0, 0.6), 0 0 30px rgba(168, 85, 247, 0.12); overflow: hidden;"">
          
          <tr>
            <td style=""height: 4px; background: linear-gradient(90deg, #a855f7, #38bdf8, #ec4899);""></td>
          </tr>

          <tr>
            <td style=""padding: 36px 40px 24px 40px; text-align: center;"">
              <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                <tr>
                  <td align=""center"">
                    <div style=""display: inline-block; padding: 10px 18px; background: rgba(168, 85, 247, 0.12); border: 1px solid rgba(168, 85, 247, 0.3); border-radius: 12px; margin-bottom: 12px;"">
                      <span style=""font-size: 18px; font-weight: 900; letter-spacing: 2px; color: #ffffff; text-transform: uppercase;"">
                        ✨ DSA <span style=""color: #c084fc;"">VISUAL</span>
                      </span>
                    </div>
                    <p style=""margin: 0; font-size: 12px; color: #94a3b8; font-weight: 500; letter-spacing: 0.5px;"">
                      Nền Tảng Trực Quan Hóa Cấu Trúc Dữ Liệu & Giải Thuật
                    </p>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <tr>
            <td style=""padding: 0 40px 32px 40px;"">
              <h1 style=""margin: 0 0 12px 0; font-size: 22px; font-weight: 800; color: #ffffff; text-align: center; letter-spacing: -0.5px;"">
                Xác Thực Đăng Ký Tài Khoản
              </h1>
              <p style=""margin: 0 0 28px 0; font-size: 14px; line-height: 1.6; color: #cbd5e1; text-align: center;"">
                Chào mừng bạn đến với <strong>DSA Visual</strong>! Sử dụng mã OTP bảo mật dưới đây để hoàn tất quá trình tạo tài khoản của bạn.
              </p>

              <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""margin-bottom: 24px;"">
                <tr>
                  <td align=""center"">
                    <div style=""background: #090812; border: 1px solid #3b3558; border-radius: 16px; padding: 24px 20px; text-align: center; box-shadow: inset 0 2px 8px rgba(0, 0, 0, 0.6);"">
                      <div style=""font-size: 11px; font-weight: 700; color: #a855f7; text-transform: uppercase; letter-spacing: 1.5px; margin-bottom: 8px;"">
                        MÃ XÁC THỰC CỦA BẠN (OTP)
                      </div>
                      <div style=""font-family: 'SF Mono', Monaco, Consolas, 'Courier New', monospace; font-size: 38px; font-weight: 900; letter-spacing: 10px; color: #38bdf8; text-indent: 10px; text-shadow: 0 0 16px rgba(56, 189, 248, 0.45); margin: 6px 0;"">
                        {encodedCode}
                      </div>
                      <div style=""font-size: 12px; color: #64748b; margin-top: 6px;"">
                        Nhập mã này vào biểu mẫu đăng ký
                      </div>
                    </div>
                  </td>
                </tr>
              </table>

              <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""margin-bottom: 24px;"">
                <tr>
                  <td width=""50%"" style=""padding-right: 6px;"">
                    <div style=""background: rgba(255, 255, 255, 0.03); border: 1px solid rgba(255, 255, 255, 0.08); border-radius: 10px; padding: 10px 12px; text-align: center;"">
                      <span style=""font-size: 11px; color: #94a3b8; display: block;"">⏱️ Hiệu lực</span>
                      <strong style=""font-size: 13px; color: #f8fafc;"">{lifetimeMinutes} Phút</strong>
                    </div>
                  </td>
                  <td width=""50%"" style=""padding-left: 6px;"">
                    <div style=""background: rgba(255, 255, 255, 0.03); border: 1px solid rgba(255, 255, 255, 0.08); border-radius: 10px; padding: 10px 12px; text-align: center;"">
                      <span style=""font-size: 11px; color: #94a3b8; display: block;"">🔒 Sử dụng</span>
                      <strong style=""font-size: 13px; color: #f8fafc;"">1 lần duy nhất</strong>
                    </div>
                  </td>
                </tr>
              </table>

              <div style=""background: rgba(239, 68, 68, 0.08); border: 1px solid rgba(239, 68, 68, 0.25); border-radius: 12px; padding: 14px 16px;"">
                <p style=""margin: 0; font-size: 12px; line-height: 1.5; color: #fca5a5;"">
                  <strong>⚠️ Cảnh báo bảo mật:</strong> Tuyệt đối không chia sẻ mã này với bất kỳ ai. Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.
                </p>
              </div>
            </td>
          </tr>

          <tr>
            <td style=""background: #0d0c15; padding: 24px 40px; border-top: 1px solid #1e1b2e; text-align: center;"">
              <p style=""margin: 0 0 6px 0; font-size: 11px; color: #64748b;"">
                Email tự động được gửi từ hệ thống <strong>DSA Visual Platform</strong>.
              </p>
              <p style=""margin: 0; font-size: 11px; color: #475569;"">
                © 2026 DSA Visual. All rights reserved.
              </p>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    public static string Build2FaOtpEmail(string code, int lifetimeMinutes = 5)
    {
        var encodedCode = WebUtility.HtmlEncode(code);
        return $@"<!DOCTYPE html>
<html lang=""vi"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Mã xác thực 2FA — DSA Visual</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #0b0a12; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; color: #e2e8f0; -webkit-font-smoothing: antialiased;"">
  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #0b0a12; padding: 40px 16px;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 540px; background: linear-gradient(180deg, #161426 0%, #0f0e1a 100%); border: 1px solid #2d2948; border-radius: 20px; box-shadow: 0 20px 50px rgba(0, 0, 0, 0.6), 0 0 30px rgba(56, 189, 248, 0.12); overflow: hidden;"">
          
          <tr>
            <td style=""height: 4px; background: linear-gradient(90deg, #38bdf8, #a855f7, #6366f1);""></td>
          </tr>

          <tr>
            <td style=""padding: 36px 40px 24px 40px; text-align: center;"">
              <div style=""display: inline-block; padding: 10px 18px; background: rgba(56, 189, 248, 0.12); border: 1px solid rgba(56, 189, 248, 0.3); border-radius: 12px; margin-bottom: 12px;"">
                <span style=""font-size: 18px; font-weight: 900; letter-spacing: 2px; color: #ffffff; text-transform: uppercase;"">
                  🛡️ DSA <span style=""color: #38bdf8;"">SECURITY</span>
                </span>
              </div>
              <p style=""margin: 0; font-size: 12px; color: #94a3b8; font-weight: 500;"">
                Hệ Thống Xác Thực Hai Lớp (2FA)
              </p>
            </td>
          </tr>

          <tr>
            <td style=""padding: 0 40px 32px 40px;"">
              <h1 style=""margin: 0 0 12px 0; font-size: 22px; font-weight: 800; color: #ffffff; text-align: center;"">
                Mã Xác Thực Bảo Mật 2FA
              </h1>
              <p style=""margin: 0 0 28px 0; font-size: 14px; line-height: 1.6; color: #cbd5e1; text-align: center;"">
                Yêu cầu đăng nhập hoặc kích hoạt bảo mật đang chờ xác nhận. Vui lòng nhập mã bên dưới để tiếp tục:
              </p>

              <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""margin-bottom: 24px;"">
                <tr>
                  <td align=""center"">
                    <div style=""background: #090812; border: 1px solid #3b3558; border-radius: 16px; padding: 24px 20px; text-align: center; box-shadow: inset 0 2px 8px rgba(0, 0, 0, 0.6);"">
                      <div style=""font-size: 11px; font-weight: 700; color: #38bdf8; text-transform: uppercase; letter-spacing: 1.5px; margin-bottom: 8px;"">
                        MÃ 2FA CỦA BẠN
                      </div>
                      <div style=""font-family: 'SF Mono', Monaco, Consolas, monospace; font-size: 38px; font-weight: 900; letter-spacing: 10px; color: #a855f7; text-indent: 10px; text-shadow: 0 0 16px rgba(168, 85, 247, 0.45); margin: 6px 0;"">
                        {encodedCode}
                      </div>
                      <div style=""font-size: 12px; color: #64748b; margin-top: 6px;"">
                        Mã có hiệu lực trong {lifetimeMinutes} phút
                      </div>
                    </div>
                  </td>
                </tr>
              </table>

              <div style=""background: rgba(239, 68, 68, 0.08); border: 1px solid rgba(239, 68, 68, 0.25); border-radius: 12px; padding: 14px 16px;"">
                <p style=""margin: 0; font-size: 12px; line-height: 1.5; color: #fca5a5;"">
                  <strong>⚠️ Bảo mật tài khoản:</strong> Nếu bạn không thực hiện yêu cầu này, tài khoản của bạn có thể đang bị truy cập trái phép. Hãy đổi mật khẩu ngay lập tức!
                </p>
              </div>
            </td>
          </tr>

          <tr>
            <td style=""background: #0d0c15; padding: 24px 40px; border-top: 1px solid #1e1b2e; text-align: center;"">
              <p style=""margin: 0; font-size: 11px; color: #64748b;"">
                © 2026 DSA Visual Platform. All rights reserved.
              </p>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    public static string BuildPasswordResetEmail(string resetLink, int lifetimeMinutes = 30)
    {
        var safeLink = WebUtility.HtmlEncode(resetLink);
        return $@"<!DOCTYPE html>
<html lang=""vi"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Đặt lại mật khẩu — DSA Visual</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #0b0a12; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; color: #e2e8f0; -webkit-font-smoothing: antialiased;"">
  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #0b0a12; padding: 40px 16px;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 540px; background: linear-gradient(180deg, #161426 0%, #0f0e1a 100%); border: 1px solid #2d2948; border-radius: 20px; box-shadow: 0 20px 50px rgba(0, 0, 0, 0.6), 0 0 30px rgba(168, 85, 247, 0.12); overflow: hidden;"">
          
          <tr>
            <td style=""height: 4px; background: linear-gradient(90deg, #f59e0b, #ec4899, #a855f7);""></td>
          </tr>

          <tr>
            <td style=""padding: 36px 40px 24px 40px; text-align: center;"">
              <div style=""display: inline-block; padding: 10px 18px; background: rgba(168, 85, 247, 0.12); border: 1px solid rgba(168, 85, 247, 0.3); border-radius: 12px; margin-bottom: 12px;"">
                <span style=""font-size: 18px; font-weight: 900; letter-spacing: 2px; color: #ffffff; text-transform: uppercase;"">
                  🔑 DSA <span style=""color: #c084fc;"">VISUAL</span>
                </span>
              </div>
            </td>
          </tr>

          <tr>
            <td style=""padding: 0 40px 32px 40px; text-align: center;"">
              <h1 style=""margin: 0 0 12px 0; font-size: 22px; font-weight: 800; color: #ffffff;"">
                Yêu Cầu Đặt Lại Mật Khẩu
              </h1>
              <p style=""margin: 0 0 28px 0; font-size: 14px; line-height: 1.6; color: #cbd5e1;"">
                Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản DSA Visual của bạn. Nhấn vào nút bên dưới để thiết lập mật khẩu mới:
              </p>

              <div style=""margin: 32px 0;"">
                <a href=""{safeLink}"" target=""_blank"" style=""display: inline-block; background: linear-gradient(135deg, #a855f7 0%, #7c3aed 100%); color: #ffffff; font-size: 15px; font-weight: 700; text-decoration: none; padding: 14px 32px; border-radius: 12px; box-shadow: 0 4px 20px rgba(168, 85, 247, 0.4); letter-spacing: 0.5px;"">
                  Đặt Lại Mật Khẩu Ngay →
                </a>
              </div>

              <p style=""margin: 0 0 20px 0; font-size: 12px; color: #94a3b8;"">
                Liên kết này có hiệu lực trong <strong>{lifetimeMinutes} phút</strong> và chỉ sử dụng được 1 lần.
              </p>

              <div style=""background: rgba(255, 255, 255, 0.03); border: 1px solid rgba(255, 255, 255, 0.08); border-radius: 10px; padding: 12px; margin-top: 20px; text-align: left; word-break: break-all;"">
                <p style=""margin: 0 0 4px 0; font-size: 11px; color: #64748b;"">Hoặc sao chép đường dẫn này vào trình duyệt:</p>
                <a href=""{safeLink}"" style=""font-size: 11px; color: #38bdf8; text-decoration: none;"">{safeLink}</a>
              </div>
            </td>
          </tr>

          <tr>
            <td style=""background: #0d0c15; padding: 24px 40px; border-top: 1px solid #1e1b2e; text-align: center;"">
              <p style=""margin: 0; font-size: 11px; color: #64748b;"">
                © 2026 DSA Visual Platform. All rights reserved.
              </p>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }
}
