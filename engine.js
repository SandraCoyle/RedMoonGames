export const SIZE=7;
export const TYPES=6;
export const STAR=6;
export function starLines(board,activeStars=[]){
 const active=new Set(activeStars),lines=[];
 for(const step of [1,SIZE])for(let lane=0;lane<SIZE;lane++){
  let run=[];const flush=()=>{if(run.length>=3)lines.push(run);run=[];};
  for(let offset=0;offset<SIZE;offset++){
   const i=step===1?lane*SIZE+offset:offset*SIZE+lane;
   if(board[i]===STAR&&active.has(i))run.push(i);else flush();
  }flush();
 }
 return lines;
}
export function matchLines(board,activeStars=[]){
 const active=new Set(activeStars);
 const lines=[];
 const values=[...new Set(board.filter(v=>v!=null&&v!==STAR))];
 for(const step of [1,SIZE])for(let lane=0;lane<SIZE;lane++)for(const value of values){
  let run=[];
  const flush=()=>{if(run.length>=3&&run.some(i=>board[i]===value))lines.push([...run]);run=[];};
  for(let offset=0;offset<SIZE;offset++){
   const i=step===1?lane*SIZE+offset:offset*SIZE+lane;
   if(board[i]===value||(board[i]===STAR&&active.has(i)))run.push(i);else flush();
  }flush();
 }
 lines.push(...starLines(board,activeStars));
 // Deduplicate overlapping candidate runs.
 return lines.filter((line,index)=>!lines.some((other,j)=>j!==index&&other.length>=line.length&&line.every(i=>other.includes(i))&&(other.length>line.length||j<index)));
}
export function matches(board,activeStars=[]){return [...new Set(matchLines(board,activeStars).flat())];}
export function adjacent(a,b){return a!==b&&Math.abs(Math.floor(a/SIZE)-Math.floor(b/SIZE))+Math.abs(a%SIZE-b%SIZE)===1;}
export function swap(board,a,b){[board[a],board[b]]=[board[b],board[a]];}
export function hasMove(board){for(let i=0;i<board.length;i++)for(const j of [i+1,i+SIZE]){if(j>=board.length||!adjacent(i,j))continue;swap(board,i,j);const hit=matches(board,activeStarsForMove(board,[i,j])).length;swap(board,i,j);if(hit)return true;}return false;}
export function createBoard(random=Math.random){let board;do{board=[];for(let i=0;i<SIZE*SIZE;i++){let choices=Array.from({length:TYPES},(_,k)=>k).filter(v=>!(i%SIZE>=2&&board[i-1]===v&&board[i-2]===v)&&!(i>=2*SIZE&&board[i-SIZE]===v&&board[i-2*SIZE]===v));board.push(choices[Math.floor(random()*choices.length)]);}}while(!hasMove(board));return board;}
export function refill(board,cleared,random=Math.random){for(const i of cleared)board[i]=null;for(let c=0;c<SIZE;c++){const col=[];for(let r=SIZE-1;r>=0;r--)if(board[r*SIZE+c]!=null)col.push(board[r*SIZE+c]);while(col.length<SIZE)col.push(Math.floor(random()*TYPES));for(let r=SIZE-1;r>=0;r--)board[r*SIZE+c]=col[SIZE-1-r];}}
export function activeStarsForMove(board,swapped){
 const stars=board.flatMap((v,i)=>v===STAR?[i]:[]);
 return [...new Set(matchLines(board,stars).filter(line=>line.some(i=>swapped.includes(i))).flat().filter(i=>board[i]===STAR))];
}
export function matchGroups(board,activeStars=[]){
 const groups=[];
 for(const line of matchLines(board,activeStars)){
  let joined=new Set(line);
  for(let i=groups.length-1;i>=0;i--)if(groups[i].some(k=>joined.has(k))){for(const k of groups[i])joined.add(k);groups.splice(i,1);}
  groups.push([...joined]);
 }
 return groups;
}
export function matchScore(board,chain=1,activeStars=[]){
 let points=starLines(board,activeStars).reduce((sum,line)=>sum+3000+(line.length-3)*1000,0);
 for(const group of matchGroups(board,activeStars)){
  if(group.every(i=>board[i]===STAR))continue;
  points+=group.length*20+(group.length>=5?200:group.length===4?40:0);
  if(group.some(i=>board[i]===STAR))points+=group.length>=5?300:group.length===4?120:0;
 }
 return points*chain;
}
export function resolveMatch(board,preferred=[],random=Math.random,activeStars=[]){
 const lines=matchGroups(board,activeStars),cleared=[...new Set(lines.flat())],stars=[];
 // Every five-in-a-row earns a joker, including an actively used joker.
 for(const line of lines){
  if(line.length<5)continue;
  const position=preferred.find(i=>line.includes(i)&&!stars.includes(i))??line.find(i=>!stars.includes(i));
  if(position!=null)stars.push(position);
 }
 for(const i of stars)board[i]=STAR;
 refill(board,cleared.filter(i=>!stars.includes(i)),random);
 return stars.length;
}
