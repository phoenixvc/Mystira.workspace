const fs = require('fs');
const path = require('path');

function walk(dir) {
  let results = [];
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      results = results.concat(walk(full));
    } else if (entry.name.endsWith('.cs')) {
      results.push(full);
    }
  }
  return results;
}

const root = path.join(__dirname, '..', 'apps', 'app');
let count1 = 0;
let count2 = 0;

for (const f of walk(root)) {
  let content = fs.readFileSync(f, 'utf8');
  let original = content;

  if (content.includes('using Mystira.App.Application.Ports.Data;')) {
    content = content.replace(/using Mystira\.App\.Application\.Ports\.Data;/g, 'using Mystira.Application.Ports.Data;');
    count1++;
  }

  if (content.includes('using Mystira.Shared.Data.Repositories;')) {
    content = content.replace(/using Mystira\.Shared\.Data\.Repositories;/g, 'using Mystira.Application.Ports.Data;');
    count2++;
  }

  if (content !== original) {
    fs.writeFileSync(f, content);
  }
}

console.log(`Replaced Mystira.App.Application.Ports.Data in ${count1} files`);
console.log(`Replaced Mystira.Shared.Data.Repositories in ${count2} files`);
