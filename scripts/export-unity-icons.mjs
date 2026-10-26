// Development-only asset exporter; generated PNGs are committed for Unity users.
import {createRequire} from 'node:module';
import {readFile,mkdir} from 'node:fs/promises';
const require=createRequire(import.meta.url);
const {chromium}=require('playwright');
const html=await readFile(new URL('../index.html',import.meta.url),'utf8');
const symbols=new Map([...html.matchAll(/<symbol id="([^"]+)" viewBox="([^"]+)">([\s\S]*?)<\/symbol>/g)].map(m=>[m[1],{viewBox:m[2],body:m[3]}]));
const extra={
 drop:'<path d="M32 4C26 15 11 32 12 43a20 20 0 0 0 40 0C53 32 38 15 32 4Z" fill="#d94b72" stroke="#b7365c" stroke-width="2"/><path d="M22 32c-5 8-4 15 0 18" fill="none" stroke="#fa91aa" stroke-width="4" stroke-linecap="round"/>',
 moon:'<path d="M43 6A27 27 0 1 0 58 44C30 53 17 26 43 6Z" fill="#ebc271" stroke="#bd8f4e" stroke-width="2"/>',
 flower:'<g fill="#ed8da7" stroke="#c55e7d" stroke-width="1.5"><ellipse cx="32" cy="18" rx="10" ry="14"/><ellipse cx="32" cy="18" rx="10" ry="14" transform="rotate(72 32 32)"/><ellipse cx="32" cy="18" rx="10" ry="14" transform="rotate(144 32 32)"/><ellipse cx="32" cy="18" rx="10" ry="14" transform="rotate(216 32 32)"/><ellipse cx="32" cy="18" rx="10" ry="14" transform="rotate(288 32 32)"/></g><circle cx="32" cy="32" r="8" fill="#f8d897"/>',
 star:'<path d="m32 4 8 17 19 3-14 14 3 20-16-9-17 9 4-20L5 24l19-3Z" fill="#ecc549" stroke="#bd8e25" stroke-width="2" stroke-linejoin="round"/>'
};
for(const [id,body] of Object.entries(extra))symbols.set(id,{viewBox:'0 0 64 64',body});
const icons={drop:'drop',moon:'moon',flower:'flower',tampon:'tampon-icon',pad:'pad-icon',cup:'cup-icon',star:'star',cat:'cat-avatar',crown:'star-crown'};
const browser=await chromium.launch({executablePath:process.env.CHROMIUM_PATH||'/usr/bin/chromium',args:['--no-sandbox']});
try{const page=await browser.newPage({viewport:{width:128,height:128},deviceScaleFactor:2});
await mkdir(new URL('../Assets/RedMoonGames/Resources/Icons/',import.meta.url),{recursive:true});
for(const [name,id] of Object.entries(icons)){const {body,viewBox}=symbols.get(id);await page.setContent(`<html><body style="margin:0;background:transparent"><svg xmlns="http://www.w3.org/2000/svg" width="128" height="128" viewBox="${viewBox}">${body}</svg></body></html>`);await page.locator('svg').screenshot({path:new URL(`../Assets/RedMoonGames/Resources/Icons/${name}.png`,import.meta.url).pathname,omitBackground:true});}
console.log('Exported nine transparent Unity icons.');}finally{await browser.close();}
