import http from 'node:http';
import { readFile } from 'node:fs/promises';
const files = {'/':'index.html','/style.css':'style.css','/game.js':'game.js','/engine.js':'engine.js','/profile.js':'profile.js'};
http.createServer(async (req,res)=>{
 const file=files[new URL(req.url,'http://localhost').pathname];
 if(!file){res.writeHead(404);res.end('Ikke fundet');return;}
 try {const body=await readFile(new URL(file,import.meta.url));res.setHeader('Content-Type',file.endsWith('.css')?'text/css':file.endsWith('.js')?'text/javascript':'text/html; charset=utf-8');res.end(body);}catch{res.writeHead(500);res.end('Kunne ikke læse filen');}
}).listen(Number(process.env.PORT)||3000,'0.0.0.0',()=>console.log('Cyklusmatch server klar'));
