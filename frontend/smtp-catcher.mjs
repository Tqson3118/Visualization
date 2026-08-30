// SMTP catcher — lưu toàn bộ mail vào smtp-mails.log
import net from 'net';
import { appendFileSync } from 'fs';
const MAILS = [];
const server = net.createServer(sock => {
  let buf = '', inData = false, mail = { from: '', to: [], data: '' };
  sock.write('220 smtp-catcher ESMTP\r\n');
  sock.on('data', d => {
    buf += d.toString('utf8');
    let idx;
    while ((idx = buf.indexOf('\r\n')) >= 0) {
      const line = buf.slice(0, idx); buf = buf.slice(idx + 2);
      if (inData) {
        if (line === '.') { inData = false; sock.write('250 OK\r\n'); MAILS.push(mail); appendFileSync('smtp-mails.log', JSON.stringify(mail) + '\n'); mail = { from: '', to: [], data: '' }; }
        else mail.data += line + '\n';
        continue;
      }
      const cmd = line.toUpperCase();
      if (cmd.startsWith('EHLO') || cmd.startsWith('HELO')) sock.write('250 smtp-catcher\r\n');
      else if (cmd.startsWith('MAIL FROM')) { mail.from = line; sock.write('250 OK\r\n'); }
      else if (cmd.startsWith('RCPT TO')) { mail.to.push(line); sock.write('250 OK\r\n'); }
      else if (cmd.startsWith('DATA')) { inData = true; sock.write('354 End data with <CR><LF>.<CR><LF>\r\n'); }
      else if (cmd.startsWith('QUIT')) { sock.write('221 Bye\r\n'); sock.end(); }
      else sock.write('250 OK\r\n');
    }
  });
});
server.listen(1025, () => console.log('SMTP catcher on :1025'));
