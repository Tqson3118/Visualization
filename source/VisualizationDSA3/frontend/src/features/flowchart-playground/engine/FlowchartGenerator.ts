import { parse } from '@babel/parser';

export class FlowchartGenerator {
  static generate(code: string): string {
    if (!code || !code.trim()) return 'graph TD;\n  Start([Bắt đầu]);';
    
    try {
      const ast = parse(code, { sourceType: 'script' });
      let mmd = 'graph TD;\n';
      let idCounter = 1;
      
      const genId = (prefix = 'N') => `${prefix}${idCounter++}`;
      
      const getSource = (node: any) => {
        if (!node || node.start == null || node.end == null) return '';
        return code.slice(node.start, node.end).replace(/"/g, "'").replace(/\n/g, ' ');
      };

      const cleanText = (text: string) => {
        if (!text) return '...';
        if (text.length > 40) return text.substring(0, 40) + '...';
        return text.replace(/[\[\]\(\)\{\}\;\"]/g, ' ').trim();
      };

      function walk(node: any): { entry: string, exits: string[] } {
        if (!node) return { entry: '', exits: [] };
        
        if (node.type === 'File') return walk(node.program);
        if (node.type === 'Program' || node.type === 'BlockStatement') {
          const stmts = node.body || [];
          if (stmts.length === 0) {
             const id = genId('Empty');
             mmd += `  ${id}([ ... ])\n`;
             return { entry: id, exits: [id] };
          }
          
          let firstId = '';
          let currentExits: string[] = [];
          
          for (let i = 0; i < stmts.length; i++) {
             const result = walk(stmts[i]);
             if (!result.entry) continue;
             if (!firstId) firstId = result.entry;
             currentExits.forEach(ex => { mmd += `  ${ex} --> ${result.entry}\n`; });
             currentExits = result.exits;
          }
          return { entry: firstId || genId('NoOp'), exits: currentExits };
        }
        
        if (node.type === 'VariableDeclaration' || node.type === 'ExpressionStatement' || node.type === 'ReturnStatement') {
          const id = genId('Stmt');
          const text = cleanText(getSource(node));
          mmd += `  ${id}["${text}"]\n`;
          return { entry: id, exits: [id] };
        }
        
        if (node.type === 'IfStatement') {
          const id = genId('If');
          const cond = cleanText(getSource(node.test));
          mmd += `  ${id}{{"${cond} ?"}}\n`;
          
          const consequent = walk(node.consequent);
          mmd += `  ${id} -- Đúng --> ${consequent.entry}\n`;
          
          let exits = [...consequent.exits];
          
          if (node.alternate) {
             const alternate = walk(node.alternate);
             mmd += `  ${id} -- Sai --> ${alternate.entry}\n`;
             exits.push(...alternate.exits);
          } else {
             exits.push(id);
          }
          return { entry: id, exits: exits };
        }
        
        if (node.type === 'ForStatement' || node.type === 'WhileStatement') {
          const loopInit = node.type === 'ForStatement' && node.init ? walk(node.init) : null;
          
          const id = genId('Loop');
          const cond = cleanText(node.test ? getSource(node.test) : 'Lặp');
          mmd += `  ${id}{{"${cond} ?"}}\n`;
          
          const body = walk(node.body);
          
          let updateId = '';
          if (node.type === 'ForStatement' && node.update) {
            updateId = genId('Update');
            const updateText = cleanText(getSource(node.update));
            mmd += `  ${updateId}["${updateText}"]\n`;
          }
          
          mmd += `  ${id} -- Đúng --> ${body.entry}\n`;
          body.exits.forEach(ex => {
             if (updateId) {
                mmd += `  ${ex} --> ${updateId}\n`;
                mmd += `  ${updateId} --> ${id}\n`;
             } else {
                mmd += `  ${ex} --> ${id}\n`;
             }
          });
          
          let entry = id;
          if (loopInit) {
             entry = loopInit.entry;
             loopInit.exits.forEach(ex => { mmd += `  ${ex} --> ${id}\n`; });
          }
          
          return { entry: entry, exits: [id] };
        }
        
        const id = genId('Unknown');
        mmd += `  ${id}["[Khối lệnh phức tạp]"]\n`;
        return { entry: id, exits: [id] };
      }
      
      const res = walk(ast);
      
      mmd += `  Start([Bắt đầu]) --> ${res.entry || genId('NoOp')}\n`;
      const endId = genId('End');
      mmd += `  ${endId}([Kết thúc])\n`;
      res.exits.forEach(ex => { mmd += `  ${ex} -. Thoát .-> ${endId}\n`; });
      
      return mmd;
    } catch (e) {
       return 'graph TD;\n  Typing([Đang phân tích cú pháp... Vui lòng gõ tiếp]);';
    }
  }
}
