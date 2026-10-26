import {writeFile} from 'node:fs/promises';
import {matchScore,matches,resolveMatch,activeStarsForMove} from '../engine.js';
const fixtures=[];
const patterned=()=>Array.from({length:49},(_,i)=>(i+Math.floor(i/7))%6);
function add(name,board,active=[]){const clone=[...board];const stars=resolveMatch(clone,[],()=>0,active);fixtures.push({name,board,active,points:matchScore(board,1,active),cells:matches(board,active).sort((a,b)=>a-b),newStars:stars,remainingStars:clone.filter(v=>v===6).length});}
for(const step of [1,7])for(let length=3;length<=7;length++){const b=patterned();const value=length<7?(b[length*step]+1)%6:2;for(let i=0;i<length;i++)b[i*step]=value;add(`natural-${length}-${step}`,b);}
for(const shape of [[0,7,14,15,16],[1,8,14,15,16]]){const b=patterned();for(const i of shape)b[i]=5;add('corner-'+shape.join('-'),b);}
for(let type=0;type<6;type++){const b=patterned();b[0]=b[2]=type;b[1]=6;add('passive-'+type,b);add('active-'+type,b,[1]);}
for(const length of [3,4,5]){const b=patterned();for(let i=0;i<length;i++)b[i]=6;add('star-jackpot-'+length,b,activeStarsForMove(b,[1,8]));}
await writeFile(new URL('../tests/unity-core/fixtures.json',import.meta.url),JSON.stringify(fixtures,null,2)+'\n');console.log(`Exported ${fixtures.length} JavaScript rule fixtures for C# parity checks.`);
