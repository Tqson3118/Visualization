
const CATALOG = [
  'sort.bubble', 'sort.selection', 'sort.insertion', 'sort.merge', 'sort.quick', 'sort.heap',
  'search.binary', 'search.linear',
  'linear.singly-linked-list', 'linear.doubly-linked-list', 'linear.circular-linked-list', 'linear.skip-list',
  'linear.dynamic-array', 'linear.stack-array', 'linear.stack-linked-list', 'linear.queue-array', 'linear.queue-linked-list',
  'tree.binary-search-tree', 'tree.avl', 'tree.red-black', 'tree.b-tree', 'tree.trie', 'tree.segment-tree', 'tree.fenwick', 'tree.suffix-tree',
  'heap.min-max-heap', 'heap.fibonacci', 'heap.binomial',
  'hash.chaining', 'hash.open-addressing', 'hash.cuckoo',
  'graph.bfs', 'graph.dfs', 'graph.dijkstra',
  'structure.stack', 'structure.queue', 'structure.linked-list', 'structure.doubly-linked-list',
  'structure.bst', 'structure.avl', 'structure.min-heap', 'structure.hash-table',
  'structure.graph-directed', 'structure.trie'
];

const DEMO_ALLOWED = ['sort.bubble', 'search.binary', 'graph.bfs'];

console.log('Total catalog algorithms:', CATALOG.length);
console.log('Demo allowed algorithms:', DEMO_ALLOWED.length);
