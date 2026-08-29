import net from 'net';
import { appendFileSync, writeFileSync, existsSync } from 'fs';
try { writeFileSync('smtp-cmd.log', ''); } catch {}
try { writeFileSync('smtp-mails.log', ''); } catch {}
const save = s => { try { appendFileSync('smtp-mails.log', JSON.stringify(s) + '\n'); } catch {} };
const log = s => { try { appendFileSync('smtp-cmd.log', s + '\n'); } catch {} };
const server = net.createServer(sock => {
  let buf = '', inData = false, mail = null;
  const reply = s => { log('> ' + s); sock.write(s + '\r\n'); };
  reply('220 catcher ESMTP ready');
  sock.on('error', () => {});
  sock.on('data', d => {
    buf += d.toString('utf8');
    let idx;
    while ((idx = buf.indexOf('\r\n')) >= 0) {
      const line = buf.slice(0, idx); buf = buf.slice(idx + 2);
      log('< ' + line);
      if (inData) { if (line === '.') { inData = false; reply('250 OK queued'); if (mail) save(mail); mail = null; } else if (mail) mail.data += line + '\n'; continue; }
      const u = line.toUpperCase();
      if (u.startsWith('EHLO') || u.startsWith('HELO')) { sock.write('250-catcher\r\n250 8BITMIME\r\n'); }
      else if (u.startsWith('MAIL FROM')) { mail = { from: line, to: [], data: '' }; reply('250 OK'); }
      else if (u.startsWith('RCPT TO')) { if (mail) mail.to.push(line); reply('250 OK'); }
      else if (u.startsWith('DATA')) { inData = true; reply('354 go ahead'); }
      else if (u.startsWith('QUIT')) { reply('221 bye'); sock.end(); }
      else reply('250 OK');
    }
  });
});
server.on('error', () => {});
server.listen(1025, () => console.log('catcher v2 on :1025'));
