import {createRequire} from 'node:module';
import {readFile} from 'node:fs/promises';
import assert from 'node:assert/strict';
const require=createRequire(import.meta.url);const {chromium}=require('playwright');
const browser=await chromium.launch({executablePath:process.env.CHROMIUM_PATH||'/usr/bin/chromium',args:['--no-sandbox']});
try{
 const page=await browser.newPage({viewport:{width:375,height:812},isMobile:true,hasTouch:true});const errors=[];page.on('pageerror',e=>errors.push(e.message));await page.setContent(await readFile(new URL('../dist/Cyklusmatch.html',import.meta.url),'utf8'));await page.locator('#play-cyklusmatch').click();await page.waitForSelector('.tile');
 const initial=await page.locator('.tile').allTextContents();
 const pairs=await page.evaluate(()=>{const b=[...document.querySelectorAll('.tile')].map(t=>t.textContent);const hit=()=>{for(let i=0;i<49;i++)if((i%7<5&&new Set([b[i],b[i+1],b[i+2]].filter(v=>v!=='⭐')).size<=1)||(i<35&&new Set([b[i],b[i+7],b[i+14]].filter(v=>v!=='⭐')).size<=1))return true;return false;};const result={};for(let i=0;i<49;i++)for(const j of [i+1,i+7]){if(j>=49||(j===i+1&&i%7===6))continue;[b[i],b[j]]=[b[j],b[i]];const valid=hit();[b[i],b[j]]=[b[j],b[i]];if(valid||b[i]!==b[j])result[valid?'valid':'invalid']??=[i,j];}return result;});
 await page.locator('.tile').nth(0).tap();await page.locator('.tile').nth(48).tap();assert.equal(await page.locator('.tile.selected').getAttribute('aria-label'),await page.locator('.tile').nth(0).getAttribute('aria-label'));assert.deepEqual(await page.locator('.tile').allTextContents(),initial);await page.locator('.tile').nth(0).tap();
 for(const i of pairs.invalid)await page.locator('.tile').nth(i).tap();await page.waitForFunction(()=>!document.querySelector('.tile:disabled'));const expected=[...initial];const [a,b]=pairs.invalid;[expected[a],expected[b]]=[expected[b],expected[a]];assert.deepEqual(await page.locator('.tile').allTextContents(),expected);assert.equal(await page.locator('#moves').textContent(),'27');assert.equal(await page.locator('#score').textContent(),'-20');assert.match(await page.locator('#status').textContent(),/−20 point/);
 await page.locator('#restart').tap();await page.locator('#dialog-action').tap();
 const current=await page.evaluate(async()=>{return [...document.querySelectorAll('.tile')].map(t=>t.textContent);});
 const valid=(()=>{const hit=()=>current.some((v,i)=>(i%7<5&&new Set([v,current[i+1],current[i+2]].filter(v=>v!=='⭐')).size<=1)||(i<35&&new Set([v,current[i+7],current[i+14]].filter(v=>v!=='⭐')).size<=1));for(let i=0;i<49;i++)for(const j of [i+1,i+7]){if(j>=49||(j===i+1&&i%7===6))continue;[current[i],current[j]]=[current[j],current[i]];const yes=hit();[current[i],current[j]]=[current[j],current[i]];if(yes)return [i,j];}})();
 for(const i of valid)await page.locator('.tile').nth(i).tap();await page.waitForFunction(()=>!document.querySelector('.tile:disabled'));assert.equal(await page.locator('#moves').textContent(),'27');assert.notEqual(await page.locator('#score').textContent(),'0');
 assert.deepEqual(errors,[]);console.log('PASS: touch selection, non-neighbor selection retained, non-match swap stays in place, deducts 20 points and uses one move, valid swap awards points and uses one move.');
}finally{await browser.close();}
